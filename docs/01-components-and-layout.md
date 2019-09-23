# Components and layout

In this session, you'll get started building a pizza store app using Blazor. The app will enable users to order pizzas, customize them, and then track the order deliveries.

## Pizza store starting point

We've set up the initial solution for you for the pizza store app in this repo. Go ahead and clone this repo to your machine. You'll find the [starting point](https://github.com/dotnet-presentations/blazor-workshop/tree/master/save-points/00-Starting-point) in the *save-points* folder along with the end state for each session.

The solution already contains four projects:

![image](https://user-images.githubusercontent.com/1874516/57006654-3e3e1300-6b97-11e9-8053-b6ec9c31614d.png)

- **BlazingPizza.Client**: This is the Blazor project. It contains the UI components for the app.
- **BlazingPizza.Server**: This is the ASP.NET Core project hosting the Blazor app and also the backend services for the app.
- **BlazingPizza.Shared**: Shared model types for the app.
- **BlazingPizza.ComponentsLibrary**: A library of components and helper code to be used by the app in later sessions.

The **BlazingPizza.Server** project should be set as the startup project.

When you run the app, you'll see that it currently only contains a simple home page.

![image](https://user-images.githubusercontent.com/1874516/51783774-afcb7880-20f3-11e9-9c22-2f330380ff1e.png)

Open *Pages/Index.razor* in the **BlazingPizza.Client** project to see the code for the home page.

```
@page "/"

<h1>Blazing Pizzas</h1>
```

The home page is implemented as a single component. The `@page` directive specifies that the Index component is a routable page with the specified route. 

## Display the list of pizza specials

First we'll update the home page to display the list of available pizza specials. The list of specials will be part of the state of the `Index` component.

Add a `@code` block to *Index.razor* with a list field to keep track of the available specials:

```csharp
@code {
    List<PizzaSpecial> specials;
}
```

The code in the `@code` block is added to the generated class for the component. The `PizzaSpecial` type is already defined for you in the Shared project.

To get the available list of specials we need to call an API on the backend. Blazor provides a preconfigured `HttpClient` through dependency injection that is already setup with the correct base address. Use the `@inject` directive to inject an `HttpClient` into the `Index` component.

```
@page "/"
@inject HttpClient HttpClient
```

The `@inject` directive essentially defines a new property on the component where the first token specified the property type and the second token specifies the property name. The property is populated for you using dependency injection.

Override the `OnInitializedAsync` method in the `@code` block to retrieve the list of pizza specials. This method is part of the component lifecycle and is called when the component is initialized. Use the `GetJsonAsync<T>()` method to handle deserializing the response JSON:

```csharp
@code {
    List<PizzaSpecial> specials;

    protected async override Task OnInitializedAsync()
    {
        specials = await HttpClient.GetJsonAsync<List<PizzaSpecial>>("specials");
    }
}
```

The `/specials` API is defined by the `SpecialsController` in the Server project.

Once the component is initialized it will render its markup. Replace the markup in the `Index` component with the following to list the pizza specials:

```html
<div class="main">
    <ul class="pizza-cards">
        @if (specials != null)
        {
            @foreach (var special in specials)
            {
                <li style="background-image: url('@special.ImageUrl')">
                    <div class="pizza-info">
                        <span class="title">@special.Name</span>
                        @special.Description
                        <span class="price">@special.GetFormattedBasePrice()</span>
                    </div>
                </li>
            }
        }
    </ul>
</div>
```

Run the app by hitting `Ctrl-F5`. Now you should see a list of the specials available.

![image](https://user-images.githubusercontent.com/1874516/57006743-1602e400-6b98-11e9-96cb-ff4829cf459f.png)

## Create the layout

Next we'll set up the layout for app. 

Layouts in Blazor are also components. They inherit from `LayoutComponentBase`, which defines a `Body` property that can be used to specify where the body of the layout should be rendered. The layout component for our pizza store app is defined in *Shared/MainLayout.razor*.

```html
@inherits LayoutComponentBase

<div class="content">
    @Body
</div>
```

To see how the layout is associated with your pages, look at the `<Router>` component in `App.razor`. Notice the `DefaultLayout` parameter which determines the layout used for any page that doesn't specify its own layout directly.

You can also override this `DefaultLayout` on a per-page basis. To do so, you can add directive such as `@layout SomeOtherLayout` at the top of any `.razor` page component. However, you don't need to do so in this application.

Update the `MainLayout` component to define a top bar with a branding logo and a nav link for the home page:

```html
@inherits LayoutComponentBase

<div class="top-bar">
    <img class="logo" src="img/logo.svg" />

    <NavLink href="" class="nav-tab" Match="NavLinkMatch.All">
        <img src="img/pizza-slice.svg" />
        <div>Get Pizza</div>
    </NavLink>
</div>

<div class="content">
    @Body
</div>
```

The `NavLink` component is provided by Blazor. Components can be used from components, which is done by specifying an element with the component's type name along with attributes for any component parameters.

The `NavLink` component is the same as an anchor tag, except that it adds an `active` class if the current URL matches the link address. `NavLinkMatch.All` means that the link should be active only when it matches the entire current URL (not just a prefix). We'll examine the `NavLink` component in more detail in a later session.

Run the app by hitting `Ctrl-F5`. With our new layout our pizza store app now looks like this:

![image](https://user-images.githubusercontent.com/1874516/57006730-e81d9f80-6b97-11e9-813d-9c35b62efa53.png)


Next up - [Customize a pizza](02-customize-a-pizza.md)
