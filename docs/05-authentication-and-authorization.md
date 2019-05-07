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

If you try to run your application now, you'll find that you can no longer place orders, nor can you retrieve details of orders already placed. Requests to these endpoints will return HTTP 302 redirections to a login URL that doesn't exist. That's good, because it shows that rules are being enforced on the server!

![image](https://user-images.githubusercontent.com/1101362/51806888-77ed3e00-2277-11e9-80c7-ffe7b9b2268c.png)

## Tracking login state

The client code needs a way to track whether the user is logged in, and if so *which* user is logged in, so it can influence how the UI behaves. We could do this using the *AppState* pattern like we did for `OrderState`, but let's consider another technique.

We're going to use a ready-made component called `UserStateProvider`, which is responsible for:

* Asking the server whether the user is logged in
* Tracking the returned logged in/out status and username, and supplying this information to other components that want it
* Exposing `public` methods that start the sign-in and sign-out flows

 This is all defined in the `BlazingPizza.ComponentsLibrary` project, and may end up being baked into the framework in some form, as it's more complex than many developers would want to write themselves.

 To use it, update `App.razor`, wrapping an instance of `UserStateProvider` around the entire application:

```html
<UserStateProvider>
    <Router AppAssembly=typeof(Program).Assembly />
</UserStateProvider>
```

At first this will appear to do nothing, but in fact this has made available a *cascading parameter* to all descendant components. A cascading parameter is a parameter that isn't passed down just one level in the hierarchy, but through any number of levels.

## Displaying login state

Create a new component called `UserInfo` in the client project's `Shared` folder, containing:

```html
Logged in: @UserState.IsLoggedIn

@functions {
    [CascadingParameter] UserStateProvider UserState { get; set; }
}
```

This is similar to using `[Parameter]`, except that it receives cascading values from any ancestor component.

By default, the framework matches by type. We're asking for an instance of `UserStateProvider`, so the framework will give us the closest one in the ancestry. Or if there isn't one, we'll get `null`. It's also possible to match by a string-valued name, but we don't need that here.

It might seem surprising that the `UserStateProvider` supplies *itself* to descendants. In fact, that's a commonly useful pattern, because it makes it easy to expose `public` methods that descendants can invoke. In this case, we'll be calling its `SignIn` and `SignOut` methods soon. However, there's no requirement to follow this pattern: components can supply other values beside themselves as cascading values if they want.

We don't really just want to display `Logged in: false`, so replace the markup in `UserInfo` as follows:

```html
<div class="user-info">
    @if (UserState.CurrentUser == null)
    {
        <text>...</text>
    }
    else if (UserState.CurrentUser.IsLoggedIn)
    {
        <img src="img/user.svg" />
        <div>
            <span class="username">@UserState.CurrentUser.DisplayName</span>
            <span class="sign-out" onclick="@UserState.SignOut">Sign out</span>
        </div>
    }
    else
    {
        <button onclick="@UserState.SignIn" class="sign-in">Sign in</button>
    }
</div>
```

This handles three scenarios:

1. The client is waiting for the server to say whether the user is signed in or not
2. The server says they are signed in
3. The server says they are not signed in

Finally, let's put the `UserInfo` in the UI somewhere. Open `MainLayout`, and update the `<div class="top-bar">` as follows:

```html
<div class="top-bar">
    (... leave existing content in place ...)

    <UserInfo />
</div>
```

Because the user isn't yet signed in, it will display a "sign in" button.

![image](https://user-images.githubusercontent.com/1101362/51807139-0a431100-227b-11e9-923b-b9313abf8992.png)

## Signing in with Twitter

This application uses server-based authentication. The mechanism is as follows:

1. The client asks the server whether the user is logged in.
1. The server uses ASP.NET Core's built-in cookie-based authentication system to track logins, so it can respond to the client's query with the authenticated username.
1. If the client asks the server to begin the sign-in flow, the server uses ASP.NET Core's built-in federated OAuth support to show a "log in with Twitter" dialog. However you could easily reconfigure this to use Google or Facebook login, or even to use ASP.NET Core's built in *Identity* system, which is a standalone user database.
1. After the user logs in with Twitter or another authentication provider, the server sets an authentication cookie so that subsequent queries in step 1 will return the authenticated username.
1. The client displays whatever username the server returns.
1. Subsequent HTTP requests to API endpoints on `OrdersController` will include the cookie, so the server will be able to authorize the request.
1. If the client wants the user to log out, it calls an endpoint on the server that will clear the authentication cookie.

You'll notice that, in `UserInfo`, the "sign in" button is wired up to `UserStateProvider`'s `SignIn` method. If you check the code there, you'll find that it makes a call via JavaScript interop to open a popup displaying `/user/signin?returnUrl=(current url)`. This is what begins the sign-in flow mentioned in step 3 above. Once it's complete, the server renders a fragment of JavaScript that closes the pop-up and notifies the Blazor application that the login state has changed.

Try it out now. When you click "sign in", you should actually be able to sign in with Twitter and then see your username in the UI.

> Tip: If you get an error saying *HttpRequestException: Response status code does not indicate success: 403 (Forbidden)*, it probably means your application is running on the wrong port. Change the port to port `64589` or `64590` by editing  `BlazingPizza.Server/Properties/launchSettings.json`, and try again.

![image](https://user-images.githubusercontent.com/1101362/51807619-f4d0e580-2280-11e9-9891-2a9cd7b2a49b.png)

For the OAuth flow to succeed in this example, you *must* be running on `http(s)://localhost:64589` or `http(s)://localhost:64590`, and not any other port. That's because the Twitter application ID in `appsettings.Development.json` references an application configured with those values. To deploy a real application, you'll need to use the [Twitter Developer Console](https://developer.twitter.com/apps) to register a new application, get your own client ID and secret, and register your own callback URLs.

Because the authentication state is persisted by the server in a cookie, you can freely reload the page and the browser will stay logged in. You can also click *Sign out* to invoke `UserStateProvider`'s `SignOut` method, which will ask the server to clear the authentication cookie.

## Ensuring authentication before placing an order

If you're now logged in, you'll be able to place orders and see order status. But if you log out then make another attempt to place an order, bad things will happen. The server will reject the `POST` request, causing a client-side exception, but the user won't know why.

To fix this, let's make the UI prompt the user to log in (if necessary) as part of placing an order.

In the `Index` component, use `[CascadingParameter]` to receive the latest `UserStatus` data by adding the following to the `@functions` block:

```cs
[CascadingParameter] UserStateProvider UserState { get; set; }
```

Then update `PlaceOrder` so that, if the user isn't signed in, they'll be sent through the sign-in flow before the order is placed:

```cs
async Task PlaceOrder()
{
    // The server will reject the submission if you're not signed in, so attempt
    // to sign in first if needed
    if (await UserState.TrySignInAsync())
    {
        await HttpClient.PostJsonAsync("orders", OrderState.Order);
        OrderState.ResetOrder();
        UriHelper.NavigateTo("myorders");
    }
}
```

This method uses `await` so that the `PlaceOrder` process can wait for as long as necessary for the user to sign in. If they sign in successfully, the order will be submitted. If they don't sign in, the order will remain unsubmitted.

Try it out: while signed out, try to place an order.

## Handling signed-out users on "My orders"

If you're signed out and visit "My orders", the server will reject the request to `/orders`, causing a client-side exception (try it and see). To avoid this, we should change the UI so that it displays a notice about needing to log in instead.

One way to do that would be to use `[CascadingParameter]` to get the user's sign-in status inside `MyOrders`, and only query for the order list if they are logged in. But then we'd have to duplicate the same logic in the "Order details" component. Isn't there some way we could share this logic across all pages that require authentication?

There are many ways this could be done, but one particularly convienient way is to use a *layout*. Since layouts can be nested, we can make a `ForceSignInLayout` component that nests inside the existing `MainLayout`. Then, any page that uses our `ForceSignInLayout` will only display when the user is signed in, and if they aren't, it will show a prompt to sign in.

Start by creating a new component called `ForceSignInLayout.razor` inside the `Shared` directory, containing:

```html
@inherits LayoutComponentBase
@layout MainLayout

@if (UserState.CurrentUser == null) // Retrieving the login state
{
    <text>Loading...</text>
}
else if (UserState.IsLoggedIn)
{
    @Body
}
else
{
    <div class="main">
        <h2>You're signed out</h2>
        <p>To continue, please sign in.</p>
        <button class="btn btn-danger" onclick="@UserState.SignIn">Sign in</button>
    </div>
}

@functions {
    [CascadingParameter] UserStateProvider UserState { get; set; }
}
```

This is a layout, because it inherits from `LayoutComponentBase`. It nests inside `MainLayout`, because it has its own `@layout` directive saying so.

Further, it uses `[CascadingParameter]` to get a `UserState` value, and uses that to decide whether to render the current page (by outputting `@Body`) or a "sign in" button instead.

Now you can go to `MyOrders`, and change its layout by putting the following directive at the top:

```
@layout ForceSignInLayout
```

Now, logged out users will see a suitable message:

![image](https://user-images.githubusercontent.com/1101362/51807840-11225180-2284-11e9-81ed-ea9caacb79ef.png)

If you click either of the two "Sign in" buttons and successfully sign in, the UI will immediately update to show the contents for the page.

## Handling signed-out users on "Order details"

If you directly browse to `/myorders/1` while signed out, or click "sign out" while on the order details page, you'll get a strange message:

![image](https://user-images.githubusercontent.com/1101362/51807869-5f375500-2284-11e9-8417-dcd572cd028d.png)

Once again, this is because the server is rejecting the query for order details while signed out.

But you can fix this trivially: just change `OrderDetails` to use your new `ForceSignInLayout`:

```
@layout ForceSignInLayout
```

Now, the "Order details" page will display the same "please sign in" prompt to unauthenticated visitors.

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

Next up - [JavaScript interop](06-javascript-interop.md)
