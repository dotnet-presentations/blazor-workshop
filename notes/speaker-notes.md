# Speaker notes

This is a rough guide of what topics are best to introduce with each section.

## 00 Starting point

- Introduce presenters
- Have attendees introduce themselves if the group is small
- What is Blazor
- Current Status and future roadmap for Blazor/Components (if appropriate)
- Machine Setup
- Create a hello world project

## 01 Components and layout

- Introduce @page - explain the difference between routable and non-routable
- Show the Router component in App.razor
- Introduce @functions - this is an old feature but isn't commonly used in Razor. Get users comfortable with the idea of defining properties, fields, methods, even nested classes
- Components are stateful so have a place to keep state in components is useful
- Introduce parameters - parameters should be non-public
- Introduce using a component from markup in razor - show how to pass parameters
- Introduce @inject and DI - can show how that's a shorthand for a property in @functions
- Introduce http + JSON in Blazor (`GetJsonAsync`)
- Talk about async and the interaction with rendering 
- Introduce `OnInitAsync` and the common pattern of starting async work
- Introduce @layout - mention that `_Imports.razor` is the most common way to hook it up
- Introduce NavLink and talk about various `NavLinkMatch` options

## 02 Customize a pizza

- Introduce event handlers and delegates, different options like method groups, lambdas, event args types, async
- What happens when you update the private state of a component? Walk through and event handler -> update -> re-render
- Defining your own components and accepting arguments
- Mention passing delegates to another component
- Show how re-rendering is different when using delegates, and show `EventCallback` as the fix
- Mention putting common or repeated functionality on model classes
- Introduce input elements and how manual two-way binding can be used (combination of `value` and `@onchange`)
- Show `bind` as a shorthand for the above
- Show `bind-value-onchange` as a more speciic version
- Mention that the framework tries to define `bind` to do the default thing for common input types, but it's possible to specify what you want to bind

## 03 Show order status

- `NavLink` in more detail, why would you use `NavLink` instead of an `<a>`
- base href and how URL navigation works in blazor (how do you generate a URL)
- @page and routing (again)
- route parameter constraints
- reminders about async, inject, http, json
- difference between `OnInitAsync` and `OnParametersSetAsync`
- Introduce `StateHasChanged` with the context about background processing
- introduce `@implements` - implementing an interfact
- introduce `Dispose` as the counterpart to `OnInit`
- introduce `IUriHelper` and programmatic navigation

## Appendix A: EventCallback - suppliment to part 04

-----

First, we need to review how event dispatching interacts with rendering. Components will automatically re-render (update the DOM) when their parameters have changed, or when they recieve an event (like `@onclick`). This generally works for the most common cases. This also makes sense because it would be infeasible to rerender the entire UI each time an event happens - Blazor has to make a decision about what part of the UI should update.

An event handler is attached to a .NET `Delegate` and the component that recieves the event notification is defined by [`Delegate.Target`](https://docs.microsoft.com/en-us/dotnet/api/system.delegate.target?view=netframework-4.8#System_Delegate_Target). Roughly-defined, if the delegate represents an instance method of some object, then the `Target` will be the object instance whose method is being invoked. 

In the following example the event handler delegate is `TestComponent.Clicked` and the `Delegate.Target` is the instance of `TestComponent`.

```html
@* TestComponent.razor *@
<button @onclick="@Clicked">Click me!</Clicked>
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
<button @onclick="Clicked">Clicking this will be cool!</button>
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
<button @onclick="Clicked">Clicking this will be cool!</button>
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
