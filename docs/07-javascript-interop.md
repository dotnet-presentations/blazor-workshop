# JavaScript interop

Users of the pizza store can now track the status of their orders in real time. In this session we'll use JavaScript interop to add a real-time map to the order status page that answers the age old question, "Where's my pizza?!?".

## The Map component

Included in the ComponentsLibrary project is a prebuilt `Map` component for displaying the location of a set of markers and animating their movements over time. We'll use this component to show the location of the user's pizza orders as they are being delivered, but first let's look at how the `Map` component is implemented.

Open *Map.razor* and take a look at the code:

```csharp
@using Microsoft.JSInterop
@inject IJSRuntime JSRuntime

<div id="@elementId" style="height: 100%; width: 100%;"></div>

@code {
    string elementId = $"map-{Guid.NewGuid().ToString("D")}";
    
    [Parameter] double Zoom { get; set; }
    [Parameter] List<Marker> Markers { get; set; }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        await JSRuntime.InvokeVoidAsync(
            "deliveryMap.showOrUpdate",
            elementId,
            Markers);
    }
}
```

The `Map` component uses dependency injection to get an `IJSRuntime` instance. This service can be used to make JavaScript calls to browser APIs or existing JavaScript libraries by calling the `InvokeVoidAsync` or `InvokeAsync<TResult>` method. The first parameter to this method specifies the path to the JavaScript function to call relative to the root `window` object. The remaining parameters are arguments to pass to the JavaScript function. The arguments are serialized to JSON so they can be handled in JavaScript.

The `Map` component first renders a `div` with a unique ID for the map and then calls the `deliveryMap.showOrUpdate` function to display the map in the specified element with the specified markers passed to the `Map` component. This is done in the `OnAfterRenderAsync` component lifecycle event to ensure that the component is done rendering its markup. The `deliveryMap.showOrUpdate` function is defined in the *wwwroot/deliveryMap.js* file, which then uses [leaflet.js](http://leafletjs.com) and [OpenStreetMap](https://www.openstreetmap.org/) to display the map. The details of how this code works isn't really important - the critical point is that it's possible to call any JavaScript function this way.

How do these files make their way to the Blazor app? For a Blazor library project (using `Sdk="Microsoft.NET.Sdk.Razor"`) any files in the `wwwroot/` folder will be bundled with the library. The server project will automatically serve these files using the static files middleware.

The final link is for the page hosting the Blazor client app to include the desired files (in our case `.js` and `.css`). The `index.html` includes these files using relative URIs like `_content/BlazingPizza.ComponentsLibrary/localStorage.js`. This is the general pattern for references files bundled with a Blazor class library - `_content/<library name>/<file path>`.

---

If you start typing in `Map`, you'll notice that the editor doesn't offer completion for it. This is because the binding between elements and components are governed by C#'s namespace binding rules. The `Map` component is defined in the `BlazingPizza.ComponentsLibrary.Map` namespace, which we don't have an `@using` for.

Add an `@using` for this namespace to the root `_Imports.razor` to bring this component into scope:
```razor
@using System.Net.Http
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.JSInterop
@using BlazingPizza.Client
@using BlazingPizza.Client.Shared
@using BlazingPizza.ComponentsLibrary
@using BlazingPizza.ComponentsLibrary.Map
```

Add the `Map` component to the `OrderDetails` page by adding the following just below the `track-order-details` `div`:

```html
<div class="track-order-map">
    <Map Zoom="13" Markers="orderWithStatus.MapMarkers" />
</div>
```

*The reason why we haven't needed to add `@using`s for our components before now is that our root `_Imports.razor` already contains an `@using BlazingPizza.Shared` which matches the reusable components we have written.*

When the `OrderDetails` component polls for order status updates, an update set of markers is returned with the latest location of the pizzas, which then gets reflected on the map.

![Real-time pizza map](https://user-images.githubusercontent.com/1874516/51807322-6018b880-227d-11e9-89e5-ef75f03466b9.gif)

## Add a confirm prompt for deleting pizzas

The JavaScript interop code for the `Map` component was provided for you. Next you'll add some JavaScript interop code of your own.

It would be a shame if users accidentally deleted pizzas from their order (and ended up not buying them!). Let's add a confirm prompt when the user tries to delete a pizza. We'll show the confirm prompt using JavaScript interop.

Add a static `JSRuntimeExtensions` class to the Client project with a `Confirm` extension method off of `IJSRuntime`. Implement the `Confirm` method to call the built-in JavaScript `confirm` function.

```csharp
public static class JSRuntimeExtensions
{
    public static ValueTask<bool> Confirm(this IJSRuntime jsRuntime, string message)
    {
        return jsRuntime.InvokeAsync<bool>("confirm", message);
    }
}
```

Inject the `IJSRuntime` service into the `Index` component so that it can be used there to make JavaScript interop calls.

```razor
@page "/"
@inject HttpClient HttpClient
@inject OrderState OrderState
@inject NavigationManager NavigationManager
@inject IJSRuntime JS
```

Add an async `RemovePizza` method to the `Index` component that calls the `Confirm` method to verify if the user really wants to remove the pizza from the order.

```csharp
async Task RemovePizza(Pizza configuredPizza)
{
    if (await JS.Confirm($"Remove {configuredPizza.Special.Name} pizza from the order?"))
    {
        OrderState.RemoveConfiguredPizza(configuredPizza);
    }
}
```

In the `Index` component update the event handler for the `ConfiguredPizzaItems` to call the new `RemovePizza` method. 

```csharp
@foreach (var configuredPizza in OrderState.Order.Pizzas)
{
    <ConfiguredPizzaItem Pizza="configuredPizza" OnRemoved="@(() => RemovePizza(configuredPizza))" />
}
```

Run the app and try removing a pizza from the order.

![Confirm pizza removal](https://user-images.githubusercontent.com/1874516/77243688-34b40400-6bca-11ea-9d1c-331fecc8e307.png)

Notice that we didn't have to update the signature of `ConfiguredPizzaItem.OnRemoved` to support async. This is another special property of `EventCallback`, it supports both synchronous event handlers and asynchronous event handlers.

Next up - [Templated components](08-templated-components.md)
