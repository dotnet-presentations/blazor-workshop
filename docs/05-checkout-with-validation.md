# Checkout with Validation

If you take a look at the `Order` class in `BlazingPizza.Shared`, you might notice that it holds a `DeliveryAddress` property of type `Address`. However, nothing in the pizza ordering flow yet populates this data, so all your orders just have a blank delivery address. 

It's time to fix this by adding a "checkout" screen that requires customers to enter a valid address.

## Inserting checkout into the flow

Start by adding a new page component, `Checkout.razor`, with a `@page` directive matching the URL `/checkout`. For the initial markup, let's display the details of the order using your `OrderReview` component:

```razor
<div class="main">
    <div class="checkout-cols">
        <div class="checkout-order-details">
            <h4>Review order</h4>
            <OrderReview Order="OrderState.Order" />
        </div>
    </div>

    <button class="checkout-button btn btn-warning" @onclick="PlaceOrder">
        Place order
    </button>
</div>
```

To implement `PlaceOrder`, copy the method with that name from `Index.razor` into `Checkout.razor`:

```razor
@code {
    async Task PlaceOrder()
    {
        var response = await HttpClient.PostAsJsonAsync("orders", OrderState.Order);
        var newOrderId = await response.Content.ReadFromJsonAsync<int>();
        OrderState.ResetOrder();
        NavigationManager.NavigateTo($"myorders/{newOrderId}");
    }
}
```

As usual, you'll need to `@inject` values for `OrderState`, `HttpClient`, and `NavigationManager` so that it can compile, just like you did in `Index.razor`.

Next, let's bring customers here when they try to submit orders. Back in `Index.razor`, make sure you've deleted the `PlaceOrder` method, and then change the order submission button into a regular HTML link to the `/checkout` URL, i.e.:

```razor
<a href="checkout" class="@(OrderState.Order.Pizzas.Count == 0 ? "btn btn-warning disabled" : "btn btn-warning")">
    Order >
</a>
```

> Note that we removed the `disabled` attribute, since HTML links do not support it, and added appropriate styling instead.

Now, when you run the app, you should be able to reach the checkout page by clicking the *Order* button, and from there you can click *Place order* to confirm it.

![Confirm order](https://user-images.githubusercontent.com/1874516/77242251-d2530780-6bb9-11ea-8535-1c41decf3fcc.png)

## Capturing the delivery address

We've now got a good place to put some UI for entering a delivery address. As usual, let's factor this out into a reusable component. You never know when you're going to be asking for addresses in other places.

Create a new component in the `BlazingPizza.Client` project's `Shared` folder called `AddressEditor.razor`. It's going to be a general way to edit `Address` instances, so have it receive a parameter of this type:

```razor
@code {
    [Parameter] public Address Address { get; set; }
}
```

The markup here is going to be a bit tedious, so you probably want to copy and paste this. We'll need input elements for each of the properties on an `Address`:

```razor
<div class="form-field">
    <label>Name:</label>
    <div>
        <input @bind="Address.Name" />
    </div>
</div>

<div class="form-field">
    <label>Line 1:</label>
    <div>
        <input @bind="Address.Line1" />
    </div>
</div>

<div class="form-field">
    <label>Line 2:</label>
    <div>
        <input @bind="Address.Line2" />
    </div>
</div>

<div class="form-field">
    <label>City:</label>
    <div>
        <input @bind="Address.City" />
    </div>
</div>

<div class="form-field">
    <label>Region:</label>
    <div>
        <input @bind="Address.Region" />
    </div>
</div>

<div class="form-field">
    <label>Postal code:</label>
    <div>
        <input @bind="Address.PostalCode" />
    </div>
</div>

@code {
    [Parameter] public Address Address { get; set; }
}
```

Finally, you can actually use your `AddressEditor` inside the `Checkout.razor` component:

```razor
<div class="checkout-cols">
    <div class="checkout-order-details">
        ... leave this div unchanged ...
    </div>

    <div class="checkout-delivery-address">
        <h4>Deliver to...</h4>
        <AddressEditor Address="OrderState.Order.DeliveryAddress" />
    </div>
</div>
```

Your checkout screen now asks for a delivery address:

![Address editor](https://user-images.githubusercontent.com/1874516/77242320-79d03a00-6bba-11ea-9e40-4bf747d4dcdc.png)

If you submit an order now, any address data that you entered will actually be saved in the database with the order, because it's all part of the `Order` object that gets serialized and sent to the server.

If you're really keen to verify the data gets saved, consider downloading a tool such as [DB Browser for SQLite](https://sqlitebrowser.org/) to inspect the contents of your `pizza.db` file. But you don't strictly need to do this.

Alternatively, set a breakpoint inside `BlazingPizza.Server`'s `OrderController.PlaceOrder` method, and use the debugger to inspect the incoming `Order` object. Here you should be able to see the backend server receive the address data you typed in.

## Adding server-side validation

As yet, customers can still leave the "delivery address" fields blank and merrily order a pizza to be delivered nowhere in particular. When it comes to validation, it's normal to implement rules both on the server and on the client:

 * Client-side validation is a courtesy to your users. It can provide instant feedback while they are editing a form. However, it can easily be bypassed by anyone with a basic knowledge of the browser dev tools.
 * Server-side validation is where the real enforcement is.

As such it's usually best to start by implementing server-side validation, so you know your app is robust no matter what happens client-side. If you go and look at `OrdersController.cs` in the `BlazingPizza.Server` project, you'll see that this API endpoint is decorated with the `[ApiController]` attribute:

```csharp
[Route("orders")]
[ApiController]
public class OrdersController : Controller
{
    // ...
}
```

`[ApiController]` adds various server-side conventions, including enforcement of `DataAnnotations` validation rules. So all we need to do is put some `DataAnnotations` validation rules onto the model classes.

Open `Address.cs` from the `BlazingPizza.Shared` project, and put a `[Required]` attribute onto each of the properties except for `Id` (which is autogenerated, because it's the primary key) and `Line2`, since not all addresses need a second line. You can also place some `[MaxLength]` attributes if you wish, or any other `DataAnnotations` rules:


```csharp
using System.ComponentModel.DataAnnotations;

namespace BlazingPizza
{
    public class Address
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; }

        [Required, MaxLength(100)]
        public string Line1 { get; set; }

        [MaxLength(100)]
        public string Line2 { get; set; }

        [Required, MaxLength(50)]
        public string City { get; set; }

        [Required, MaxLength(20)]
        public string Region { get; set; }

        [Required, MaxLength(20)]
        public string PostalCode { get; set; }
    }
}
```

Now, after you recompile and run your application, you should be able to observe the validation rules being enforced on the server. If you try to submit an order with a blank delivery address, then the server will reject the request and you'll see an HTTP 400 ("Bad Request") error in the browser's *Network* tab:

![Server validation](https://user-images.githubusercontent.com/1874516/77242384-067af800-6bbb-11ea-8dd0-74f457d15afd.png)

... whereas if you fill out the address fields fully, the server will allow you to place the order. Check that both of these cases behave as expected.

## Adding client-side validation

Blazor has a comprehensive system for data entry forms and validation. We'll now use this to apply the same `DataAnnotations` rules on the client that are already being enforced on the server.

The way Blazor's forms and validation system works is based around something called an `EditContext`. An `EditContext` tracks the state of an editing process, so it knows which fields have been modified, what data has been entered, and whether or not the fields are valid. Various built-in UI components hook into the `EditContext` both to read its state (e.g., display validation messages) and to write to its state (e.g., to populate it with the data entered by the user).

### Using EditForm

One of the most important built-in UI components for data entry is the `EditForm`. This renders as an HTML `<form>` tag, but also sets up an `EditContext` to track what's going on inside the form. To use this, go to your `Checkout.razor` component, and wrap an `EditForm` around the whole of the contents of the `main` div:

```razor
<div class="main">
    <EditForm Model="OrderState.Order.DeliveryAddress">
        <div class="checkout-cols">
            ... leave unchanged ...
        </div>

        <button class="checkout-button btn btn-warning" @onclick="PlaceOrder">
            Place order
        </button>
    </EditForm>
</div>
```

You can have multiple `EditForm` components at once, but they can't overlap (because HTML's `<form>` elements can't overlap). By specifying a `Model`, we're telling the internal `EditContext` which object it should validate when the form is submitted (in this case, the delivery address).

Let's start by displaying validation messages in a very basic (and not very attractive) way. Inside the `EditForm`, right at the bottom, add the following two components:

```razor
<DataAnnotationsValidator />
<ValidationSummary />
```

The `DataAnnotationsValidator` hooks into events on the `EditContext` and executes `DataAnnotations` rules. If you wanted to use a different validation system other than `DataAnnotations`, you'd swap `DataAnnotationsValidator` for something else.

The `ValidationSummary` simply renders an HTML `<ul>` containing any validation messages from the `EditContext`.

### Handling submission

If you ran your application now, you could still submit a blank form (and the server would still respond with an HTTP 400 error). That's because your `<button>` isn't actually a `submit` button. Modify the `button` by adding `type="submit"` and **removing** its `@onclick` attribute entirely.

Next, instead of triggering `PlaceOrder` directly from the button, you need to trigger it from the `EditForm`. Add the following `OnValidSubmit` attribute onto the `EditForm`:

```razor
<EditForm Model="OrderState.Order.DeliveryAddress" OnValidSubmit="PlaceOrder">
```

As you can probably guess, the `<button>` no longer triggers `PlaceOrder` directly. Instead, the button just asks the form to be submitted. And then the form decides whether or not it's valid, and if it is, *then* it will call `PlaceOrder`.

Try it out: you should no longer be able to submit an invalid form, and you'll see validation messages (albeit unattractive ones) where you placed the `ValidationSummary`.

![Validation summary](https://user-images.githubusercontent.com/1874516/77242430-9d47b480-6bbb-11ea-96ef-8865468375fb.png)

### Using ValidationMessage

Obviously it's pretty disgusting to display all the validation messages so far away from the text boxes. Let's move them to better places.

Start by removing the `<ValidationSummary>` component entirely. Then, switch over to `AddressEditor.razor`, and add separate `<ValidationMessage>` components next to each of the form fields. For example,

```razor
<div class="form-field">
    <label>Name:</label>
    <div>
        <input @bind="Address.Name" />
        <ValidationMessage For="@(() => Address.Name)" />
    </div>
</div>
```

Do the equivalent for all of the form fields.

In case you're wondering, the syntax `@(() => Address.Name)` is a *lambda expression*, and we use this syntax as a way of describing which property to read the metadata from, without actually evaluating the property's value.

Now things look a lot better:

![Validation messages](https://user-images.githubusercontent.com/1874516/77242484-03ccd280-6bbc-11ea-8dd1-5d723b043ee2.png)

If you want, you can improve the readability of the messages by specifying custom ones. For example, instead of displaying *The City field is required*, you could go to `Address.cs` and do this:

```csharp
[Required(ErrorMessage = "How do you expect to receive the pizza if we don't even know what city you're in?"), MaxLength(50)]
public string City { get; set; }
```

### Better validation UX using the built-in input components

The user experience is still not great, because once the validation messages are displayed, they remain on the screen until you click *Place order* again, even if you have edited the field values. Try it out and see how it feels pretty basic!

To improve on this, you can replace the low-level HTML input elements with Blazor's built-in input components. They know how to hook more deeply into the `EditContext`:

* When they are edited, they notify the `EditContext` immediately so it can refresh validation status.
* They also receive notifications about validity from the `EditContext`, so they can highlight themselves as either valid or invalid as the user edits them.

Go back to `AddressEditor.razor` once again. Replace each of the `<input>` elements with a corresponding `<InputText>`. For example,

```html
<div class="form-field">
    <label>Name:</label>
    <div>
        <InputText @bind-Value="Address.Name" />
        <ValidationMessage For="@(() => Address.Name)" />
    </div>
</div>
```

Do this for all the properties. The behavior is now much better! As well as having the validation messages update individually for each form field as you change focus, you'll get a neat "valid" or "invalid" highlight around each one:

![Input components](https://user-images.githubusercontent.com/1874516/77242542-ba30b780-6bbc-11ea-8018-be022d6cac0b.png)

The green/red styling is achieved by applying CSS classes, so you can change the appearance of these effects or remove them entirely if you wish.

`InputText` isn't the only built-in input component, though it is the only one we need in this case. Others include `InputCheckbox`, `InputDate`, `InputSelect`, and more.

## Bonus challenge

If you're keen and have time, can you prevent accidental double-submission of the form?

Currently, if it takes a while for the form post to reach the server, the user could click submit multiple times and send multiple copies of their order. Try declaring a `bool isSubmitting` property that, when `true`, results in the *Place order* button being disabled. Remember to set it back to `false` when the submission is completed (successfully or not), otherwise the user might get stuck.

To check your solution works, you might want to slow down the server by adding the following line at the top of `PlaceOrder()` inside `OrdersController.cs`:

```cs
await Task.Delay(5000); // Wait 5 seconds
```

## Up next

Up next we'll add [authentication and authorization](https://github.com/dotnet-presentations/blazor-workshop/blob/master/docs/06-authentication-and-authorization.md)
