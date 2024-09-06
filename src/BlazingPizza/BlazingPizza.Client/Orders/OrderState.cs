// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Client.Orders;

internal sealed class OrderState(LocalStorageJSInterop js)
{
    public bool ShowingConfigureDialog { get; private set; }

    public Pizza? ConfiguringPizza { get; private set; }

    public Order Order { get; private set; } = new();

    public void ShowConfigurePizzaDialog(PizzaSpecial special)
    {
        ConfiguringPizza = new Pizza()
        {
            Special = special,
            SpecialId = special.Id,
            Size = Pizza.DefaultSize,
            Toppings = [],
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
        if (ConfiguringPizza is not null)
        {
            Order.Pizzas.Add(ConfiguringPizza);
            ConfiguringPizza = null;
        }

        ShowingConfigureDialog = false;
    }

    public void RemoveConfiguredPizza(Pizza pizza) => Order.Pizzas.Remove(pizza);

    public async ValueTask InitializeOrderAsync()
    {
        var address = await js.GetLocalStorageItemAsync<Address>(
            "address");

        if (address is not null)
        {
            Order.DeliveryAddress = address;
        }
    }

    public async ValueTask ResetOrderAsync()
    {
        if (Order.DeliveryAddress is not null)
        {
            await js.SetLocalStorageItemAsync(
                "address", Order.DeliveryAddress);
        }

        Order = new();

        await InitializeOrderAsync();
    }
}
