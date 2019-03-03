# Refactor state management

In this section we'll revisit some of the code we've already written and try to make it nicer. We'll also talk more about eventing and how events cause the UI to update.

## A problem

You might have noticed this already, but our application has a bug! Since we're storing the list of pizzas in the current order on the Index component, the users state can be lost if the user leaves the Index page. To see this in action, add a pizza to the current order (don't place the order yet) - then navigate to the MyOrders page and back to Index. When you get back, you'll notice the order is empty!

## A solution

We're going to fix this bug by introducing something we've dubbed the *AppState pattern*. The basics are that you want to add a object to DI container that you will use to coordinate state between related components. Bacause the *AppState* object is managed by the DI container, it can outlive the components and hold on to state even when the UI is changing a lot. Another benefit of the *AppState pattern* is that it lead to greater separation between presentation (components) and logic 

## Getting started

Create a new class called `OrderState` - and register it as a scoped service in the DI container. Much like an ASP.NET Core method, a Blazor application has a `Startup` class and a `ConfigureServices` method. Add the service in `Startup.cs`.

```C#
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<OrderState>();
}
```

note: the reason why we choose scoped over singleton is for symmetry with a server-side-components application. Singleton usually means *for all users*, where as scoped means *for the current unit-of-work*. 

## Updating Index

Now that this type is registered in DI, we can `@inject` it into the Index page.

```
@page "/"
@inject HttpClient HttpClient
@inject OrderState OrderState
@inject IUriHelper UriHelper
@implements IDisposable
```

Recall that `@inject` is a convenient shorthand to both retrieve something from DI by type, and define a property of that type.

You can test this now by running the app again. If you try to inject something that isn't found in the DI container, then it will throw an exception and the Index will fail to come up.

-------

Now, let's add properties and methods to this class that will represent and manipulate the state of an `Order` and a `Pizza`.

Move the `configuringPizza`, `showingConfigureDialog` and `Order` to be properties on the `OrderState` class. I like to make them `private set` so they can only be manipulated via methods on `OrderState`.

```C#
public class OrderState
{
    public bool ShowingConfigureDialog { get; private set; }

    public Pizza ConfiguringPizza { get; private set; }

    public Order Order { get; private set; } = new Order();
}
```

Now let's move some of the methods from the `Index` to `OrderState`. We won't move PlaceOrder into OrderState because that triggers a navigation, so instead we'll just add a ResetOrder method.

```C#
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
    Order.Pizzas.Add(ConfiguringPizza);
    ConfiguringPizza = null;

    ShowingConfigureDialog = false;
}

public void ResetOrder()
{
    Order = new Order();
}
```

## Updating ConfigurePizzaDialog

As a next step, let's give the `ConfigurePizzaDialog` the same treatment.

First inject `OrderState` into the `ConfigurePizzaDialog` by adding `@inject OrderState OrderState` at the top. Then you can remove the parameter properties (i.e., the ones with the [Parameter] attribute). We no longer need the parameters because `ConfigurePizzaDialog` will get its data from `OrderState` instead. 

Also make sure to go back to `Index` and remove the arguments to the `<ConfigurePizzaDialog />`. If you see then exception message like "Component ConfigurePizzaDialog does not have a parameter named 'Pizza'." then this is the problem.

Now you can add the `AddTopping` and `RemoveTopping` methods to `OrderState`.

```C#
public void AddTopping(Topping topping)
{
    if (ConfiguringPizza.Toppings.Find(pt => pt.Topping == topping) == null)
    {
        ConfiguringPizza.Toppings.Add(new PizzaTopping() { Topping = topping });
    }
}

public void RemoveTopping(Topping topping)
{
    ConfiguringPizza.Toppings.RemoveAll(pt => pt.Topping == topping);
}
```

## Flowing state changes

At this point it should be possible to get the `Index` and `ConfigurePizzaDialog` components compiling again by updating references to refer to various bits attached to `OrderState`. Feel free to create convenience properties for things like `OrderState.Order.Pizzas` or `OrderState.ConfiguringPizza` if it feels better to you that way.

Let's run it now that it builds. So, it shouldn't crash (that's good) but you'll notice that UI actions like trying to add a pizza to the order don't update the UI. The problem is that when you click - that's going to be triggered on `CustomizePizzaDialog`, but we need to re-render its parent (`Index`). 

The previous solution was to put a call to `StateHasChanged` inside the method:
```C#
void ConfirmConfigurePizzaDialog()
{
    order.Pizzas.Add(configuringPizza);
    configuringPizza = null;

    showingConfigureDialog = false;
    StateHasChanged();
}
```

To understand this, we need to review how event dispatching interacts with rendering. Components will automatically re-render (update the DOM) when their parameters have changed, or when they recieve an event (like `onclick`). This generally works for the most common cases.

A case that isn't handled is when a event needs to cause an *ancestor* or unrelated component to also re-render. One of these cases is the `ConfirmConfigurePizzaDialog` delegate which is triggered by a button on `ConfigurePizzaDialog`. So the sequence of actions by default is:
```
button is clicked
ConfirmConfigurePizzaDialog executes and modifies some properties
ConfigurePizzaDialog is automatically re-rendered
Index is not automatically re-rendered (even though we wish it would)
``` 

This is why when we defined `ConfirmConfigurePizzaDialog` we put in a call to `StateHasChanged`. `StateHasChanged` is the way to trigger a manual re-render.

For now the easiest way to think about it is... *if I am passing a delegate to another component, and I want it to update my UI, reach for StateHasChanged*. This pattern is fairly common so it's important to know about. We're looking to improve the situation further in the future.

In our case, we've still got a problem because the methods we're passing around are defined on the `OrderState`. So we need a way for `OrderState` to trigger an update on any component that depends upon it. 

-------

Next, let's can make it possible for `OrderState` to produce its own state-change notifications.

Let's define a .NET event on `OrderState`
```C#
public event EventHandler StateChanged;

private void StateHasChanged()
{
    StateChanged?.Invoke(this, EventArgs.Empty);
}
```

Now we can add a call to `StateHasChanged` to methods on `OrderState` that need it (`CancelConfigurePizzaDialog` and `ConfirmConfigurePizzaDialog`, and `RemoveConfiguredPizza`).

To hook up the other side, we should subscribe to the change notifications on `Index`
```
@implements IDisposable

...

@functions {
    ...

    protected async override Task OnInitAsync()
    {
        OrderState.StateChanged += OnOrderStateChanged;
        specials = await HttpClient.GetJsonAsync<List<PizzaSpecial>>("/specials");
    }

    void IDisposable.Dispose()
    {
        OrderState.StateChanged -= OnOrderStateChanged;
    }

    void OnOrderStateChanged(object sender, EventArgs e) => StateHasChanged();
}
```

This is just a normal .NET event handler, and the `Index` component will unsubscribe from changes once it gets disposed.

note: You might be wondering whether it's necessary to make `ConfigurePizzaDialog` subscribe to `OrderState.StateChanged` too. As it happens, there's no need in this case. That's because `ConfigurePizzaDialog` only ever needs to re-render in response to its own DOM events, and the framework automatically triggers a re-render for you in that case. This is different from `Index`, because `Index` also needs to re-render after `ConfigurePizzaDialog` has modified `OrderState`.

## Conclusion

Now if you test our bug again, it should be fixed. 

So let's sum up what the *AppState pattern* provides:
- Moves shared state outside of components into OrderState
- Components call methods to trigger a state change
- OrderState publishes change notifications when state has change
- Components subscribe to the state change notifications and re-render

We've covered a lot of information as well about rendering and eventing:
- Components re-render when parameters change or they recieve an event
- Events work well enough for most scenarios
- You may need to trigger a manual update with `StateHasChanged` when passing a delegate to another component
- The *AppState pattern* can also implement change notifications
- Components that attach to a .NET event should also take care to unsubscribe

Next up - [Authentication and authorization](05-authentication-and-authorization.md)
