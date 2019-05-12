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
- Introduce input elements and how manual two-way binding can be used (combination of `value` and `onchange`)
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