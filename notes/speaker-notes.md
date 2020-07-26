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
- Introduce `@code` - this is like the old `@functions` feature from `.cshtml`. Get users comfortable with the idea of defining properties, fields, methods, even nested classes
- Components are stateful so have a place to keep state in components is useful
- Introduce parameters - parameters should be public
- Introduce using a component from markup in razor - show how to pass parameters
- Introduce @inject and DI - can show how that's a shorthand for a property in @code
- Introduce http + JSON in Blazor (`GetFromJsonAsync`)
- Talk about async and the interaction with rendering 
- Introduce `OnInitializedAsync` and the common pattern of starting async work
- Introduce @layout - mention that `_Imports.razor` is the most common way to hook it up

*demos: All of the above can just be introduced with the template*

## 02 Customize a pizza

- Introduce event handlers and delegates, different options like method groups, lambdas, event args types, async
- What happens when you update the private state of a component? Walk through and event handler -> update -> re-render
- Defining your own components and accepting parameters
- Mention passing delegates to another component
- Show how re-rendering is different when using delegates, and show `EventCallback` as the fix
- Mention putting common or repeated functionality on model classes
- Introduce input elements and how manual two-way binding can be used (combination of `value` and `@onchange`)
- Show `@bind` as a shorthand for the above
- Show `@bind-value` + `@bind-value:event` as a more speciic version
- Mention that the framework tries to define `@bind` to do the default thing for common input types, but it's possible to specify what you want to bind

*demos: TodoList, with multiple levels of components*
 - Create basic todo list example with TodoItem.cs and TodoList.razor being a self-contained UI
   - See that your "add item" event handler can be an inline lambda, but it's nicer to make a method
   - See how you can make the handler async if you want (e.g., with a Task.Delay) and it re-renders correctly
 - On the textbox where the user enters a new item, also display the current value
   - See it only updates when you tab out
   - Use `@bind-value:event="oninput"`
 - Factor out a `TodoListEditor` component that takes readonly `Text` and `IsDone` parameters
   - Initially, it's a one-way binding. How do we propagate changes back to the parent?
   - Add an `IsDoneChanged` parameter and invoke a callback that manually updates model and calls `StateHasChanged`
   - Replace with `@bind-IsDone` (change param type to `EventCallback<bool>`).

## 03 Show order status

- `NavLink` in more detail, why would you use `NavLink` instead of an `<a>`
- base href and how URL navigation works in blazor (how do you generate a URL)
- @page and routing (again)
- route parameter constraints
- reminders about async, inject, http, json
- difference between `OnInitializedAsync` and `OnParametersSetAsync`
- Introduce `StateHasChanged` with the context about background processing
- introduce `@implements` - implementing an interface
- introduce `Dispose` as the counterpart to `OnInitialized`
- introduce `NavigationManager` and programmatic navigation

*demos: a counter with a timer*
 - In `NavMenu.razor`, replace all `<NavLink>` with `<a>` and see how it still works, except no highlighting
   - Switch back to `<NavLink>` and see it still renders `<a>` tags except with "active" class
   - See how you can modify the active class
   - Explain `NavLinkMatch`
   - Explain why the URLs aren't prefixed with `/` because of `<base href>`
 - Modify `Counter.razor` to take an initial `startCount` param
   - Try visiting it with a non-int param value. Add `:int` route constraint.
   - Customize the "not found" message
 - Demo programmatic navigation: In `Counter`, if the count exceeds 5, auto-navigate to `Index`
 - Recap the purpose of all the lifecycle methods, noting that there's a hidden one ("dispose")
 - In `Counter.razor`, make `OnInitialized` start up a timer that increments count *and* logs to console
   - See that if you navigate in and out repeatedly, you have multiple timers
   - Fix by implementing `IDisposable`

## 04 Refactor state management

- Talk about when components are created and disposed - this is the source of the bug in this section
- Introduce DI scopes, why you use the scoped lifetime for per-user data and how it works
- What happens when you move event handlers to a non-component class?
- Show the generated code for an event handler, how does the runtime know where to dispatch the event? (`EventCallback`)

*demos before*
 - Create a Blazor Server app
 - See that the "counter" state is lost when you navigate away and back. How could we fix this?
   - We could make the state static (see that work).
     - This is a very limiting solution because there's no control over granularity, and it's completely
       disasterous in Blazor Server.
   - Factor out the state into a `CounterState` class and make it into a singleton DI service
     - For Blazor Server, you still have the same problem as with static
     - Now make it scoped, and see that fixes it

*demos after*
 - As an alternative to using DI, you could pass the state as a CascadingValue
 - To understand one of the limitations of DI as it is, also `@inject CounterState` into `MainLayout.razor`
   and add `<button @onclick="() => { counterState.Count++; }">Increment</button>`
   - Notice how updates do *not* flow automatically into `Counter.razor`, because nothing tells the framework that
     your actions against a DI service in one component may affect another component
 - Remove the DI service for CounterState and see it now fails at runtime
 - In `MainLayout.razor`, add a `@code` block declaring a field with value `new CounterState()`
   - Also surround `@Body` with `<CascadingValue Value=@counterState>` - see it work
 - As well as providing a subtree-scoped value, CascadingValue takes care of triggering a re-render of any
   subscriber when the supplied value may have changed.
   - See how the "increment" button in `MainLayout` causes an update to `Counter` now
 - Pros of using CascadingValue for shared state:
   - It's subtree-scoped, not one-per-type-per-user.
   - You control the instantiation, not the DI system
   - Value changes trigger re-rendering automatically in subscribers
 - Cons:
   - Doesn't do constructor injection automatically like DI system does
   - No single central point for configuration

## 05 Checkout with Validation

- Introduce `EditForm` and input components

*demos-before: todolist with validation*

 - Have a TodoList page, but for each item also track an "importance"
   number and have the list auto-sort by importance
   - Have DataAnnotations attributes on the `TodoItem` class
   - Show you can add empty-string items
   - Show the <input type=number> doesn't stop submission if you type in
     bizarre input like `-------` or `15+3`, but when adding the item it
     got reset to 0, which doesn't make sense as UX
 - Explain: Blazor has a general built-in validation system that's designed
   for extensibility and even to allow you to replace it completely
 - Change `<form>` to `<EditForm>` and explain its responsibilities
   - Set `Model="newItem"` where `newItem` is the a `TodoItem` field
   - See it offers `OnSubmit`, `OnValidSubmit`, `OnInvalidSubmit`
   - Wire up `OnValidSubmit` to your submission method
 - See that, at first, it still allows submission of arbitrary junk
 - Need to add `<DataAnnotationsValidator />` to the form
   - Now see you can't submit junk, but still doesn't display reasons
 - Add `<ValidationSummary />` and see it displays reasons
 - Replace form fields with Blazor ones `<input>` => `<InputText>` etc
   - See it now updates validation state on each change
 - Replace `<ValidationSummary>` with `<ValidationMessage>` for each field
 - If you want, customise a validation message

*demos-after: tri-state checkbox OR slider component*

 - How would you create a brand-new input component that integrates with validation?
   - Look at InputBase.cs in project repo
   - Explain you can inherit from this. Your responsiblity is to provide the
     rendered markup, and how to format/parse the value you've been given as a string.
     For example, for some underlying HTML5 inputs, the browser deals with culture
     variant values, and for others culture invariant ones, so you have to control
     this exchange of data with the browser
 - Example: InputSlider
   - CurrentValueAsString represents the value being given to the browser or being
     received from it. This is usually what you use with `@bind` with the HTML.
   - CssClass is computed by the framework and combines the user's supplied class
     along with standard validation status classes
   - AdditionalAttributes should be used with `@attributes` on the output if it makes
     sense to add aribtrary user-supplied attributes to a particular element.
     Explain "last wins" rule.

## 06 Add Authentication

- All security enforcement is on server. So what's the point of doing anything with auth in the client?
  - It's to provide a nice UX. Tell the user if they are logged in, and if so as who, and what features they may access.
  - It's also about being able to log in a user and collect, hold, and send authentication tokens to the server in a robust and safe way.
- The workshop code will use an OpenID Connect-based flow for acquiring tokens, which is built into Blazor WebAssembly's project templates. The user accounts will be stored in the server-side database. However, other options are also supported:
  - Instead of using your ASP.NET Core server as the OIDC provider, you can connect to an external OIDC-compliant login system such as AzureAD, Google login, etc.
  - (Note: OpenID Connect (OIDC) is a protocol for logging in with an external identity provider and getting back an auth token that identifies you to other services. This is very flexible and pretty much industry standard for SPAs, and fixes the complicated problems inherent to cookie-based auth.)
  - Instead of using OIDC, you can use the lower-level auth APIs to provide info about the user authentication state directly, based on whatever custom logic you have for determining who a user is logged in as

*demos*
 - Start with a Blazor WebAssembly app into which you've:
   - Referenced the package `Microsoft.AspNetCore.Components.WebAssembly.Authentication`
   - In `_Imports.razor`, added `Microsoft.AspNetCore.Authorization` and `Microsoft.AspNetCore.Components.Authorization`
 - First you want to show the name of the logged in user. In `MainLayout.razor`, inside the top bar, add:
 
```razor
<AuthorizeView>
    <Authorized>
        <strong>Hello, @context.User.Identity.Name!</strong>
    </Authorized>
    <NotAuthorized>
        You're not logged in. Please log in!
    </NotAuthorized>
</AuthorizeView>
```

 - At first this gives an exception (no cascading auth state). Fix by adding `<CascadingAuthenticationState>` around everything in `App.razor`
 - Now you get a different exception about missing `AuthenticationStateProvider`.
   - This makes sense. How is the system supposed to know who the user is unless you tell it?
 - Now register a fake auth state provider in DI:

```cs
class MyFakeAuthenticationStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Hard-coded logged-out user
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

        return Task.FromResult(new AuthenticationState(claimsPrincipal));
    }
}
```
 
 - Now the UI shows the user is logged out
 - Next want to limit access to `Counter.razor` to logged in users
   - Add `[Authorize]` there - see it has no effect
   - Fix by changing router to use `AuthorizeRouteView` - see it works now
   - Customize the `NotAuthorized` template
 - Next want to hide the counter link from nav menu if logged out
   - Do it using `<AuthorizeView>`
 - OK so that handles logged-out users. What happens if the user is logged in?
   - Change `MyFakeAuthenticationStateProvider` to have a hardcoded logged-in user:
   
```cs
var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] {
  new Claim(ClaimTypes.Name, "Bert"),
  //new Claim(ClaimTypes.Role, "superadmin")
 }, "fake"));
```

  - See the UI displays this. So far we've seen two ways to reflect auth state in the UI: `[Authorize]` and `<AuthorizeView>`. But what if you want to use auth state programmatically during logic?
  - Let's limit the counter to 3 except if you're a `superadmin`
  - Add UI to show this on counter page:
  
```razor
<AuthorizeView Roles="superadmin">
    <Authorized>
        <p>You're a superadmin, so you can count as high as you like</p>
    </Authorized>
    <NotAuthorized>
        <p>You're a loser, so you can only count up to 3</p>
    </NotAuthorized>
</AuthorizeView>
```
 - Implement by adding `[CascadingParameter] public Task<AuthenticationState> AuthStateTask { get; set; }` and then inside the count logic, `await` it to get `authState` and then check `if (currentCount < 3 || authState.User.IsInRole("superadmin"))`
 - Summarize:
   - Three ways to interact with auth system
   - Various pieces working together behind the scenes to provide and update the auth state info
   - Although it looked messy to set it up in this demo, it's all set up for you by default in the project template. We only built it manually here so you could see the pieces step by step.
 - Now what about real auth, not the hardcoded auth state?
   - Create a new project: `dotnet new blazorwasm --hosted --auth Individual -o MyRealAuthApp`
   - Go through the UI flow of registering and logging in
   - See how the weather data is now protected on the server-side
   - See how we include the auth token with requests for it now
   - See in `Program.cs` how this is configured

## 07 JavaScript Interop

- Introduce JS Interop and `IJSRuntime` with a simple example (like showing a JavaScript `alert`)
- When to do JS Interop (relative to component lifecycle), `OnAfterRender` and in response to events
- Talk about how `IJSRuntime` is async for everything, why that's important, what to do if you need low-level synchronous interop
- Introduce component libraries and project-bundled static content
- Show how component library content files are editable while running
- Reminder about component namespaces
- note: This section can include some demos and examples of more advanced JS interop cases like calling .NET from JS or using `DotNetObjectRef<>`, the actual usage of JS interop in the workshop is pretty simple

*demos-before: alert()*
*demos-after: DotNetObjectRef*

## 08 Templated Components

- Bring up component libraries and review the project content, last section used and existing library, this section creates a new one.
- Introduce `RenderFragment`, talk about how its used to pass markup and rendering logic to other components, recall examples like `AuthorizeView` and the `MainLayout`
- Show a simple example that renders a `ChildContent` property
- Talk about what happens when you have multiple `RenderFragment` parameters
- Show example of a `RenderFragment<>` that requires an argument 
- Introduce generics with `@typeparam` and compare to a generic class written in C#
- Introduce type inference and show examples of using inference vs specifying the type

*demo: material design components*

## 09 Progressive Web Apps
 - Demo all the ways of shipping an app (wasm, server, electron, pwa, webwindow)
   and talk about pros/cons and capabilities of each
 - Possible also demo deploying to Azure
  
## Appendix A: EventCallback - supplement to part 04

First, we need to review how event dispatching interacts with rendering. Components will automatically re-render (update the DOM) when their parameters have changed, or when they receive an event (like `@onclick`). This generally works for the most common cases. This also makes sense because it would be infeasible to rerender the entire UI each time an event happens - Blazor has to make a decision about what part of the UI should update.

An event handler is attached to a .NET `Delegate` and the component that receives the event notification is defined by [`Delegate.Target`](https://docs.microsoft.com/en-us/dotnet/api/system.delegate.target?view=netframework-4.8#System_Delegate_Target). Roughly-defined, if the delegate represents an instance method of some object, then the `Target` will be the object instance whose method is being invoked. 

In the following example the event handler delegate is `TestComponent.Clicked` and the `Delegate.Target` is the instance of `TestComponent`.

```html
@* TestComponent.razor *@
<button @onclick="Clicked">Click me!</Clicked>
<p>Clicked @i times!</p>
@code {
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
@code {
    [Parameter] public Action Clicked { get; set; }
}

@* TestComponent2.razor *@
<CoolButton Clicked="Clicked" />
<p>Clicked @i times!</p>
@code {
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
@code {
    [Parameter] public Action Clicked { get; set; }
}

@* TestComponent3.razor *@
@inject TestState State
<CoolButton Clicked="@State.Clicked" />
<p>Clicked @State.Count times!</p>
```

In this third example the event handler delegate is `TestState.Clicked` and the so `Delegate.Target` is `TestState` - **not a component**. When the button is clicked, no component gets the event notification, and so nothing will rerender.

This is the problem that `EventCallback` was created to solve. By changing the parameter on `CoolButton` from `Action` to `EventCallback` you fix the event dispatching behavior. This works because `EventCallback` is known to the compiler, when you create an `EventCallback` from a delegate that doesn't have its `Target` set to a component, then the compiler will pass the current component to receive the event.

----

Let's jump back to our application. If you like you can reproduce the problem that's been described here by changing the parameters of `ConfigurePizzaDialog` from `EventCallback` to `Action`. If you try this you can see that cancelling or confirming the dialog does nothing. This is because our use case is exactly like the third example above:

```html
@* from Index.razor *@
@if (OrderState.ShowingConfigureDialog)
{
    <ConfigurePizzaDialog
        Pizza="OrderState.ConfiguringPizza"
        OnConfirm="OrderState.ConfirmConfigurePizzaDialog" 
        OnCancel="OrderState.CancelConfigurePizzaDialog" />
}
```

For the `OnConfirm` and `OnCancel` parameters, the `Delegate.Target` will be `OrderState` since we're passing reference to methods defined by `OrderState`. If you're using `EventCallback` then the special logic of the compiler kicks in and it will specify additional information to dispatch the event to `Index`.
