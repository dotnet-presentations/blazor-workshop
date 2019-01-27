# Customize a pizza

In this session we'll update the pizza store app to enable users to customize their pizza orders.

## Event handling

When the user clicks a pizza special a pizza customization dialog should pop up to allow the user to customize their pizza and add it to their order. To handle DOM UI events in a Blazor app, you specify which event you want to handle using the corresponding HTML attribute and then specify the C# delegate you want called. The delegate may optionally take an event specific argument, but it is not required.

In *Pages/Index.cshtml* Add the following `onclick` handler to the list item for each pizza special:

```html
@foreach (var special in specials)
{
    <li style="background-image: url('@special.ImageUrl')">
    <li onclick="@(() => Console.WriteLine(special.Name))" style="background-image: url('@special.ImageUrl')">
        <div class="pizza-info">
            <span class="title">@special.Name</span>
            @special.Description
            <span class="price">@special.GetFormattedBasePrice()</span>
        </div>
    </li>
}
```

Run the app and check that the special name is written to the browser console whenever a pizza special is clicked. 

The `@` symbol is used in Razor files to indicate the start of C# code. Surround the C# code with parens if needed to clarify where the C# code begins and ends.

Update the `@functions` block in *Index.cshtml* to add some additional fields for tracking the pizza being customized and whether the pizza customization dialog is visible.

```csharp
List<PizzaSpecial> specials;
Pizza configuringPizza;
bool showingConfigureDialog;
```

Add a `ShowConfigurePizzaDialog` method to the `@functions` block for handling when a pizza special is clicked.

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

Update the `onclick` handler to call the `ShowConfigurePizzaDialog` method instead of `Console.WriteLine`.

```html
<li onclick="@(() => ShowConfigurePizzaDialog(special))" style="background-image: url('@special.ImageUrl')">
```

## Implement the pizza customization dialog

Now we need to implement the pizza customization dialog. The pizza customization dialog will be a new component that let's you specify the size of your pizza and what toppings you want, shows the price, and let's you add the pizza to your order.

Add a *ConfigurePizzaDialog.cshtml* file under the *Shared* directory. Since this component is not a separate page, it does not need the `@page` directive. 

The `ConfigurePizzaDialog` should have a `Pizza` parameter that specifies the pizza being configured. Component parameters are defined by adding a writable property to the component decorated with the `[Parameter]` attribute. Add a `@functions` block with the following `Pizza` parameter:

```csharp
@functions {
    [Parameter] Pizza Pizza { get; set; }
}
```
> Note: Component parameters values should only ever be set by the runtime, so they should *not* be public. This allows the runtime to keep track of when components need to be rendered.

Add the following basic markup for the `ConfigurePizzaDialog`:

```html
<div class="dialog-container">
    <div class="dialog">
        <div class="dialog-title">
            <h2>@Pizza.Special.Name</h2>
            @Pizza.Special.Description
        </div>
        <form class="dialog-body">

        </form>

        <div class="dialog-buttons">
            <button class="btn btn-secondary mr-auto">Cancel</button>
            <span class="mr-center">
                Price: <span class="price">@(Pizza.GetFormattedTotalPrice())</span>
            </span>
            <button class="btn btn-success ml-auto">Order ></button>
        </div>
    </div>
</div>
```

Update *Pages/Index.cshtml* to show the `ConfigurePizzaDialog` when a pizza special has been selected:

```html
@if (showingConfigureDialog)
{
    <ConfigurePizzaDialog Pizza="configuringPizza" />
}
```

Run the app and select a pizza special to see the `ConfigurePizzaDialog`.

![Initial configure pizza dialog]()

## Component events

The Cancel and Order buttons don't do anything yet. We need some way to communicate to the `Index` component when the user completes their order or cancels. We can do that by defining component events. Component events are action parameters that parent components can subscribe to.

Add two parameters to the `ConfigurePizzaDialog` component: `OnCancel` and `OnConfirm`. Both parameters should be of type `Action`.

```csharp
[Parameter] Action OnCancel { get; set; }
[Parameter] Action OnConfirm { get; set; }
```

Add `onclick` event handlers that trigger the `OnCancel` and `OnConfirm` events.

```html
<div class="dialog-buttons">
    <button class="btn btn-secondary mr-auto" onclick="@OnCancel">Cancel</button>
    <span class="mr-center">
        Price: <span class="price">@(Pizza.GetFormattedTotalPrice())</span>
    </span>
    <button class="btn btn-success ml-auto" onclick="@OnConfirm">Order ></button>
</div>
```

In the `Index` component add an event handler for the `OnCancel`event and wire it up to the `ConfigurePizzaDialog`.

```html
<ConfigurePizzaDialog Pizza="configuringPizza" OnCancel="CancelConfigurePizzaDialog" />
```

```csharp
void CancelConfigurePizzaDialog()
{
    configuringPizza = null;
    showingConfigureDialog = false;
    StateHasChanged();
}
```

The `StateHasChanged` method signals to the runtime that the component's state has changed and it needs to be rendered. Components are rendered automatically by the runtime when it's parameters change or when a UI event is fired on that component. In this case the event triggering the state change came from a different component, so `StateHasChanged` must be called manually.



Add a list property for storing the available toppings. Initialize the list of toppings by making an HTTP GET request to the `/toppings` API.

```csharp
@inject HttpClient HttpClient

@functions {
    List<Topping> toppings { get; set; }

    [Parameter] Pizza Pizza { get; set; }

    protected async override Task OnInitAsync()
    {
        toppings = await HttpClient.GetJsonAsync<List<Topping>>("/toppings");
    }
}
```



The completed `ConfigurePizzaDialog` should look like this:

```csharp
@inject HttpClient HttpClient

<div class="dialog-container">
    <div class="dialog">
        <div class="dialog-title">
            <h2>@Pizza.Special.Name</h2>
            @Pizza.Special.Description
        </div>
        <form class="dialog-body">
            <div>
                <label>Size:</label>
                <input type="range" min="@Pizza.MinimumSize" max="@Pizza.MaximumSize" step="1" bind-value-oninput="Pizza.Size" />
                <span class="size-label">
                    @(Pizza.Size)" (£@(Pizza.GetFormattedTotalPrice()))
                </span>
            </div>
            <div>
                <label>Extra Toppings:</label>
                @if (toppings == null)
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
                    <select class="custom-select" onchange="@ToppingSelected">
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
                    <div class="topping">
                        @topping.Topping.Name
                        <span class="topping-price">@topping.Topping.GetFormattedPrice()</span>
                        <button type="button" class="delete-topping" onclick="@(() => RemoveTopping(topping.Topping))">x</button>
                    </div>
                }
            </div>
        </form>

        <div class="dialog-buttons">
            <button class="btn btn-secondary mr-auto" onclick="@OnCancel">Cancel</button>
            <span class="mr-center">
                Price: <span class="price">@(Pizza.GetFormattedTotalPrice())</span>
            </span>
            <button class="btn btn-success ml-auto" onclick="@OnConfirm">Order ></button>
        </div>
    </div>
</div>

@functions {
    List<Topping> toppings { get; set; }

    protected async override Task OnInitAsync()
    {
        toppings = await HttpClient.GetJsonAsync<List<Topping>>("/toppings");
    }

    void ToppingSelected(UIChangeEventArgs e)
    {
        if (int.TryParse((string)e.Value, out var index) && index >= 0)
        {
            AddTopping(toppings[index]);
        }
    }

    void AddTopping(Topping topping)
    {
        if (Pizza.Toppings.Find(pt => pt.Topping == topping) == null)
        {
            Pizza.Toppings.Add(new PizzaTopping() { Topping = topping });
        }
    }

    void RemoveTopping(Topping topping)
    {
        Pizza.Toppings.RemoveAll(pt => pt.Topping == topping);
        StateHasChanged();
    }
}
```




- Make special pizza cards clickable
- Clicking on a special brings up the new customize dialog
- Index needs to handle the hide/show of the dialog 
- Index needs to pass in the Pizza object as well as two 'command' delegates
- Using `bind` and `onclick` on the customize dialog to update prices in real time
- explain the difference between `bind` and `bind-value-oninput` on the slider
- cancel button should close the dialog
- confirm button should close the dialog and add to order
- now add the markup for sidebar which will display orders
- add a ConfiguredPizzaItem component
- hook up the order button to do an HTTP POST and clear the order
- (no way to see existing orders yet)