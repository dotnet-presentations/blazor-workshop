// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Data;

public sealed class OrderDataService(
    PizzaStoreContext db,
    AuthenticationStateProvider auth)
{
    public async Task<OrderWithStatus[]> GetOrdersAsync()
    {
        var state = await auth.GetAuthenticationStateAsync();

        var userId = state.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var orders = await db.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.DeliveryLocation)
            .Include(o => o.Pizzas).ThenInclude(p => p.Special)
            .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
            .OrderByDescending(o => o.CreatedTime)
            .ToListAsync();

        return [..orders.Select(OrderWithStatus.FromOrder)];
    }
}
