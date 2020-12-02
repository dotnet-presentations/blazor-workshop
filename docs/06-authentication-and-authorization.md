# Authentication

The application is working well. Users can place orders and track their order status. But there's one little problem: currently we don't distinguish between users at all. The "My orders" page lists *all* orders placed by *all* users, and anybody can view the state of anybody else's order. Your customers, and privacy regulations, may have an issue with this.

The solution is *authentication*. We need a way for users to log in, so we know who's who. Then we can implement *authorization*, which is to enforce rules about who's allowed to do what.

## Enforcement is on the server

The first and most important principle is that all *real* security rules must be enforced on the backend server. The client (UI) merely shows or hides options as a courtesy to well-behaved users, but a malicious user can always change the behavior of the client-side code.

As such, we're going to start by enforcing some access rules in the backend server, even before the client code knows about them.

Inside the `BlazorPizza.Server` project, you'll find `OrdersController.cs`. This is the controller class that handles incoming HTTP requests for `/orders` and `/orders/{orderId}`. To require that all requests to these endpoints come from authenticated users (i.e., people who have logged in), add the `[Authorize]` attribute to the `OrdersController` class:

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

To enable the authentication services, add a call to `AddApiAuthorization` in *Program.cs* in the client project:

```csharp
public static async Task Main(string[] args)
{
    var builder = WebAssemblyHostBuilder.CreateDefault(args);
    builder.RootComponents.Add<App>("#app");

    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
    builder.Services.AddScoped<OrderState>();

    // Add auth services
    builder.Services.AddApiAuthorization();

    await builder.Build().RunAsync();
}
```

The added services will be configured by default to use an identity provider on the same origin as the app. The server project for the Blazing Pizza app has already been setup to use [IdentityServer](https://identityserver.io/) as the identity provider and ASP.NET Core Identity for the authentication system:

*BlazingPizza.Server/Startup.cs*

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllersWithViews();
    services.AddRazorPages();

    services.AddDbContext<PizzaStoreContext>(options => 
        options.UseSqlite("Data Source=pizza.db"));

    services.AddDefaultIdentity<PizzaStoreUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<PizzaStoreContext>();

    services.AddIdentityServer()
        .AddApiAuthorization<PizzaStoreUser, PizzaStoreContext>();

    services.AddAuthentication()
        .AddIdentityServerJwt();
}
```

The server has also already been configured to issue tokens to the client app:

*BlazingPizza.Server/appsettings.json*

```json
"IdentityServer": {
  "Clients": {
    "BlazingPizza.Client": {
      "Profile": "IdentityServerSPA"
    }
  }
}
```

To orchestrate the authentication flow, add an `Authentication` component to the *Pages* directory in the client project:

*BlazingPizza.Client/Pages/Authentication.razor*

```razor
@page "/authentication/{action}"

<RemoteAuthenticatorView Action="@Action" />

@code{
    [Parameter]
    public string Action { get; set; }
}
```

The `Authentication` component is setup to handle the various authentication actions using the built-in `RemoteAuthenticatorView` component. The `Action` parameter is bound to the `{action}` route value, which is then passed to the `RemoteAuthenticatorView` component to handle. The `RemoteAuthenticatorView` handles all of the actions used as part of remote authentication. Valid actions include: register, login, profile, and logout. See [Customize the authentication user interface](https://docs.microsoft.com/aspnet/core/security/blazor/webassembly/additional-scenarios#customize-app-routes) for more details.

To flow the authentication state information through your app, you need to add one more component. In `App.razor`, surround the entire `<Router>` with a `<CascadingAuthenticationState>`:

```html
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly" Context="routeData">
        ...
    </Router>
</CascadingAuthenticationState>
```

At first this will appear to do nothing, but in fact this has made a *cascading parameter* available to all descendant components. A cascading parameter is a parameter that isn't passed down just one level in the hierarchy, but through any number of levels.

Finally, you're ready to display something in the UI!

## Displaying login state

Create a new component called `LoginDisplay` in the client project's `Shared` folder, containing:

```html
@inject NavigationManager Navigation
@inject SignOutSessionStateManager SignOutManager

<div class="user-info">
    <AuthorizeView>
        <Authorizing>
            <text>...</text>
        </Authorizing>
        <Authorized>
            <img src="img/user.svg" />
            <div>
                <a href="authentication/profile" class="username">@context.User.Identity.Name</a>
                <button class="btn btn-link sign-out" @onclick="BeginSignOut">Sign out</button>
            </div>
        </Authorized>
        <NotAuthorized>
            <a class="sign-in" href="authentication/register">Register</a>
            <a class="sign-in" href="authentication/login">Log in</a>
        </NotAuthorized>
    </AuthorizeView>
</div>

@code{
    async Task BeginSignOut()
    {
        await SignOutManager.SetSignOutState();
        Navigation.NavigateTo("authentication/logout");
    }
}
```

`AuthorizeView` is a built-in component that displays different content depending on whether the user meets specified authorization conditions. We didn't specify any authorization conditions, so by default it considers the user authorized if they are authenticated (logged in), otherwise not authorized.

You can use `AuthorizeView` anywhere you need UI content to vary by authorization state, such as controlling the visibility of menu entries based on a user's roles. In this case, we're using it to tell the user who they are, and conditionally show either a "log in" or "log out" link as applicable.

The links to register, log in, and see the user profile are normal links that navigate to the `Authentication` component. The sign out link is a button and has additional logic to prevent a forged request from logging the user out. Using a button ensures that the sign out can only be triggered by a user action, and the `SignOutSessionStateManager` service maintains state across the sign out flow to ensure the whole flow originated with a user action.

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

## Request an access token

Even though you are now logged in, placing an order still fails because the HTTP request to place the order requires a valid access token. To request access tokens and attach them to outbound requests, use the `BaseAddressAuthorizationMessageHandler` with the `HttpClient` that you're using to make the request. This message handler will acquire access tokens using the built-in `IAccessTokenProvider` service and attach them to each request using the standard Authorization header. If an access token cannot be acquired, an `AccessTokenNotAvailableException` is thrown, which can be used to redirect the user to the login page to authorize a new token.

To add the `BaseAddressAuthorizationMessageHandler` to our `HttpClient` in our app, we'll use the [IHttpClientFactory` helpers from ASP.NET Core](https://docs.microsoft.com/aspnet/core/fundamentals/http-requests) with a strongly typed client.

To create the strongly typed client, add a new `OrdersClient` class to the client project. The class should take an `HttpClient` in its constructor, and provide methods getting and placing orders:

*BlazingPizza.Client/OrdersClient.cs*

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace BlazingPizza.Client
{
    public class OrdersClient
    {
        private readonly HttpClient httpClient;

        public OrdersClient(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<IEnumerable<OrderWithStatus>> GetOrders() =>
            await httpClient.GetFromJsonAsync<IEnumerable<OrderWithStatus>>("orders");


        public async Task<OrderWithStatus> GetOrder(int orderId) =>
            await httpClient.GetFromJsonAsync<OrderWithStatus>($"orders/{orderId}");


        public async Task<int> PlaceOrder(Order order)
        {
            var response = await httpClient.PostAsJsonAsync("orders", order);
            response.EnsureSuccessStatusCode();
            var orderId = await response.Content.ReadFromJsonAsync<int>();
            return orderId;
        }
    }
}
```

Register the `OrdersClient` as a typed client, with the underlying `HttpClient` configured with the correct base address and the `BaseAddressAuthorizationMessageHandler`.

```csharp
builder.Services.AddHttpClient<OrdersClient>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
```

Update each page where an `HttpClient` is used to manage orders to use the new typed `OrdersClient`. Inject an `OrdersClient` instead of an `HttpClient` and use the new client to make the API call. Wrap each call in a `try-catch` that handles exceptions of type `AccessTokenNotAvailableException` by calling the provided `Redirect()` method.

*Checkout.razor*

```csharp
async Task PlaceOrder()
{
    isSubmitting = true;

    try
    {
        var newOrderId = await OrdersClient.PlaceOrder(OrderState.Order);
        OrderState.ResetOrder();
        NavigationManager.NavigateTo($"myorders/{newOrderId}");
    }
    catch (AccessTokenNotAvailableException ex)
    {
        ex.Redirect();
    }
}
```

*MyOrders.razor*

```csharp
protected override async Task OnParametersSetAsync()
{
    try
    {
        ordersWithStatus = await OrdersClient.GetOrders();
    }
    catch (AccessTokenNotAvailableException ex)
    {
        ex.Redirect();
    }
}
```

*OrderDetails.razor*

```csharp
private async void PollForUpdates()
{
    invalidOrder = false;
    pollingCancellationToken = new CancellationTokenSource();
    while (!pollingCancellationToken.IsCancellationRequested)
    {
        try
        {
            orderWithStatus = await OrdersClient.GetOrder(OrderId);
            StateHasChanged();
            await Task.Delay(4000);
        }
        catch (AccessTokenNotAvailableException ex)
        {
            pollingCancellationToken.Cancel();
            ex.Redirect();
        }
        catch (Exception ex)
        {
            invalidOrder = true;
            pollingCancellationToken.Cancel();
            Console.Error.WriteLine(ex);
            StateHasChanged();
        }
    }
}
```

## Authorizing access to specific order details

Although the server requires authentication before accepting queries for order information, it still doesn't distinguish between users. All signed-in users can see the orders from all other signed-in users. We have authentication, but no authorization!

To verify this, place an order while signed in with one account. Then sign out and back in using a different account. You'll still be able to see the same order details.

This is easily fixed. Back in the `OrdersController` code, look for the commented-out line in `PlaceOrder`, and uncomment it:

```cs
order.UserId = GetUserId();
```

Now each order will be stamped with the ID of the user who owns it.

Next look for the commented-out `.Where` lines in `GetOrders` and `GetOrderWithStatus`, and uncomment both. These lines ensure that users can only retrieve details of their own orders:

```csharp
.Where(o => o.UserId == GetUserId())
```

Now if you run the app again, you'll no longer be able to see the existing order details, because they aren't associated with your user ID. If you place a new order with one account, you won't be able to see it from a different account. That makes the application much more useful.

## Enforcing login on specific pages

Now if you're logged in, you'll be able to place orders and see order status. But if you're not logged in and try to place an order, the flow isn't ideal. It doesn't ask you to log in until you *submit* the checkout form (because that's when the server responds 401 Not Authorized). What if you want to make certain pages require authorization, even before receiving 401 Not Authorized responses from the server?

You can do this quite easily. In the same way that you use the `[Authorize]` attribute in server-side code, you can use that attribute in client-side Blazor pages. Let's fix the checkout page so that you have to be logged in as soon as you get there, not just when you submit its form.

By default, all pages allow for anonymous access, but we can specify that the user must be logged in to access the checkout page by adding the `[Authorize]` attribute at the top of `Checkout.razor`:

```razor
@attribute [Authorize]
```

Next, to make the router respect such attributes, update *App.razor* to render an `AuthorizeRouteView` instead of a `RouteView` when the route is found.

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly" Context="routeData">
        <Found>
            <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)">
                <NotAuthorized>
                    <p>You are not authorized to access this resource.</p>
                </NotAuthorized>
                <Authorizing>
                    <div class="main">Please wait...</div>
                </Authorizing>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <LayoutView Layout="typeof(MainLayout)">
                <div class="main">Sorry, there's nothing at this address.</div>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
```

The `AuthorizeRouteView` will route navigations to the correct component, but only if the user is authorized. If the user is not authorized, the `NotAuthorized` content is displayed. You can also specify content to display while the `AuthorizeRouteView` is determining if the user is authorized.

Now when you try to navigate to the checkout page while signed out, you see the `NotAuthorized` content we setup in *App.razor*.

![Not authorized](https://user-images.githubusercontent.com/1874516/78410504-63b27880-75c1-11ea-8c2c-ab62c1c24596.png)

Instead of telling the user they are unauthorized it would be better if we redirected them to the login page. To do that, add the following `RedirectToLogin` component:

*BlazingPizza.Client/Shared/RedirectToLogin.razor*

```razor
@inject NavigationManager Navigation
@code {
    protected override void OnInitialized()
    {
        Navigation.NavigateTo($"authentication/login?returnUrl={Navigation.Uri}");
    }
}
```

Then replace the `NotAuthorized` content in *App.razor* with the `RedirectToLogin` component.

```razor
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly" Context="routeData">
        <Found>
            <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)">
                <NotAuthorized>
                    <RedirectToLogin />
                </NotAuthorized>
                <Authorizing>
                    <div class="main">Please wait...</div>
                </Authorizing>
            </AuthorizeRouteView>
        </Found>
        <NotFound>
            <LayoutView Layout="typeof(MainLayout)">
                <div class="main">Sorry, there's nothing at this address.</div>
            </LayoutView>
        </NotFound>
    </Router>
</CascadingAuthenticationState>
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

## Preserving order state across the redirection flow

We've just introduced a pretty serious defect into the application. Since you're building a client-side SPA, the application state (such as the current order) is held in the browser's memory. When you redirect away to log in, that state is discarded. When the user is redirected back, their order has now become empty!

Check you can reproduce this bug. Start logged out, and create an order. When you try to place the order, you get redirected to the login page. After logging in, you are then redirected to the checkout page, but your pizzas in your order have now gone missing! This is a common concern with browser-based single-page applications (SPAs), but fortunately there is a straightforward solution.

We'll fix the bug by persisting the order state. Blazor's authentication library makes this straight forward to do.

To define the state that we want persisted, add a `PizzaAuthenticationState` class that inherits from `RemoteAuthenticationState`. `RemoteAuthenticationState` is used by the authentication system to preserve state across the redirects, like the return URL. When you derive from this type, any public properties will be JSON serialized as part of the persisted state. Add an `Order` property to persist the current order.

```csharp
public class PizzaAuthenticationState : RemoteAuthenticationState
{
    public Order Order { get; set; }
}
```

To configure the authentication system to use our `PizzaAuthenticationState` instead of the default `RemoteAuthenticationState`, update *Program.cs* as follows:

```csharp
// Add auth services
builder.Services.AddApiAuthorization<PizzaAuthenticationState>();
```

Now we need to add logic to persist the current order, and then reestablish the current order from the persisted state after the user has successfully logged in. To do that, update the `Authentication` component to use `RemoteAuthenticatorViewCore` instead of `RemoteAuthenticatorView`. Override `OnInitialized` to setup the order state to be persisted, and implement the `OnLogInSucceeded` callback to reestablish the order state. You'll need to add a `ReplaceOrder` method to `OrderState` so that you can reestablish the saved order.

*BlazingPizza.Client/Pages/Authentication.razor*

```razor
@page "/authentication/{action}"
@inject OrderState OrderState
@inject NavigationManager NavigationManager

<RemoteAuthenticatorViewCore
    TAuthenticationState="PizzaAuthenticationState"
    AuthenticationState="RemoteAuthenticationState"
    OnLogInSucceeded="RestorePizza"
    Action="@Action" />

@code{
    [Parameter] public string Action { get; set; }

    public PizzaAuthenticationState RemoteAuthenticationState { get; set; } = new PizzaAuthenticationState();

    protected override void OnInitialized()
    {
        if (RemoteAuthenticationActions.IsAction(RemoteAuthenticationActions.LogIn, Action))
        {
            // Preserve the current order so that we don't loose it
            RemoteAuthenticationState.Order = OrderState.Order;
        }
    }

    private void RestorePizza(PizzaAuthenticationState pizzaState)
    {
        if (pizzaState.Order != null)
        {
            OrderState.ReplaceOrder(pizzaState.Order);
        }
    }
}
```

*BlazingPizza.Client/OrderState.cs*

```csharp
public class OrderState
{
    ...

    public void ReplaceOrder(Order order)
    {
        Order = order;
    }
}
```

Now if you try to place an order when signed out, you can see the order persisted in local storage during the authentication process:

![Persisted order state](https://user-images.githubusercontent.com/1874516/78414685-30c4b080-75d2-11ea-98df-d1ac73548774.png)

## Customize the logout experience

Currently, when the user signs out, they are brought to a generic logged out page:

![Logged out](https://user-images.githubusercontent.com/1874516/78414080-4684a680-75cf-11ea-808d-8d44a5f3941e.png)

You can customize this page in your `Authentication` component by setting the `LogOutSucceeded` property on `RemoteAuthenticatorViewCore`.

But what if we want the user to be redirected back to the home page after they log out? To do that, we can configure in *Program.cs* which path to direct the user to when they successfully log out.

```csharp
// Add auth services
builder.Services.AddApiAuthorization<PizzaAuthenticationState>(options =>
{
    options.AuthenticationPaths.LogOutSucceededPath = "";
});
```

Now when you sign out, the user should be brought back to the home page.

Next up - [JavaScript interop](07-javascript-interop.md)
