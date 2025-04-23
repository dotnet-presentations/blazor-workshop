# Home Screen

In this session, you'll get started building a pizza store app using Blazor. The app will enable users to order pizzas, customize them, and then track the order deliveries.

When we think about what this type of application does, it has a very static home page that presents a menu but then pivots to being very interactive when a customer starts placing an order.  We'll use this information about application behavior to guide our development.

## Pizza store starting point

We've set up the initial solution for you for the pizza store app in this repo. Go ahead and clone this repo to your machine. You'll find the [starting point](../modules/0-Start) in the *save-points* folder along with the end state for each session.

The solution already contains four projects:

![image](https://user-images.githubusercontent.com/1874516/77238114-e2072780-6b8a-11ea-8e44-de6d7910183e.png)


- **BlazingPizza**: This is the ASP.NET Core project hosting the Blazor app, runs on a webserver, and also the backend services for the app.
- **BlazingPizza.Client**: This is the Blazor project. It contains the UI components for the app that will run with WebAssembly
- **BlazingPizza.Shared**: This project contains the shared model types for the app.
- **BlazingPizza.ComponentsLibrary**: This is a library of components and helper code to be used by the app in later sessions.

The **BlazingPizza** project should be set as the startup project.

### Introducing the Repository pattern

This application uses a repository pattern to provide access to the data in a sqlite database that will be created with the `BlazingPizza/SeedData.cs` file.  For fun, go ahead and add some of your favorite and fun toppings to the list to customize the data for you:

```csharp
...
    new Topping()
        {
            Id=22,
            Name = "Blue cheese",
            Price = 2.50m,
        },
    new Topping()
        {
            Id=23,
            Name = "M & Ms",
            Price = 2.50m,
        },
    new Topping()
        {
            Id=24,
            Name = "Rainbow Sprinkles",
            Price = 1.25m
        }
};
```

This data is read and presented through a Repository interface conveniently called IRepository and resides in the `BlazingPizza.Shared/IRepository.cs` file:

```csharp
public interface IRepository
{

	Task<List<Topping>> GetToppings();

	Task<List<PizzaSpecial>> GetSpecials();

	Task<List<OrderWithStatus>> GetOrdersAsync();

	Task<OrderWithStatus> GetOrderWithStatus(int orderId);

}
```

There is an implementation of this repository sitting in the `BlazingPizza` project called `EfRepository` that will interact with the database and return data appropriately.  It has already been registered with the service locator in `Program.cs` for you:

```csharp
builder.Services.AddScoped<IRepository, EfRepository>();
```

Additionally, there are APIs built using the minimal API pattern and residing in the `BlazingPizza.PizzaApiExtensions.cs` file.

See also [Create a minimal web API with ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-8.0) for additional details.

### Running the app for the first time

When you run the app, you'll see that it currently only contains a simple home page.

![image](https://user-images.githubusercontent.com/1874516/77238160-25fa2c80-6b8b-11ea-8145-e163a9f743fe.png)

Open *Components/Pages/Home.razor* in the **BlazingPizza** project to see the code for the home page.

```
@page "/"

<h1>Blazing Pizzas</h1>
```

The home page is implemented as a single component. The `@page` directive specifies that the `Home` component is a routable page with the specified route.

## Display the list of pizza specials

First we'll update the home page to display the list of available pizza specials. The list of specials will be part of the state of the `Home` component.

Add a `@code` block to *Home.razor* with a list field to keep track of the available specials:

```csharp
@code {
    List<PizzaSpecial>? specials;
}
```

The code in the `@code` block is added to the generated class for the component at compile-time. The `PizzaSpecial` type is already defined for you in the **BlazingPizza.Shared** project.

To get the available list of specials we need to reach into our repository and get data from the database. We'll use the repository object already defined and access it with dependency injection. Use the `@inject` directive to inject an `IRepository` into the `Home` page.

```
@page "/"
@inject IRepository PizzaStore
```

The `@inject` directive essentially defines a new property on the component where the first token specifies the property type and the second token specifies the property name. The property is populated for you using dependency injection.

Override the `OnInitializedAsync` method in the `@code` block to retrieve the list of pizza specials. This method is part of the component lifecycle and is called when the component is initialized. Use the `GetSpecials()` method to acquire the list of pizza specials from the database:

```csharp
@code {
    List<PizzaSpecial>? specials;

	protected override async Task OnInitializedAsync()
	{
		specials = await PizzaStore.GetSpecials();
	}

}
```

Once the component is initialized it will render its markup. Replace the markup in the `Home` component with the following to list the pizza specials:

```html
<div class="main">
    <ul class="pizza-cards">
        @if (specials is not null)
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

Run the app by hitting `Ctrl-F5`. Now you should see a list of the specials that are available.

![image](https://user-images.githubusercontent.com/1874516/77239386-6c558880-6b97-11ea-9a14-83933146ba68.png)


## Create the layout

Next we'll set up the layout for the app. 

Layouts in Blazor are also components. They inherit from `LayoutComponentBase`, which defines a `Body` property that can be used to specify where the body of the layout should be rendered. The layout component for our pizza store app is defined in *BlazingPizza/Components/Layout/MainLayout.razor*.

```html
@inherits LayoutComponentBase

<div class="content">
    @Body
</div>
```

To see how the layout is associated with your pages, look at the contents of `Routes.razor`. Notice that the `DefaultLayout` parameter determines the layout used for any page that doesn't specify its own layout directly.

You can also override this `DefaultLayout` on a per-page basis. To do so, you can add a directive such as `@layout SomeOtherLayout` at the top of any `.razor` page component. However, you will not need to do so in this application.

Update the `MainLayout` component to define a top bar with a branding logo and a nav link for the home page:

```html
@inherits LayoutComponentBase

<div class="top-bar">
    <a class="logo" href="">
        <img src="img/logo.svg" />
    </a>

    <NavLink href="" class="nav-tab" Match="NavLinkMatch.All">
        <img src="img/pizza-slice.svg" />
        <div>Get Pizza</div>
    </NavLink>
</div>

<div class="content">
    @Body
</div>
```

The `NavLink` component is provided by Blazor. Components can be used from components by specifying an element with the component's type name along with attributes for any component parameters.

The `NavLink` component is the same as an anchor tag, except that it adds an `active` class if the current URL matches the link address. `NavLinkMatch.All` means that the link should be active only when it matches the entire current URL (not just a prefix). We'll examine the `NavLink` component in more detail in a later session.

Run the app by hitting `Ctrl-F5`. With our new layout, our pizza store app now looks like this:

![image](https://user-images.githubusercontent.com/1874516/77239419-aa52ac80-6b97-11ea-84ae-f880db776f5c.png)

This is a static server-side rendered web page that shows the menu nicely for our prospective customers.  Let's start to engage them and make this site more than a brochure promoting our tasty pizza specials. 

## Event handling

When the user clicks a pizza special, a pizza customization dialog should pop up to allow the user to customize their pizza and add it to their order. To handle DOM UI events in a Blazor app, you specify which event you want to handle using the corresponding HTML attribute and then specify the C# delegate you want called. The delegate may optionally take an event specific argument, but it's not required.

In *BlazingPizza/Components/Pages/Home.razor* add the following `@onclick` handler to the list item for each pizza special:

```html
@foreach (var special in specials)
{
    <li @onclick="@(() => Console.WriteLine(special.Name))" style="background-image: url('@special.ImageUrl')">
        <div class="pizza-info">
            <span class="title">@special.Name</span>
            @special.Description
            <span class="price">@special.GetFormattedBasePrice()</span>
        </div>
    </li>
}
```

The `@` symbol is used in Razor files to indicate the start of C# code. Surround the C# code with parens if needed to clarify where the C# code begins and ends.

Run the app and check that the pizza name is written to the browser console whenever a pizza is clicked. 

... but, its not.  The sample code doesn't work.

We're introducing interactivity to this page.  We're making it do more work beyond the rendering of HTML content and delivering it into the browser.  Interactivity is a key feature of Blazor that allows you as a developer to provide event handlers and interactions with on-screen components beyond a simple update from the server.  We're handling a click interaction, so we will want to make this page interactive.

### Making the home page interactive

We can make this page interactive by adding a `@rendermode` directive to the top of the file that indicates that it should be interactive on the server and it will work.  Add this line to the top of the `Home.razor` file:

```csharp
@rendermode InteractiveServer
```

This will direct the server to run this page in server interactive mode.  The server will maintain the state of this page in-memory for all users, handle interactions, and push updates using SignalR to the browser.  Our `Console.WriteLine` code _will_ work but it gets written to the server's console.

It makes more sense to deliver the first page as WebAssembly.  In this way, we can pre-render the non-interactive parts and allow the interactive parts of the page to render and run in the browser after the download of WebAssembly components completes.

However, WebAssembly code runs in a separate assembly that's only delivered in the browser.  In our solution, that assembly is the output of the BlazingPizza.Client project.  Let's move the `Home.razor` file into that project by cutting and pasting the file into the root of the BlazingPizza.Client project. After the move, we will need to update render mode for this page to enable it to be rendered using web assembly:

```csharp
@rendermode InteractiveWebAssembly
```

We'll need to make two more updates in order to enable our Home page to run as a web assembly file:

1. Create a new IRepository for the BlazingPizza.Client project

We need an IRepository that will run in the web assembly part of our application in order to load pizza data for the Home component.  Fortunately, there already is a set of APIs available on the server.  Let's just create an `HttpRepository` class that will use an `HttpClient` to fetch data from the server:

```csharp
using System.Net.Http.Json;

public class HttpRepository : IRepository
{

	private readonly HttpClient _httpClient;

	public HttpRepository(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}

	public Task<List<OrderWithStatus>> GetOrdersAsync()
	{
		throw new NotImplementedException();
	}

	public Task<OrderWithStatus> GetOrderWithStatus(int orderId)
	{
		throw new NotImplementedException();
	}

	public async Task<List<PizzaSpecial>> GetSpecials()
	{
		return await _httpClient.GetFromJsonAsync<List<PizzaSpecial>>("specials") ?? new();
	}

	public async Task<List<Topping>> GetToppings()
	{
		return await _httpClient.GetFromJsonAsync<List<Topping>>("toppings") ?? new();
	}

	public async Task PlaceOrder(Order order)
	{
		await _httpClient.PostAsJsonAsync("orders", order);
	}
}
```

We don't need all of those methods yet, so let's just leave them as is.

2. We need to register this new repository so that our Home component finds it

Let's register the new `HttpRepository` with the server locator that runs in web assembly by updating the contents of the `BlazingPizza.Client/Program.cs` file to include this registration:

```csharp
global using BlazingPizza.Shared;


builder.Services.AddScoped<HttpClient>(sp => 
	new HttpClient
	{
		BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
	});

builder.Services.AddScoped<IRepository, HttpRepository>();
```

Now we can run our project and see that the website will respond to the click interaction on the cards for the pizza specials:

![@onclick-event](https://user-images.githubusercontent.com/1874516/77239615-f56dbf00-6b99-11ea-8535-ddcc8bc0d8ae.png)

## Tracking the pizza being customized

Update the `@code` block in *Home.razor* to add some additional fields for tracking the pizza being customized and whether the pizza customization dialog is visible.

```csharp
List<PizzaSpecial>? specials;
Pizza? configuringPizza;
bool showingConfigureDialog;
```

Add a `ShowConfigurePizzaDialog` method to the `@code` block for handling when a pizza special is clicked.

```csharp
void ShowConfigurePizzaDialog(PizzaSpecial special)
{
    configuringPizza = new Pizza()
    {
        Special = special,
        SpecialId = special.Id,
        Size = Pizza.DefaultSize,
        Toppings = new List<PizzaTopping>(),
    };

    showingConfigureDialog = true;
}
```

Update the `@onclick` handler to call the `ShowConfigurePizzaDialog` method instead of `Console.WriteLine`.

```html
<li @onclick="@(() => ShowConfigurePizzaDialog(special))" style="background-image: url('@special.ImageUrl')">
```

## Implement the pizza customization dialog

Now we need to implement the pizza customization dialog so we can display it when the user selects a pizza. The pizza customization dialog will be a new component that lets you specify the size of your pizza and what toppings you want, shows the price, and lets you add the pizza to your order.

Add a *ConfigurePizzaDialog.razor* file under the *BlazingPizza.Client* directory. Since this component is not a separate page, it does not need the `@page` directive.

> Note: In Visual Studio, you can right-click the *BlazingPizza.Client* project in Solution Explorer, then choose *Add* -> *New Item* to use the *Razor Component* item template to add a new Razor component.

The `ConfigurePizzaDialog` should have a `Pizza` parameter that specifies the pizza being configured. Component parameters are defined by adding a writable property to the component decorated with the `[Parameter]` attribute. Because the `Pizza` parameter requires a value for the component to function, the `[EditorRequired]` attribute is also added. By adding the `[EditorRequired]` attribute, if a parameter value isn't provided, editors or build tools may display warnings to the user. 

Add a `@code` block to the `ConfigurePizzaDialog` with the following `Pizza` parameter:

```csharp
@code {
    [Parameter, EditorRequired] public Pizza Pizza { get; set; } = new();
}
```

> Note: Component parameter values need to have a setter and be declared `public` because they get set by the framework. However, they should *only* be set by the framework as part of the rendering process. Don't write code that overwrites these parameter values from outside the component, because then your component's state will be out of sync with its render output.

Add the following basic markup for the `ConfigurePizzaDialog`:

```html
    <div class="dialog-container">
        <div class="dialog">
            <div class="dialog-title">
                <h2>@Pizza.Special?.Name</h2>
                @Pizza.Special?.Description
            </div>
            <form class="dialog-body"></form>
            <div class="dialog-buttons">
                <button class="btn btn-secondary mr-auto">Cancel</button>
                <span class="mr-center">
                    Price: <span class="price">@(Pizza.GetFormattedTotalPrice())</span>
                </span>
                <button class="btn btn-success ml-auto">Order</button>
            </div>
        </div>
    </div>
```

Update *BlazingPizza.Client/Home.razor* to show the `ConfigurePizzaDialog` when a pizza special has been selected. The `ConfigurePizzaDialog` is styled to overlay the current page, so it doesn't really matter where you put this code block.

```html
@if (showingConfigureDialog)
{
    <ConfigurePizzaDialog Pizza="configuringPizza" />
}
```

Run the app and select a pizza special to see the skeleton of the `ConfigurePizzaDialog`.

![initial-pizza-dialog](https://user-images.githubusercontent.com/1874516/77239685-e3d8e700-6b9a-11ea-8adf-5ee8a69f08ae.png)


Unfortunately at this point there's no functionality in place to close the dialog. We'll add that shortly. Let's get to work on the dialog itself.

## Data binding

The user should be able to specify the size of their pizza. Add markup to the body of `ConfigurePizzaDialog` for a slider that lets the user specify the pizza size. This should replace the existing `<form class="dialog-body"></form>` element.

```html
<form class="dialog-body">
    <div>
        <label>Size:</label>
        <input type="range" min="@Pizza.MinimumSize" max="@Pizza.MaximumSize" step="1" />
        <span class="size-label">
            @(Pizza.Size)" (£@(Pizza.GetFormattedTotalPrice()))
        </span>
    </div>
</form>
```

Now the dialog shows a slider that can be used to change the pizza size. However it doesn't do anything right now if you use it.

![Slider](https://user-images.githubusercontent.com/1430011/57576985-eff40400-7421-11e9-9a1b-b22d96c06bcb.png)

We want the value of `Pizza.Size` to reflect the value of the slider. When the dialog opens, the slider gets its value from `Pizza.Size`. Moving the slider should update the pizza size stored in `Pizza.Size` accordingly. This concept is called two-way binding.

If you wanted to implement two-way binding manually, you could do so by combining value and @onchange, as in the following code (which you don't actually need to put in your application, because there's an easier solution):

```html
<input 
    type="range" 
    min="@Pizza.MinimumSize" 
    max="@Pizza.MaximumSize" 
    step="1" 
    value="@Pizza.Size"
    @onchange="@((ChangeEventArgs e) => Pizza.Size = int.Parse((string?) e.Value))" />
```

In Blazor you can use the `@bind` directive attribute to specify a two-way binding with this same behavior. The equivalent markup using `@bind` looks like this:

```html
<input type="range" min="@Pizza.MinimumSize" max="@Pizza.MaximumSize" step="1" @bind="Pizza.Size"  />
```

But if we use `@bind` with no further changes, the behavior isn't exactly what we want. Give it a try and see how it behaves. The update event only fires after the slider is released.

![Slider with default bind](https://user-images.githubusercontent.com/1874516/51804870-acec9700-225d-11e9-8e89-7761c9008909.gif)

We'd prefer to see updates as the slider is moved. Data binding in Blazor allows for this by letting you specify which event triggers a change using the syntax `@bind:<eventname>`. So, to bind using the `oninput` event instead do this:

```html
<input type="range" min="@Pizza.MinimumSize" max="@Pizza.MaximumSize" step="1" @bind="Pizza.Size" @bind:event="oninput" />
```

The pizza size should now update as you move the slider.

![Slider bound to oninput](https://user-images.githubusercontent.com/1874516/51804899-28e6df00-225e-11e9-9148-caf2dd269ce0.gif)

## Add additional toppings

The user should also be able to select additional toppings on `ConfigurePizzaDialog`. Add a list for storing the available toppings. Initialize the list of available toppings by making an HTTP GET request to the `/toppings` minimal API, defined at `PizzaApiExtensions.cs` in the **BlazingPizza** project.

```csharp
@inject IRepository Repository

<div class="dialog-container">
...
</div>

@code {
    // toppings is only null while loading
    List<Topping> toppings = null!; 

    [Parameter, EditorRequired] public Pizza Pizza { get; set; } = default!; 

    protected async override Task OnInitializedAsync()
    {
        toppings = await Repository.GetToppings();
    }
}
```

Add the following markup in the dialog body for displaying a drop down list with the list of available toppings followed by the set of selected toppings. Put this inside the `<form class="dialog-body">`, below the existing `<div>`."

```html
<div>
    <label>Extra Toppings:</label>
    @if (toppings is null)
    {
        <select class="custom-select" disabled>
            <option>(loading...)</option>
        </select>
    }
    else if (Pizza.Toppings.Count >= 6)
    {
        <div>(maximum reached)</div>
    }
    else
    {
        <select class="custom-select" @onchange="ToppingSelected">
            <option value="-1" disabled selected>(select)</option>
            @for (var i = 0; i < toppings.Count; i++)
            {
                <option value="@i">@toppings[i].Name - (£@(toppings[i].GetFormattedPrice()))</option>
            }
        </select>
    }
</div>

<div class="toppings">
    @foreach (var topping in Pizza.Toppings)
    {
        if (topping?.Topping is not null)
        {
            <div class="topping">
                @topping.Topping.Name
                <span class="topping-price">@topping.Topping.GetFormattedPrice()</span>
                <button type="button" class="delete-topping" @onclick="@(() => RemoveTopping(topping.Topping))">x</button>
            </div>
        }
    }
</div>
```

Also add the following event handlers for topping selection and removal:

```csharp
void ToppingSelected(ChangeEventArgs e)
{
    if (int.TryParse((string?)e.Value, out var index) && index >= 0)
    {
        AddTopping(toppings[index]);
    }
}

void AddTopping(Topping topping)
{
    if (toppings is null) return;
    if (Pizza.Toppings.Find(pt => pt.Topping == topping) is null)
    {
        Pizza.Toppings.Add(new PizzaTopping() { Topping = topping });
    }
}

void RemoveTopping(Topping topping)
{
    Pizza.Toppings.RemoveAll(pt => pt.Topping == topping);
}
```

You should now be able to add and remove toppings.

![Add and remove toppings](https://user-images.githubusercontent.com/1874516/77239789-c0626c00-6b9b-11ea-9030-0bcccdee6da7.png)


## Component events

The Cancel and Order buttons don't do anything yet. We need some way to communicate to the `Home` component when the user adds the pizza to their order or cancels. We can do that by defining component events. Component events are callback parameters that parent components can subscribe to.

Add two parameters to the `ConfigurePizzaDialog` component: `OnCancel` and `OnConfirm`. Both parameters should be of type `EventCallback`.

```csharp
[Parameter, EditorRequired] public EventCallback OnCancel { get; set; }
[Parameter, EditorRequired] public EventCallback OnConfirm { get; set; }
```

Add `@onclick` event handlers to the `ConfigurePizzaDialog` that trigger the `OnCancel` and `OnConfirm` events.

```html
<div class="dialog-buttons">
    <button class="btn btn-secondary mr-auto" @onclick="OnCancel">Cancel</button>
    <span class="mr-center">
        Price: <span class="price">@(Pizza.GetFormattedTotalPrice())</span>
    </span>
    <button class="btn btn-success ml-auto" @onclick="OnConfirm">Order ></button>
</div>
```

In the `Home` component add an event handler for the `OnCancel` event that hides the dialog and wires it up to the `ConfigurePizzaDialog`.

```html
<ConfigurePizzaDialog Pizza="configuringPizza" OnCancel="CancelConfigurePizzaDialog" />
```

```csharp
void CancelConfigurePizzaDialog()
{
    configuringPizza = null;
    showingConfigureDialog = false;
}
```

Now when you click the dialog's Cancel button, `Home.CancelConfigurePizzaDialog` will execute, and then the `Home` component will rerender itself. Since `showingConfigureDialog` is now `false` the dialog will not be displayed.

Normally what happens when you trigger an event (like clicking the Cancel button) is that the component that defined the event handler delegate will rerender. You could define events using any delegate type like `Action` or `Func<string, Task>`. Sometimes you want to use an event handler delegate that doesn't belong to a component - if you used a normal delegate type to define the event then nothing will be rendered or updated.

`EventCallback` is a special type that is known to the compiler that resolves some of these issues. It tells the compiler to dispatch the event to the component that contains the event handler logic. `EventCallback` has a few more tricks up its sleeve, but for now just remember that using `EventCallback` makes your component smart about dispatching events to the right place.

Run the app and verify that the dialog now disappears when the Cancel button is clicked.

When the `OnConfirm` event is fired, the customized pizza should be added to the user's order. Add an `Order` field to the `Home` component to track the user's order.

```csharp
List<PizzaSpecial>? specials;
Pizza? configuringPizza;
bool showingConfigureDialog;
Order order = new Order();
```

In the `Home` component add an event handler for the `OnConfirm` event that adds the configured pizza to the order and wire it up to the `ConfigurePizzaDialog`.

```html
<ConfigurePizzaDialog 
    Pizza="configuringPizza" 
    OnCancel="CancelConfigurePizzaDialog"  
    OnConfirm="ConfirmConfigurePizzaDialog" />
```

```csharp
void ConfirmConfigurePizzaDialog()
{
    if (configuringPizza is not null)
    {
        order.Pizzas.Add(configuringPizza);
        configuringPizza = null;
    }
    
    showingConfigureDialog = false;
}
```

Run the app and verify the dialog now disappears when the Order button is clicked. We can't see yet that a pizza was added to the order because there's no UI that shows this information. We'll address that next.

## Display the current order

Next we need to display the configured pizzas in the current order, calculate the total price, and provide a way to place the order.

Create a new `ConfiguredPizzaItem` component for displaying a configured pizza. It takes two parameters: the configured pizza, and an event for when the pizza was removed.

```html
<div class="cart-item">
    <a @onclick="OnRemoved" class="delete-item">x</a>
    <div class="title">@(Pizza.Size)" @Pizza.Special?.Name</div>
    <ul>
        @foreach (var topping in Pizza.Toppings)
        {
        <li>+ @topping.Topping?.Name</li>
        }
    </ul>
    <div class="item-price">
        @Pizza.GetFormattedTotalPrice()
    </div>
</div>

@code {
    [Parameter, EditorRequired] public Pizza Pizza { get; set; } = new(); 
    [Parameter, EditorRequired] public EventCallback OnRemoved { get; set; }
}
```

Add the following markup to the `Home` component just below the `main` div to add a right side pane for displaying the configured pizzas in the current order.

```html
<div class="sidebar">
    @if (order.Pizzas.Any())
    {
        <div class="order-contents">
            <h2>Your order</h2>

            @foreach (var configuredPizza in order.Pizzas)
            {
                <ConfiguredPizzaItem Pizza="configuredPizza" OnRemoved="@(() => RemoveConfiguredPizza(configuredPizza))" />
            }
        </div>
    }
    else
    {
        <div class="empty-cart">Choose a pizza<br>to get started</div>
    }

    <div class="order-total @(order.Pizzas.Any() ? "" : "hidden")">
        Total:
        <span class="total-price">@order.GetFormattedTotalPrice()</span>
        <button class="btn btn-warning" disabled="@(order.Pizzas.Count == 0)" @onclick="PlaceOrder">
            Order >
        </button>
    </div>
</div>
```

Also add the following event handlers to the `Home` component for removing a configured pizza from the order and submitting the order.

```csharp
void RemoveConfiguredPizza(Pizza pizza)
{
    order.Pizzas.Remove(pizza);
}

async Task PlaceOrder()
{
    await PizzaStore.PlaceOrder(order);
    order = new Order();
}
```

We are calling method PlaceOrder in HttpRepository.cs which is part of the BlazingPizza.Client project. However, at this point, we will also need to add signature of the PlaceOrder method to the interface IRepository in BlazingPizza.Shared project and all of its implementations. If we don't do that, our solution will fail to build. 

In the IRepository.cs in BlazingPizza.Shared project, add the following line before closing parenthesis of the IRepository interface:

```csharp
Task PlaceOrder(Order order);
```

In the EfRepository.cs in BlazingPizza project, add empty implementation of the interface before closing parenthesis of the EfRepository class:

```csharp
public Task PlaceOrder(Order order)
{
    throw new NotImplementedException();
}
```

You should now be able to add and remove configured pizzas from the order and submit the order.

![Order list pane](https://user-images.githubusercontent.com/1874516/77239878-b55c0b80-6b9c-11ea-905f-0b2558ede63d.png)


Even though the order was successfully added to the database, there's nothing in the UI yet that indicates this happened. That's what we'll address in the next session.

---

Next up - [Managing State](02-managing-state.md)
