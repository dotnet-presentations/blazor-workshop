# Refactor state management

In this section we'll revisit some of the code we've already written and try to make it nicer. We'll also talk more about eventing and how events cause the UI to update.

## A problem

You might have noticed this already, but our application has a bug! Since we're storing the list of pizzas in the current order on the Index component, the user's state can be lost if the user leaves the Index page. To see this in action, add a pizza to the current order (don't place the order yet) - then navigate to the MyOrders page and back to Index. When you get back, you'll notice the order is empty!

## A solution

We're going to fix this bug by introducing something we've dubbed the *AppState pattern*. The basics are that you want to add an object to the DI container that you will use to coordinate state between related components. Because the *AppState* object is managed by the DI container, it can outlive the components and hold on to state even when the UI is changing a lot. Another benefit of the *AppState pattern* is that it leads to greater separation between presentation (components) and business logic. 

## Getting started

Create a new class called `OrderState` in the Client Project root directory - and register it as a scoped service in the DI container. Much like an ASP.NET Core method, a Blazor application has a `Startup` class and a `ConfigureServices` method. Add the service in `Startup.cs`.

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

public void RemoveConfiguredPizza(Pizza pizza)
{
    Order.Pizzas.Remove(pizza);
}
```

## Exploring state changes

At this point it should be possible to get the `Index` component compiling again by updating references to refer to various bits attached to `OrderState`. Feel free to create convenience properties for things like `OrderState.Order` or `OrderState.Order.Pizzas` if it feels better to you that way.

Try this out and verify that everything still works.

This is a good opportunity to explore how state changes and rendering work in Blazor, and how `EventCallback` solves some common problems. The detail of what are happening now became more complicated now that `OrderState` involved.

-----

First, we need to review how event dispatching interacts with rendering. Components will automatically re-render (update the DOM) when their parameters have changed, or when they recieve an event (like `onclick`). This generally works for the most common cases. This also makes sense because it would be infeasible to rerender the entire UI each time an event happens - Blazor has to make a decision about what part of the UI should update.

An event handler is attached to a .NET `Delegate` and the component that recieves the event notification is defined by [`Delegate.Target`](https://docs.microsoft.com/en-us/dotnet/api/system.delegate.target?view=netframework-4.8#System_Delegate_Target). Roughly-defined, if the delegate represents an instance method of some object, then the `Target` will be the object instance whose method is being invoked. 

In the following example the event handler delegate is `TestComponent.Clicked` and the `Delegate.Target` is the instance of `TestComponent`.

```html
@* TestComponent.razor *@
<button onclick="@Clicked">Click me!</Clicked>
<p>Clicked @i times!</p>
@functions {
    int i;
    void Clicked()
    {
        i++;
    }
}
```

Since usually the purpose of an event handler is to call a method that updates the private state of a component, it makes sense that Blazor would want to rerender the component that defines the event handler. The previous example is the most simple use of an event handler, and it makes sense that we'd want to rerender `TestComponent` in this case.

Now let's consider what happens when we want an event to rerender an *ancestor* component. This is similar to what happens with `Index` and `ConfigurePizzaDialog` - and it *just works* for the typical case where the event handler is a parameter. This example will use `Action` instead of `EventCallback` since we're building up to an explanation of why `EventCallback` is needed.

```html
@* CoolButton.razor *@
<button onclick="Clicked">Clicking this will be cool!</button>
@functions {
    [Parameter] Action Clicked { get; set; }
}

@* TestComponent2.razor *@
<CoolButton Clicked="@Clicked" />
<p>Clicked @i times!</p>
@functions {
    int i;
    void Clicked()
    {
        i++;
    }
}
```

In the this example the event handler delegate is `TestComponent2.Clicked` and the `Delegate.Target` is the instance of `TestComponent` - even though it's `CoolButton` that actually defines the event. This means that `TestComponent2` will be rerendered when the button is clicked. This makes sense because if `TestComponent2` didn't get rerendered, you couldn't update the count.

Let's see a third example to show how this falls apart. There are cases where `Delegate.Target` isn't a component at all, and so nothing will rerender. Let's see that example again, but with an *AppState* object:

```C#
public class TestState
{
    public int Count { get; private set; }

    public void Clicked()
    {
        Count++;
    }
}
```

```html
@* CoolButton.razor *@
<button onclick="Clicked">Clicking this will be cool!</button>
@functions {
    [Parameter] Action Clicked { get; set; }
}

@* TestComponent3.razor *@
@inject TestState State
<CoolButton Clicked="@State.Clicked" />
<p>Clicked @State.Count times!</p>
```

In this third example the event handler delegate is `TestState.Clicked` and the so `Delegate.Target` is `TestState` - **not a component**. When the button is clicked, no component gets the event notification, and so nothing will rerender.

This is the problem that `EventCallback` was created to solve. By changing the parameter on `CoolButton` from `Action` to `EventCallback` you fix the event dispatching behavior. This works because `EventCallback` is known to the compiler, when you create an `EventCallback` from a delegate that doesn't have its `Target` set to a component, then the compiler will pass the curent component to recieve the event.

----

Let's jump back to our application. If you like you can reproduce the problem that's been described here by changing the parameters of `ConfigurePizzaDialog` from `EventCallback` to `Action`. If you try this you can see that cancelling or confirming the dialog does nothing. This is because our use case is exactly like the third example above:

```html
@* from Index.razor *@
@if (OrderState.ShowingConfigureDialog)
{
    <ConfigurePizzaDialog
        Pizza="@OrderState.ConfiguringPizza"
        OnConfirm="@OrderState.ConfirmConfigurePizzaDialog" 
        OnCancel="@OrderState.CancelConfigurePizzaDialog" />
}
```

For the `OnConfirm` and `OnCancel` parameters, the `Delegate.Target` will be `OrderState` since we're passing reference to methods defined by `OrderState`. If you're using `EventCallback` then the special logic of the compiler kicks in and it will specify additional information to dispatch the event to `Index`.

## Conclusion

So let's sum up what the *AppState pattern* provides:
- Moves shared state outside of components into OrderState
- Components call methods to trigger a state change
- `EventCallback` takes care of dispatching change notifications

We've covered a lot of information as well about rendering and eventing:
- Components re-render when parameters change or they recieve an event
- Dispatching of events depends on the event handler delegate target
- Use `EventCallback` to have the most flexible and friendly behavior for dispatching events

Next up - [Authentication and authorization](05-authentication-and-authorization.md)
