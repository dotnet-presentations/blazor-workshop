# Authentication with Blazor

The application is working well. Users can place orders and track their order status. But there's one little problem: currently we don't distinguish between users at all. The "My orders" page lists *all* orders placed by *all* users, and anybody can view the state of anybody else's order. Your customers, and privacy regulations, may have an issue with this.

The solution is *authentication*. We need a way for users to log in, so we know who's who. Then we can implement *authorization*, which is to enforce rules about who's allowed to do what.

## Enforcement is on the server

The first and most important principle is that all *real* security rules must be enforced on the backend server. The client (UI) merely shows or hides options as a courtesy to well-behaved users, but a malicious user can always change the behavior of the client-side code.

As such, we're going to start by enforcing some access rules in the backend server, even before the client code knows about them.

Inside the `BlazingPizza` project, you'll find `OrdersController.cs`. This is the controller class that handles incoming HTTP requests for `/orders` and `/orders/{orderId}`. To require that all requests to these endpoints come from authenticated users (i.e., people who have logged in), add the `[Authorize]` attribute to the `OrdersController` class:

```csharp
[Route("orders")]
[ApiController]
[Authorize]
public class OrdersController : Controller
{
}
```

The `AuthorizeAttribute` class is located in the `Microsoft.AspNetCore.Authorization` namespace.

If you try to run your application now, you'll find that you can no longer place orders, nor can you retrieve details of orders already placed. Requests to these endpoints will return HTTP 401 "Not Authorized" responses, triggering an error message in the UI. That's good, because it shows that rules are being enforced on the server!

![Secure orders](https://user-images.githubusercontent.com/1101362/83876158-49ffef80-a730-11ea-8c86-f1fb2b51755b.png)

## Tracking authentication state

The client code needs a way to track whether the user is logged in, and if so *which* user is logged in, so it can influence how the UI behaves. Blazor has a built-in DI service for doing this: the `AuthenticationStateProvider`. Blazor provides an implementation of the `AuthenticationStateProvider` service and other related services and components based on [OpenID Connect](https://openid.net/connect/) that handle all the details of establishing who the user is. These services and components are provided in the Microsoft.AspNetCore.Components.WebAssembly.Authentication package, which has already been added to the client project for you.

In broad terms, the authentication process implemented by these services looks like this:

* When a user attempts to login or tries to access a protected resource, the user is redirected to the app's login page (`/authentication/login`).
* In the login page, the app prepares to redirect to the authorization endpoint of the configured identity provider. The endpoint is responsible for determining whether the user is authenticated and for issuing one or more tokens in response. The app provides a login callback to receive the authentication response.
  * If the user isn't authenticated, the user is first redirected to the underlying authentication system (typically ASP.NET Core Identity).
  * Once the user is authenticated, the authorization endpoint generates the appropriate tokens and redirects the browser back to the login callback endpoint (`/authentication/login-callback`).
* When the Blazor WebAssembly app loads the login callback endpoint (`/authentication/login-callback`), the authentication response is processed.
  * If the authentication process completes successfully, the user is authenticated and optionally sent back to the original protected URL that the user requested.
  * If the authentication process fails for any reason, the user is sent to the login failed page (`/authentication/login-failed`), and an error is displayed.

See also [Secure ASP.NET Core Blazor WebAssembly](https://docs.microsoft.com/aspnet/core/security/blazor/webassembly/) for additional details.

To enable the authentication services, add calls to `AddAuthorizationCore` and `AddCascadingAuthenticationState` in *Program.cs* in the (`BlazingPizza.Client`) client project:

```csharp
global using BlazingPizza.Shared;
global using BlazingPizza.Client;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure HttpClient to use the base address of the server project
builder.Services.AddScoped<HttpClient>(sp => 
	new HttpClient
	{
		BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
	});

// Add Security
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddScoped<IRepository, HttpRepository>();
builder.Services.AddScoped<OrderState>();

await builder.Build().RunAsync();
```

The added services will be configured by default to use an identity provider on the same origin as the app. The server project for the Blazing Pizza app has already been setup to use ASP.NET Core Identity for the authentication system:

*BlazingPizza/Program.cs*

```csharp
global using BlazingPizza.Shared;
global using BlazingPizza;
using BlazingPizza.Client;
using BlazingPizza.Components;
using BlazingPizza.Components.Account;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddRazorComponents()
		.AddInteractiveServerComponents()
		.AddInteractiveWebAssemblyComponents();

// Add Security
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

// Add Identity
builder.Services.AddIdentityCore<PizzaStoreUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<PizzaStoreContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddDbContext<PizzaStoreContext>(options =>
				options.UseSqlite("Data Source=pizza.db"));

builder.Services.AddSingleton<IEmailSender<PizzaStoreUser>, IdentityNoOpEmailSender>();

//more code below hidden for brevity
var app = builder.Build();
```

The files are available in [project checkpoint for Module 4](../modules/4-Authentication) 


Because we're using ASP.NET Identity, the server will issue an authentication cookie to the client app.

For ASP.NET Identity, we have all the related components to orchestrate the authentication flow in the *BlazingPizza/Components/Account* folder.

The ASP.NET Identity components will provide the pieces to allow user registration, profile setup, and user login/logout.

To enable flow the authentication state information through your app, the `AddCascadingAuthenticationState` method in the `Program.cs` file. Using the `AddCascadingAuthenticationState` method will enable `AuthenticationState` as a *cascading parameter* so it can be available to all descendant components. A cascading parameter is a parameter that isn't passed down just one level in the hierarchy, but through any number of levels.

Finally, you're ready to display something in the UI!

## Displaying login state

Create a new component called `LoginDisplay` in the main project's `Component` folder, containing:

```html
@implements IDisposable
@inject NavigationManager Navigation

<div class="user-info">
    <AuthorizeView>
        <Authorizing>
            <text>...</text>
        </Authorizing>
        <Authorized>
            <img src="img/user.svg" />
            <div>
                <a href="Account/Manage" class="username">@context.User.Identity.Name</a>
                <form action="Account/Logout" method="post">
                    <AntiforgeryToken />
                    <input type="hidden" name="ReturnUrl" value="@currentUrl" />
                    <button class="btn btn-link sign-out" type="submit">Sign out</button>
                </form>
            </div>
        </Authorized>
        <NotAuthorized>
            <a class="sign-in" href="Account/Register">Register</a>
            <a class="sign-in" href="Account/Login">Log in</a>
        </NotAuthorized>
    </AuthorizeView>
</div>

@code {
    private string? currentUrl;

    protected override void OnInitialized()
    {
        currentUrl = Navigation.ToBaseRelativePath(Navigation.Uri);
        Navigation.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        currentUrl = Navigation.ToBaseRelativePath(e.Location);
        StateHasChanged();
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
    }
}
```

`AuthorizeView` is a built-in component that displays different content depending on whether the user meets specified authorization conditions. We didn't specify any authorization conditions, so by default it considers the user authorized if they are authenticated (logged in), otherwise not authorized.

You can use `AuthorizeView` anywhere you need UI content to vary by authorization state, such as controlling the visibility of menu entries based on a user's roles. In this case, we're using it to tell the user who they are, and conditionally show either a "log in" or "log out" link as applicable.

The links to register, log in, and see the user profile are normal links that navigate to the ASP.NET Identity pages. The sign out link is a button that performs a form POST to the Account/Logout endpoint. This endpoint is defined in the `IdentityComponentsEndpointRouteBuilderExtensions` class to correctly perform signout on the user.

Let's put the `LoginDisplay` in the UI somewhere. Open `MainLayout`, and update the `<div class="top-bar">` as follows:

```html
<div class="top-bar">
    (... leave existing content in place ...)

    <LoginDisplay />
</div>
```

## Register a user and log in

Try it out now. Run the app and register a new user.

Select Register on the home page.

![Select register](https://user-images.githubusercontent.com/1874516/78322144-b25d0580-7522-11ea-863d-59083c2bf111.png)

Fill in an email address for the new user and a password.

![Register a new user](https://user-images.githubusercontent.com/1874516/78322197-e6d0c180-7522-11ea-8728-2bd9cbd3c8f8.png)

To compete the user registration, the user needs to confirm their email address. During development you can just click the link to confirm the account.

![Email confirmation](https://user-images.githubusercontent.com/1874516/78389880-62208a80-7598-11ea-945a-d2ced76133d9.png)

Once the user's email has been confirmed, select Login and enter the user's email address and password.

![Select login](https://user-images.githubusercontent.com/1874516/78389922-7bc1d200-7598-11ea-8a10-e8bf8efa512e.png)

![Login](https://user-images.githubusercontent.com/1874516/78390092-cc392f80-7598-11ea-9d8e-562c2be1aad6.png)

The user is logged in and redirected back to the home page.

![Logged in](https://user-images.githubusercontent.com/1874516/78390115-d9561e80-7598-11ea-912b-e9dd71f787f2.png)

## Enforcing login on specific pages

Now if you're logged in, you'll be able to place orders and see order status. But if you're not logged in and try to place an order, the flow isn't ideal. It doesn't ask you to log in until you *submit* the checkout form (because that's when the server responds 401 Not Authorized). What if you want to make certain pages require authorization, even before receiving 401 Not Authorized responses from the server?

You can do this quite easily. In the same way that you use the `[Authorize]` attribute in server-side code, you can use that attribute in client-side Blazor pages. Let's fix the checkout page so that you have to be logged in as soon as you get there, not just when you submit its form.

By default, all pages allow for anonymous access, but we can specify that the user must be logged in to access the checkout page by adding the `[Authorize]` attribute at the top of `Checkout.razor` client component:

```razor
@attribute [Authorize]
```

This attribute is part of `Microsoft.AspNetCore.Authorization` namespace, so you might be prompted to add the using reference to the file.

Next, to make the router respect such attributes, update *Routes.razor* to render an `AuthorizeRouteView` instead of a `RouteView` when the route is found.

```razor
@using BlazingPizza.Components.Account.Shared
@using Microsoft.AspNetCore.Components.Authorization
<Router AppAssembly="typeof(Program).Assembly" AdditionalAssemblies="new[] { typeof(Client.OrderState).Assembly }">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)">
            <NotAuthorized>
                <p>You are not authorized to access this resource.</p>
            </NotAuthorized>
            <Authorizing>
                <text>Authorizing...  Please wait</text>
            </Authorizing>
        </AuthorizeRouteView>
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

The `AuthorizeRouteView` will route navigation to the correct component, but only if the user is authorized. If the user is not authorized, the `NotAuthorized` content is displayed. You can also specify content to display while the `AuthorizeRouteView` is determining if the user is authorized.

Now when you try to navigate to the checkout page while signed out, you see the `NotAuthorized` content we setup in *App.razor*.

![Not authorized](https://user-images.githubusercontent.com/1874516/78410504-63b27880-75c1-11ea-8c2c-ab62c1c24596.png)

Instead of telling the user they are unauthorized it would be better if we redirected them to the login page. To do that, add the following `RedirectToLogin` component:

*BlazingPizza/Components/Account/Shared/RedirectToLogin.razor*

```razor
@inject NavigationManager NavigationManager

@code {
    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo($"Account/Login?returnUrl={Uri.EscapeDataString(NavigationManager.Uri)}", forceLoad: true);
    }
}
```

Then replace the `NotAuthorized` content in *Routes.razor* with the `RedirectToLogin` component.

```razor
@using BlazingPizza.Components.Account.Shared
@using Microsoft.AspNetCore.Components.Authorization
<Router AppAssembly="typeof(Program).Assembly" AdditionalAssemblies="new[] { typeof(Client.OrderState).Assembly }">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)">
            <NotAuthorized>
                <RedirectToLogin />
            </NotAuthorized>
            <Authorizing>
                <text>Authorizing...  Please wait</text>
            </Authorizing>
        </AuthorizeRouteView>
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

If you now try to access the checkout page while signed out, you are redirected to the login page. And once the user is logged in, they are redirected back to the page they were trying to access thanks to the `returnUrl` parameter.

## Hiding navigation options depending on authorization status

It's a bit unfortunate that users can see the My Orders tab when they are not logged in. We can hide the My Orders tab for unauthenticated users using the `AuthorizeView` component.

Update `MainLayout` to wrap the My Orders `NavLink` in an `AuthorizeView`.

```razor
<AuthorizeView>
    <NavLink href="myorders" class="nav-tab">
        <img src="img/bike.svg" />
        <div>My Orders</div>
    </NavLink>
</AuthorizeView>
```

The My Orders tab should now only be visible when the user is logged in.

We've now seen two ways to interact with the authentication/authorization system inside components:

 * Wrap content in an `AuthorizeView`. This is useful when you just need to vary some UI content according to authorization status.
 * Place an `[Authorize]` attribute on a routable component. This is useful if you want to control the reachability of an entire page based on authorization conditions.

---
[Let's explore building components](05-components.md)
