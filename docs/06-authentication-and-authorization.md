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

If you try to run your application now, you'll find that you can no longer place orders, nor can you retrieve details of orders already placed. Requests to these endpoints will return HTTP 302 redirections to a login URL that doesn't exist. That's good, because it shows that rules are being enforced on the server!

![image](https://user-images.githubusercontent.com/1101362/51806888-77ed3e00-2277-11e9-80c7-ffe7b9b2268c.png)

## Tracking authentication state

The client code needs a way to track whether the user is logged in, and if so *which* user is logged in, so it can influence how the UI behaves. Blazor has a built-in DI service for doing this: the `AuthenticationStateProvider`.

Server-side Blazor comes with a built-in `AuthenticationStateProvider` that hooks into server-side authentication features to determine who's logged in. But your application runs on the client, so you'll need to implement your own `AuthenticationStateProvider` that gets the login state somehow.

To start, create a new class named `ServerAuthenticationStateProvider` in the root of your `BlazingPizza.Client` project:

```cs
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazingPizza.Client
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            // Currently, this returns fake data
            // In a moment, we'll get real data from the server
            var claim = new Claim(ClaimTypes.Name, "Fake user");
            var identity = new ClaimsIdentity(new[] { claim }, "serverauth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}
```

... then register this as a DI service in `Startup.cs`:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<OrderState>();

    // Add auth services
    services.AddAuthorizationCore();
    services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
}
```

To flow the authentication state information through your app, you need to add one more component. In `App.razor`, surround the entire `<Router>` with a `<CascadingAuthenticationState>`:

```html
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly" Context="routeData">
        ...
    </Router>
</CascadingAuthenticationState>
```

At first this will appear to do nothing, but in fact this has made available a *cascading parameter* to all descendant components. A cascading parameter is a parameter that isn't passed down just one level in the hierarchy, but through any number of levels.

Finally, you're ready to display something in the UI!

## Displaying login state

Create a new component called `LoginDisplay` in the client project's `Shared` folder, containing:

```html
<div class="user-info">
    <AuthorizeView>
        <Authorizing>
            <text>...</text>
        </Authorizing>
        <Authorized>
            <img src="img/user.svg" />
            <div>
                <span class="username">@context.User.Identity.Name</span>
                <a class="sign-out" href="user/signout">Sign out</a>
            </div>
        </Authorized>
        <NotAuthorized>
            <a class="sign-in" href="user/signin">Sign in</a>
        </NotAuthorized>
    </AuthorizeView>
</div>
```

`<AuthorizeView>` is a built-in component that displays different content depending on whether the user meets specified authorization conditions. We didn't specify any authorization conditions, so by default it considers the user authorized if they are authenticated (logged in), otherwise not authorized.

You can use `<AuthorizeView>` anywhere you need UI content to vary by authorization state, such as controlling the visibility of menu entries based on a user's roles. In this case, we're using it to tell the user who they are, and conditionally show either a "log in" or "log out" link as applicable.

Let's put the `LoginDisplay` in the UI somewhere. Open `MainLayout`, and update the `<div class="top-bar">` as follows:

```html
<div class="top-bar">
    (... leave existing content in place ...)

    <LoginDisplay />
</div>
```

Because you're supplying fake login information, the user will appear to be signed in as "Fake user", and clicking the "sign out" link will not change that:

![image](https://user-images.githubusercontent.com/1101362/59272849-cb708f00-8c4e-11e9-9201-d350fb7ec9f9.png)

Note that you still can't retrieve any order information. The server won't be fooled by the fake login information.

## Signing in for real with Twitter

Your application is going to use cookie-based authentication. The mechanism is as follows:

1. The client asks the server whether the user is logged in.
1. The server uses ASP.NET Core's built-in cookie-based authentication system to track logins, so it can respond to the client's query with the authenticated username.
1. If the client asks the server to begin the sign-in flow, the server uses ASP.NET Core's built-in federated OAuth support to redirect to Twitter's login page. However you could easily reconfigure this to use Google or Facebook login, or even to use ASP.NET Core's built in *Identity* system, which is a standalone user database.
1. After the user logs in with Twitter or another authentication provider, the server sets an authentication cookie so that subsequent queries in step 1 will return the authenticated username.
1. The client app restarts, and this time shows whatever username the server returns.
1. Subsequent HTTP requests to API endpoints on `OrdersController` will include the cookie, so the server will be able to authorize the request.
1. If the client wants the user to log out, it calls an endpoint on the server that will clear the authentication cookie.

You'll notice that, in `LoginDisplay`, the "sign in" and "sign out" links take you to server-side endpoints implemented on `UserController` in `BlazingPizza.Server`. Have a look at the code in that controller and see how it uses ASP.NET Core's server-side APIs to handle the redirections.

What's missing currently is having your client-side app query the server to ask for the current login state. Go back to `ServerAuthenticationStateProvider`, and modify its logic as follows:

```cs
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazingPizza.Client
{
    public class ServerAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;

        public ServerAuthenticationStateProvider(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var userInfo = await _httpClient.GetJsonAsync<UserInfo>("user");

            var identity = userInfo.IsAuthenticated
                ? new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, userInfo.Name) }, "serverauth")
                : new ClaimsIdentity();

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }
}
```

Try it out now. Initially, the request to `/user` will return data saying you're logged out, so that's what your authentication state provider will flow through the UI &mdash; you'll see a *Sign in* link.

When you click "sign in", you should actually be able to sign in with Twitter and then see your username in the UI.

> Tip: If after logging in, the flow doesn't complete, it probably means your application is running on the wrong port. Change the port to port `64589` or `64590` by editing  `BlazingPizza.Server/Properties/launchSettings.json`, and try again.

![image](https://user-images.githubusercontent.com/1101362/51807619-f4d0e580-2280-11e9-9891-2a9cd7b2a49b.png)

For the OAuth flow to succeed in this example, you *must* be running on `http(s)://localhost:64589` or `http(s)://localhost:64590`, and not any other port. That's because the Twitter application ID in `appsettings.Development.json` references an application configured with those values. To deploy a real application, you'll need to use the [Twitter Developer Console](https://developer.twitter.com/apps) to register a new application, get your own client ID and secret, and register your own callback URLs.

Because the authentication state is persisted by the server in a cookie, you can freely reload the page and the browser will stay logged in. You can also click *Sign out* to hit the server-side endpoint that will clear the authentication cookie then redirect back.

## Ensuring authentication before placing an order

If you're now logged in, you'll be able to place orders and see order status. But if you log out then make another attempt to place an order, bad things will happen. The server will reject the `POST` request, causing a client-side exception, but the user won't know why.

To fix this, let's make the UI prompt the user to log in (if necessary) as part of placing an order.

In the `Checkout` page component, add an `OnInitializedAsync` with some logic to to check whether the user is currently authenticated. If they aren't, send them off to the login endpoint.

```cs
@code {
    [CascadingParameter] Task<AuthenticationState> AuthenticationStateTask { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateTask;
        if (!authState.User.Identity.IsAuthenticated)
        {
            // The server won't accept orders from unauthenticated users, so avoid
            // an error by making them log in at this point
            NavigationManager.NavigateTo("user/signin?redirectUri=/checkout", true);
        }
    }

    // Leave PlaceOrder unchanged here
}
```

Try it out: now if you're logged out and get to the checkout screen, you'll be redirected to log in. The value for the `[CascadingParameter]` comes from your `AuthenticationStateProvider` via the `<CascadingAuthenticationState>` you added earlier.

But do you notice something a bit awkward about it? It still shows the checkout UI briefly before the browser loads the Twitter login page. We can fix that easily by wrapping the "checkout" UI inside an `<AuthorizeView>`. Update the markup in `Checkout.razor` as follows:

```html
<div class="main">
    <AuthorizeView Context="authContext">
        <NotAuthorized>
            <h2>Redirecting you...</h2>
        </NotAuthorized>
        <Authorized>
            [the whole EditForm and contents remains here]
        </Authorized>
    </AuthorizeView>
</div>
```

That's better! Now you don't get the awkward brief appearance of a non-applicable bit of UI, and you can't possibly click the *Place order* button really quickly before the redirection completes.

## Preserving order state across the redirection flow

We've just introduced a pretty serious defect into the application. Since you're building a client-side SPA, the application state (such as the current order) is held in the browser's memory. When you redirect away to log in, that state is discarded. When the user is redirected back, their order has now become empty!

Check you can reproduce this bug. Start logged out, and build an order. Then go to the checkout screen via the redirection. When you get back to the app, you should be able to see your order contents were lost. This is a common concern with browser-based single-page applications (SPAs), but fortunately there are straightforward solutions.

We'll fix the bug by persisting the order state in the browser's `localStorage`. Since `localStorage` is a JavaScript API, we can reach it using *JavaScript interop*. Go back to `Checkout.razor` and at the top, inject an instance of `IJSRuntime`:

```cs
@inject IJSRuntime JSRuntime
```

Then, inside `OnInitializedAsync`, add the following line just above the `NavigationManager.NavigateTo` call:

```cs
await LocalStorage.SetAsync(JSRuntime, "currentorder", OrderState.Order);
```

You'll learn much more about JavaScript interop in later part of this workshop, so you don't need to get too deep into this right now. But if you want, have a look at the implementation of `LocalStorage.cs` in `BlazingPizza.ComponentsLibrary` and `localStorage.js` - there's not much to it.

Now you've done this, the current order state will be persisted in JSON form in `localStorage` right before the redirection occurs. You can see the data using the browser's JavaScript console after executing this code path:

![image](https://user-images.githubusercontent.com/1101362/59276103-90258e80-8c55-11e9-9489-5625f424880f.png)

This is still not quite enough, because even though you're saving the data, you're not yet reloading it when the user returns to the app. Add the following logic at the bottom of `OnInitializedAsync` in `Checkout.razor`:

```cs
// Try to recover any temporary saved order
if (!OrderState.Order.Pizzas.Any())
{
    var savedOrder = await LocalStorage.GetAsync<Order>(JSRuntime, "currentorder");
    if (savedOrder != null)
    {
        OrderState.ReplaceOrder(savedOrder);
        await LocalStorage.DeleteAsync(JSRuntime, "currentorder");
    }
    else
    {
        // There's nothing check out - go to home
        NavigationManager.NavigateTo("");
    }
}
```

You'll also need to add the following method to `OrderState` to accept the loaded order:

```cs
public void ReplaceOrder(Order order)
{
    Order = order;
}
```

Now you should no longer be able to reproduce the "lost order state" bug. Your order should be preserved across the redirection flow.

## Handling signed-out users on "My orders"

If you're signed out and visit "My orders", the server will reject the request to `/orders`, causing a client-side exception (try it and see). To avoid this, we should change the UI so that it displays a notice about needing to log in instead. How should we do this?

There are three basic ways to interact with the authentication/authorization system inside components. We've already seen two of them:

 * You can use `<AuthorizeView>`. This is useful when you just need to vary some UI content according to authorization status.
 * You can use a `[CascadingParameter]` to receive a `Task<AuthenticationState>`. This is useful when you want to use the `AuthenticationState` in procedural logic such as an event handler.

The third way, which we'll use here, is:

 * You can place an `[Authorize]` attribute on a routable `@page` component. This is useful if you want to control the reachability of an entire page based on authorization conditions.

So, go to `MyOrders`, and and put the following directive at the top (just under the `@page` line):

```cs
@attribute [Authorize]
```

The `[Authorize]` functionality is part of the routing system, and we'll need to make some changes there. In `App.razor`, replace `<RouteView ../>` with `<AuthorizeRouteView .../>`.

```html
<CascadingAuthenticationState>
    <Router AppAssembly="typeof(Program).Assembly" Context="routeData">
        <Found>
            <AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)" />
        </Found>
        ...
    </Router>
</CascadingAuthenticationState>
```

The `AuthorizeRouteView` component is like `RouteView` in that it can display a routable component and it's layout, but also integrates with `[Authorize]`.

---

Now, logged in users can reach the *My orders* page, but logged out users will see the message *Not authorized* instead. Verify you can see this working.

Finally, let's be a bit friendlier to logged out users. Instead of just saying *Not authorized*, we can customize this to display a link to sign in. Go to `App.razor`, and pass the following `<NotAuthorized>` and `<Authorizing>` parameters to the `<AuthorizeRouteView>`:

```html
<AuthorizeRouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)">
    <NotAuthorized>
        <div class="main">
            <h2>You're signed out</h2>
            <p>To continue, please sign in.</p>
            <a class="btn btn-danger" href="user/signin">Sign in</a>
        </div>
    </NotAuthorized>
    <Authorizing>
        Please wait...
    </Authorizing>
</AuthorizeRouteView>
```

Now if you're logged out and try to go to *My orders*, you'll get a much nicer outcome:

![image](https://user-images.githubusercontent.com/1101362/51807840-11225180-2284-11e9-81ed-ea9caacb79ef.png)

## Handling signed-out users on "Order details"

If you directly browse to `/myorders/1` while signed out, you'll get a strange message:

![image](https://user-images.githubusercontent.com/1101362/51807869-5f375500-2284-11e9-8417-dcd572cd028d.png)

Once again, this is because the server is rejecting the query for order details while signed out.

But you can fix this trivially: just use `[Authorize]` on `OrderDetails.razor` in the same way you did on `MyOrders.razor`. Try it out! It will display the same "please sign in" prompt to unauthenticated visitors.

## Authorizing access to specific order details

Although the server requires authentication before accepting queries for order information, it still doesn't distinguish between users. All signed-in users can see the orders from all other signed-in users. We have authentication, but no authorization!

To verify this, place an order while signed in with one Twitter account. Then sign out and back in using a different Twitter account. You'll still be able to see the same order details.

This is easily fixed. Back in the `OrdersController` code, look for the commented-out line in `PlaceOrder`, and uncomment it:

```cs
order.UserId = GetUserId();
```

Now each order will be stamped with the ID of the user who owns it.

Next look for the commented-out `.Where` lines in `GetOrders` and `GetOrderWithStatus`, and uncomment both. These lines ensure that users can only retrieve details of their own orders:

```csharp
.Where(o => o.UserId == GetUserId())
```

Now if you run the app again, you'll no longer be able to see the existing order details, because they aren't associated with your user ID. If you place a new order with one Twitter account, you won't be able to see it from a different Twitter account. That makes the application much more useful.

Next up - [JavaScript interop](07-javascript-interop.md)
