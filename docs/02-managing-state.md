# Order Status and Managing State

Your customers can order pizzas, but so far they have no way to see the status of their orders. In this session you'll implement a "My orders" page that lists multiple orders, plus an "Order details" view showing the contents and status of an individual order.

Thinking back to our interactivity concerns, these pages could function statically for the logged in user and their orders as there is no interaction to be offered with the order data.

## Adding a navigation link

Open `BlazingPizza/Components/Layout/MainLayout.razor`. As an experiment, let's try adding a new link element *without* using a `NavLink` component. Add a plain HTML `<a>` tag pointing to `myorders`:

```html
<div class="top-bar">
    (leave existing content in place)

    <a href="myorders" class="nav-tab">
        <img src="img/bike.svg" />
        <div>My Orders</div>
    </a>
</div>
```

> Notice how the URL we're linking to does *not* start with a `/`. If you linked to `/myorders`, it would appear to work the same, but if you ever wanted to deploy the app to a non-root URL the link would break. The `<base href="/">` tag in `App.razor` specifies the prefix for all non-slash-prefixed URLs in the app, regardless of which component renders them.

If you run the app now, you'll see the link, styled as expected:

![My orders link](https://user-images.githubusercontent.com/1874516/77241321-a03ba880-6bad-11ea-9a46-c73be397cb5e.png)


This shows it's not strictly necessary to use `<NavLink>`. We'll see the reason to use it momentarily.

## Adding a "My Orders" page

If you click "My Orders", you'll end up on a page that says "404: Not Found". Obviously this is because you haven't yet added anything that matches the URL `myorders`. But if you're watching really carefully, you might notice that on this occasion it's not just doing client-side (SPA-style) navigation, but instead is doing a full-page reload.

What's really happening is this:

1. You click on the link to `myorders`
2. Blazor, running on the client, tries to match this to a component based on `@page` directive attributes.
3. However, since no match is found, Blazor falls back on full-page load navigation in case the URL is meant to be handled by server-side code.
4. However, the server doesn't have anything that matches this either, so it falls back on rendering the client-side Blazor application.
5. This time, Blazor sees that nothing matches on either client *or* server, so it falls back on rendering the `NotFound` block from your `Routes.razor` component.

As you can guess, we will make the link actually work by adding a component to match this route. Create a file in the `BlazingPizza/Components/Pages` folder called `MyOrders.razor`, with the following content:

```html
@page "/myorders"

<div class="main">
    My orders will go here
</div>
```

Now when you run the app, you'll be able to visit this page:

![My orders blank page](https://user-images.githubusercontent.com/1874516/77241343-fc9ec800-6bad-11ea-8176-febf614ed4ad.png)

Also notice that this time, no full-page load occurs when you navigate, because the URL is matched entirely within the client-side SPA. As such, navigation is instantaneous.

## Adding a page title

In your browser, the title of the new page is listed as **Blazing Pizza** and it would be nice to update the title to reflect that this is the 'My Orders' page.  We can use the new `PageTitle` component to update the title from the `MyOrders.razor` page:

```html
@page "/myorders"

<PageTitle>Blazing Pizza - My Orders</PageTitle>

<div class="main">
    My orders will go here
</div>
```

This works because there is a `<HeadOutlet>` component in the `<head>` tag inside the `App.razor` file. Blazor uses this `HeadOutlet` to write content into the header of the HTML page.

```html
<head>
    ...
    <HeadOutlet />
</head>
```

## Highlighting navigation position

Look closely at the top bar. Notice that when you're on "My orders", the link *isn't* highlighted in yellow. How can we highlight links when the user is on them? By using a `NavLink` component instead of a plain `<a>` tag. The only special thing a `NavLink` component does is toggle its own `active` CSS class depending on whether its `href` matches the current navigation state.

Replace the `<a>` tag you just added in `MainLayout` with the following (which is identical apart from the tag name):

```html
<NavLink href="myorders" class="nav-tab">
    <img src="img/bike.svg" />
    <div>My Orders</div>
</NavLink>
```

Now you'll see the links are correctly highlighted according to the navigation state:

![My orders nav link](https://user-images.githubusercontent.com/1874516/77241358-412a6380-6bae-11ea-88da-424434d34393.png)

## Displaying the list of orders

Switch back to the `MyOrders` component code. Once again we're going to inject a `IRepository` so that we can query the database. Add the following under the `@page` directive line:

```html
@inject IRepository Repository
```

Then add a `@code` block that makes an asynchronous request for the data we need:

```csharp
@code {
    IEnumerable<OrderWithStatus>? ordersWithStatus;

    protected override async Task OnParametersSetAsync()
    {
		ordersWithStatus = await Repository.GetOrdersAsync();
    }
}
```

Let's make the UI display different output in three different cases:

 1. While we're waiting for the data to load
 2. If the user has never placed any orders
 3. If the user has placed one or more orders

It's simple to express this using `@if/else` blocks in Razor code. Update the markup inside your component as follows:

```html
<div class="main">
    @if (ordersWithStatus is null)
    {
        <text>Loading...</text>
    }
    else if (!ordersWithStatus.Any())
    {
        <h2>No orders placed</h2>
        <a class="btn btn-success" href="">Order some pizza</a>
    }
    else
    {
        <text>TODO: show orders</text>
    }
</div>
```

Perhaps some parts of this code aren't obvious, so let's point out a few things.

### 1. What's a `<text>` element?

`<text>` is *not* an HTML element at all. Nor is it a component. Once the `MyOrders` component is compiled, the `<text>` tag won't exist in the result at all.

`<text>` is a special signal to the Razor compiler that you want to treat its contents as a markup string and *not* as C# source code. It's only used on rare occasions where the syntax would otherwise be ambiguous.

### 2. What's with href=""?

If `<a href="">` (with an empty string for `href`) surprises you, remember that the browser will prefix the `<base href="/">` value to all non-slash-prefixed URLs. So, an empty string is the correct way to link to the client app's root URL.

### 3. How does this render?

The asynchronous flow we've implemented above means the component will render twice: once before the data has loaded (displaying "Loading.."), and then once afterwards (displaying one of the other two outputs).

### 4. Why are we using OnParametersSetAsync?

Asynchronous work when applying parameters and property values must occur during the OnParametersSetAsync lifecycle event. We will be adding a parameter in a later session.

### 5. How can I reset the database?

If you want to reset your database to see the "no orders" case, simply delete `pizza.db` from the **BlazingPizza** project and reload the page in your browser.

![My orders empty list](https://user-images.githubusercontent.com/1874516/77241390-a4b49100-6bae-11ea-8dd4-e59afdd8f710.png)

## Rendering a grid of orders

Now we have all the data we need, we can use Razor syntax to render an HTML grid.

Replace the `<text>TODO: show orders</text>` code with the following:

```html
<div class="list-group orders-list">
    @foreach (var item in ordersWithStatus)
    {
        <div class="list-group-item">
            <div class="col">
                <h5>@item.Order.CreatedTime.ToLongDateString()</h5>
                Items:
                <strong>@item.Order.Pizzas.Count()</strong>;
                Total price:
                <strong>£@item.Order.GetFormattedTotalPrice()</strong>
            </div>
            <div class="col">
                Status: <strong>@item.StatusText</strong>
            </div>
            <div class="col flex-grow-0">
                <a href="myorders/@item.Order.OrderId" class="btn btn-success">
                    Track &gt;
                </a>
            </div>
        </div>
    }
</div>
```

It looks like a lot of code, but there's nothing special here. It simply uses a `@foreach` to iterate over the `ordersWithStatus` and outputs a `<div>` for each one. The net result is as follows:

![My orders grid](https://user-images.githubusercontent.com/1874516/77241415-feb55680-6bae-11ea-89ba-f8367ef6a96c.png)

## Adding an Order Details display

If you click on the "Track" link buttons next to an order, the browser will attempt to navigate to `myorders/<id>` (e.g., `http://example.com/myorders/37`). Currently this will result in a "Sorry, there's nothing at this address." message because no component matches this route.

Once again we'll add a component to handle this. In the `BlazingPizza/Components/Pages` directory, create a file called `OrderDetails.razor`, containing:

```html
@page "/myorders/{orderId:int}"

<div class="main">
    TODO: Show details for order @OrderId
</div>

@code {
    [Parameter] public int OrderId { get; set; }
}
```

This code illustrates how components can receive parameters from the router by declaring them as tokens in the `@page` directive. If you want to receive a `string`, the syntax is simply `{parameterName}`, which matches a `[Parameter]` name case-insensitively. If you want to receive a numeric value, the syntax is `{parameterName:int}`, as in the example above. The `:int` is an example of a *route constraint*. Other route constraints, such as bool, datetime and guid, are also supported.

![Order details empty](https://user-images.githubusercontent.com/1874516/77241434-391ef380-6baf-11ea-9803-9e7e65a4ea2b.png)

If you're wondering how routing actually works, let's go through it step-by-step.

1. When the app first starts up, code in `Program.cs` tells the framework to render `App` as the root component.
2. The `App` component (in `App.razor`) contains a reference to the `<Routes>`. `Routes.razor` is a component that interacts with the browser's client-side navigation APIs. It registers a navigation event handler that gets notification whenever the user clicks on a link.
3. Whenever the user clicks on a link, code in `Routes.razor` checks whether the destination URL is within the same application (i.e., whether it's under the `<base href>` value, and it matches some component's declared routes). If it's not, traditional full-page navigation occurs as usual. But if the URL is within the application, `Router` will handle it.
4. `Router` handles it by looking for a component with a compatible `@page` URL pattern. Each `{parameter}` token needs to have a value, and the value has to be compatible with any constraints such as `:int`.
   * If there is a matching component, that's what the `Router` will render. This is how all the pages in your application have been rendering all along.
   * If there's no matching component, the router tries a full-page load in case it matches something on the server.
   * If the server chooses to re-render the client-side Blazor app (which is also what happens if a visitor is initially arriving at this URL and the server thinks it may be a client-side route), then Blazor concludes that nothing matches on either server or client, so it displays whatever `NotFound` content is configured.

## Polling for order details

The `OrderDetails` logic will be quite different from `MyOrders`. Instead of fetching the data just once when the component is instantiated, we'll poll the database every few seconds for updated data. This will make it possible to show the order status in (nearly) real-time, and later, to display the delivery driver's location on a moving map.

This leads us to the decision to make our OrderDetails page interactive.  In this case, let's make it ServerInteractive and we'll let it update the user interface when there are updates to present to our users.

What's more, we'll also account for the possibility of `OrderId` being invalid. This might happen if:

* No such order exists
* Or later, when we've implemented authentication, if the order is for a different user and you're not allowed to see it

Before we can implement the polling, we'll need to add the following directives at the top of `OrderDetails.razor`, typically directly under the `@page` directive:

```html
@using System.Threading
@rendermode InteractiveServer
@inject IRepository Repository
```

You've already seen `@inject` used with `IRepository`, so you know what that is for. Plus, you'll recognize `@using` from the equivalent in regular `.cs` files, so this shouldn't be much of a mystery either. Unfortunately, Visual Studio does not yet add `@using` directives automatically in Razor files, so you do have to write them in yourself when needed.  We also added the directive to force this page's render mode to `InteractiveServer`

Now you can implement the polling. Update your `@code` block as follows:

```cs
@code {
    [Parameter] public int OrderId { get; set; }

    OrderWithStatus? orderWithStatus;
    bool invalidOrder;
    CancellationTokenSource? pollingCancellationToken;

    protected override async Task OnParametersSetAsync()
    {
        // If we were already polling for a different order, stop doing so
        pollingCancellationToken?.Cancel();

		orderWithStatus = await Repository.GetOrderWithStatus(OrderId);

        // Start a new poll loop
        PollForUpdates();
    }

    private async void PollForUpdates()
    {
        pollingCancellationToken = new CancellationTokenSource();
        while (!pollingCancellationToken.IsCancellationRequested)
        {
            try
            {
				invalidOrder = false;
				orderWithStatus = await Repository.GetOrderWithStatus(OrderId);
				await InvokeAsync(StateHasChanged);

				if (orderWithStatus.IsDelivered)
				{
					pollingCancellationToken.Cancel();
				}
				else
				{
					await Task.Delay(4000);
				}
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
}
```

The code is a bit intricate, so be sure to go through it carefully to understand each aspect of it before proceeding. Here are some notes:

* This uses `OnParametersSet` instead of `OnInitialized` or `OnInitializedAsync`. `OnParametersSet` is another component lifecycle method, and it fires when the component is first instantiated *and* any time its parameters change value. If the user clicks a link directly from `myorders/2` to `myorders/3`, the framework will retain the `OrderDetails` instance and simply update its `OrderId` parameter in place.
  * As it happens, we haven't provided any links from one "my orders" screen to another, so the scenario never occurs in this application, but it's still the right lifecycle method to use in case we change the navigation rules in the future.
* We're using an `async void` method to represent the polling. This method runs for arbitrarily long, even while other methods run. `async void` methods have no way to report exceptions upstream to callers (because typically the callers have already finished), so it's important to use `try/catch` and do something meaningful with any exceptions that may occur.
* We're using `CancellationTokenSource` as a way of signalling when the polling should stop. Currently it stops if there's an exception, or once the order is delivered.
* We need to call `StateHasChanged` to tell Blazor that the component's data has (possibly) changed. The framework will then re-render the component. There's no way that the framework could know when to re-render your component otherwise, because it doesn't know about your polling logic.

## Rendering the order details

OK, so we're getting the order details, and we're even polling and updating that data every few seconds. But we're still not rendering it in the UI. Let's fix that. Update your `<div class="main">` as follows:

```html
<div class="main">
    @if (invalidOrder)
    {
        <h2>Nope</h2>
        <p>Sorry, this order could not be loaded.</p>
    }
    else if (orderWithStatus is null)
    {
        <text>Loading...</text>
    }
    else
    {
        <div class="track-order">
            <div class="track-order-title">
                <h2>
                    Order placed @orderWithStatus.Order.CreatedTime.ToLongDateString()
                </h2>
                <p class="ml-auto mb-0">
                    Status: <strong>@orderWithStatus.StatusText</strong>
                </p>
            </div>
            <div class="track-order-body">
                TODO: show more details
            </div>
        </div>
    }
</div>
```

This accounts for the three main states of the component:

1. If the `OrderId` value is invalid (i.e., the server reported an error when we tried to retrieve the data)
2. If we haven't yet loaded the data
3. If we have got some data to show

![Order details status](https://user-images.githubusercontent.com/1874516/77241460-a7fc4c80-6baf-11ea-80c1-3286374e9e29.png)


The last bit of UI we want to add is the actual contents of the order. To do this, we'll create another reusable component.

Create a new file, `OrderReview.razor` inside the `BlazingPizza.Client/Components` directory. The components in the web assembly project can be reused both in our Server and Client-side rendered pages.

We'll have this component receive an `Order` and render its contents as follows:

```html
@foreach (var pizza in Order.Pizzas)
{
    <p>
        <strong>
            @(pizza.Size)"
            @pizza.Special?.Name
            (£@pizza.GetFormattedTotalPrice())
        </strong>
    </p>

    <ul>
        @foreach (var topping in pizza.Toppings)
        {
            <li>+ @topping.Topping?.Name</li>
        }
    </ul>
}

<p>
    <strong>
        Total price:
        £@Order.GetFormattedTotalPrice()
    </strong>
</p>

@code {
    [Parameter, EditorRequired] public Order Order { get; set; } = new();
}
```

Finally, back in `OrderDetails.razor`, replace text `TODO: show more details` with your new `OrderReview` component:

```html
<div class="track-order-body">
    <div class="track-order-details">
        <OrderReview Order="orderWithStatus.Order" />
    </div>
</div>
```

(Don't forget to add the extra `div` with CSS class `track-order-details`, as this is necessary for correct styling.)

Finally, you have a functional order details display!

![Order details](https://user-images.githubusercontent.com/1874516/77241512-2e189300-6bb0-11ea-9740-fe778e0ce622.png)


## See it update in realtime

The backend server will update the order status to simulate an actual dispatch and delivery process. To see this in action, try placing a new order, then immediately view its details.

Initially, the order status will be *Preparing*, then after 10-15 seconds the order status will change to *Out for delivery*, then 60 seconds later it will change to *Delivered*. Because `OrderDetails` polls for updates, the UI will update without the user having to refresh the page.

## Remember to Dispose!

If you deployed your app to production right now, bad things would happen. The `OrderDetails` logic starts a polling process, but doesn't end it. If the user navigated through hundreds of different orders (thereby creating hundreds of different `OrderDetails` instances), then there would be hundreds of polling processes left running concurrently, even though all except the last were pointless because no UI was displaying their results.

You can actually observe this chaos yourself as follows:

1. Navigate to "my orders"
2. Click "Track" on any order to get to its details
3. Click "Back" to return to "my orders"
4. Repeat steps 2 and 3 a lot of times (e.g., 20 times)
5. Now, open your browser's debugging tools and look in the network tab. You should see 20 or more HTTP requests being issued every few seconds, because there are 20 or more concurrent polling processes.

This is wasteful of client-side memory and CPU time, network bandwidth, and server resources.

To fix this, we need to make `OrderDetails` stop the polling once it gets removed from the display. This is simply a matter of using the `IDisposable` interface.

In `OrderDetails.razor`, add the following directive at the top of the file, underneath the other directives:

```html
@implements IDisposable
```

Now if you try to compile the application, the compiler will complain:

```
error CS0535: 'OrderDetails' does not implement interface member 'IDisposable.Dispose()'
```

Resolve this by adding the following method inside the `@code` block:

```cs
void IDisposable.Dispose()
{
    pollingCancellationToken?.Cancel();
}
```

The framework calls `Dispose` automatically when any given component instance is torn down and removed from the UI.

Once you've put in this fix, you can try again to start lots of concurrent polling processes, and you'll see they no longer keep running after the component is gone. Now, the only component that continues to poll is the one that remains on the screen.

## Automatically navigating to order details

Right now, once users place an order, the `Home` component simply resets its state and their order appears to vanish without a trace. This is not very reassuring for users. We know the order is in the database, but users don't know that.

It would be nice if, once the order is placed, the app automatically navigated to the "order details" display for that order. This is quite easy to do.

Switch back to your `Home` component code. Add the following directive at the top:

```
@inject NavigationManager NavigationManager
```

The `NavigationManager` lets you interact with URIs and navigation state. It has methods to get the current URL, to navigate to a different one, and more.

To use this, update the `PlaceOrder` code so it calls `NavigationManager.NavigateTo`:

```csharp
async Task PlaceOrder()
{
    var newOrderId = await PizzaStore.PlaceOrder(order);
    order = new Order();
    NavigationManager.NavigateTo($"myorders/{newOrderId}");
}
```

You will need to update method signature of PlaceOrder in IRepository and implementations in HttpRepository, and EfRepository so that it returns newOrderId:

BlazingPizza.Shared/IRepository.cs:
```csharp
Task<int> PlaceOrder(Order order);
```

BlazingPizza.Client/HttpRepository.cs:
```csharp
public async Task<int> PlaceOrder(Order order)
{
    var response = await _httpClient.PostAsJsonAsync("orders", order);
    var newOrderId = await response.Content.ReadFromJsonAsync<int>();
    return newOrderId;

}
```

BlazingPizza/EfRepository.cs:
```csharp
public Task<int> PlaceOrder(Order order)
{
    throw new NotImplementedException();
}
```

Now as soon as the server accepts the order, the browser will switch to the "order details" display and begin polling for updates.

## Advanced state management

You might have noticed this already, but our application has a bug! Since we're storing the list of pizzas in the current order on the Home component, the user's state can be lost if the user leaves the Home page. To see this in action, add a pizza to the current order (don't place the order yet) - then navigate to the MyOrders page and back to Home. When you get back, you'll notice the order is empty!

## A solution

We're going to fix this bug by introducing something we've dubbed the *AppState pattern*. The *AppState pattern* adds an object to the DI container that you will use to coordinate state between related components. Because the *AppState* object is managed by the DI container, it can outlive the components and hold on to state even when the UI changes. Another benefit of the *AppState pattern* is that it leads to greater separation between presentation (components) and business logic. 

## Getting started

Create a new class called `OrderState` in the BlazingPizzaClient Project root directory - and register it as a scoped service in the DI container. In Blazor WebAssembly applications, services are registered in the `Program` class. Add the service just before the call to `await builder.Build().RunAsync();`.

```csharp
global using BlazingPizza.Shared;
global using BlazingPizza.Client;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure HttpClient to use the base address of the server project
builder.Services.AddScoped<HttpClient>(sp => 
	new HttpClient
	{
		BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
	});

builder.Services.AddScoped<IRepository, HttpRepository>();
builder.Services.AddScoped<OrderState>();


await builder.Build().RunAsync();
```

> Note: the reason why we choose scoped over singleton is for symmetry with a server-side-components application. Singleton usually means *for all users*, where as scoped means *for the current unit-of-work*.

## Updating Home Screen

Now that this type is registered in DI, we can `@inject` it into the `Home` page.

```razor
@page "/"
@inject IRepository PizzaStore
@inject OrderState OrderState
@inject NavigationManager NavigationManager
```

Recall that `@inject` is a convenient shorthand to both retrieve something from DI by type, and define a property of that type.

You can test this now by running the app again. If you try to inject something that isn't found in the DI container, then it will throw an exception and the `Home` page will fail to come up.

Now, let's add properties and methods to this class that will represent and manipulate the state of an `Order` and a `Pizza`.

Move the `configuringPizza`, `showingConfigureDialog` and `order` fields to be properties on the `OrderState` class. Make them `private set` so they can only be manipulated via methods on `OrderState`.

```csharp
public class OrderState
{
    public bool ShowingConfigureDialog { get; private set; }

    public Pizza? ConfiguringPizza { get; private set; }

    public Order Order { get; private set; } = new Order();
}
```

Now let's move some of the methods from the `Home` to `OrderState`. We won't move `PlaceOrder` into `OrderState` because that triggers a navigation, so instead we'll just add a `ResetOrder` method.

```csharp
public void ShowConfigurePizzaDialog(PizzaSpecial special)
{
    ConfiguringPizza = new Pizza()
    {
        Special = special,
        SpecialId = special.Id,
        Size = Pizza.DefaultSize,
        Toppings = new List<PizzaTopping>(),
    };

    ShowingConfigureDialog = true;
}

public void CancelConfigurePizzaDialog()
{
    ConfiguringPizza = null;

    ShowingConfigureDialog = false;
}

public void ConfirmConfigurePizzaDialog()
{
    if (ConfiguringPizza is not null)
    {
        Order.Pizzas.Add(ConfiguringPizza);
        ConfiguringPizza = null;
    }

    ShowingConfigureDialog = false;
}

public void ResetOrder()
{
    Order = new Order();
}

public void RemoveConfiguredPizza(Pizza pizza)
{
    Order.Pizzas.Remove(pizza);
}
```

Remember to remove the corresponding methods from `Home.razor`. You must also remember to remove the `order`, `configuringPizza`, and `showingConfigureDialog` fields entirely from `Home.razor`, since you'll be getting the state data from the injected `OrderState`.

At this point it should be possible to get the `Home` component compiling again by updating references to refer to various bits attached to `OrderState`. For example, the remaining `PlaceOrder` method in `Home.razor` should look like this:

```csharp
async Task PlaceOrder()
{
    var newOrderId = await PizzaStore.PlaceOrder(OrderState.Order);
    OrderState.ResetOrder();
    NavigationManager.NavigateTo($"myorders/{newOrderId}");
}
```

Feel free to create convenience properties for things like `OrderState.Order` or `OrderState.Order.Pizzas` if it feels better to you that way.

Try this out and verify that everything still works. In particular, verify that you've fixed the original bug: you can now add some pizzas, navigate to "My orders", navigate back, and your order has no longer been lost.

## Exploring state changes

This is a good opportunity to explore how state changes and rendering work in Blazor, and how `EventCallback` solves some common problems. The details of what is happening become more complicated now that `OrderState` is involved.

`EventCallback` tells Blazor to dispatch the event notification (and rendering) to the component that defined the event handler. If the event handler is not defined by a component (`OrderState`) then it will substitute the component that *hooked up* the event handler (`Home`).


## Conclusion

So let's sum up what the *AppState pattern* provides:
- Moves shared state outside of components into `OrderState`
- Components call methods to trigger a state change
- `EventCallback` takes care of dispatching change notifications

We've covered a lot of information as well about rendering and eventing:
- Components re-render when parameters change or they receive an event
- Dispatching of events depends on the event handler delegate target
- Use `EventCallback` to have the most flexible and friendly behavior for dispatching events

---

Next up - [Validation](03-validation.md)
