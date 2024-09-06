// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Client.Orders;

public sealed class OrdersClient(HttpClient client)
{
    public async Task<IEnumerable<OrderWithStatus>> GetOrdersAsync() =>
        await client.GetFromJsonAsync(
            requestUri: "api/orders",
            jsonTypeInfo: OrderContext.Default.ListOrderWithStatus) ?? [];

    public async Task<OrderWithStatus> GetOrderAsync(int orderId) =>
        await client.GetFromJsonAsync(
            requestUri: $"api/orders/{orderId}",
            jsonTypeInfo: OrderContext.Default.OrderWithStatus) ?? new();

    public async Task<int> PlaceOrderAsync(Order order)
    {
        var response = await client.PostAsJsonAsync(
            requestUri: "api/orders",
            value: order,
            jsonTypeInfo: OrderContext.Default.Order);

        response.EnsureSuccessStatusCode();

        var orderId = await response.Content.ReadFromJsonAsync<int>();

        return orderId;
    }

    public async Task SubscribeToNotificationsAsync(NotificationSubscription subscription)
    {
        var response = await client.PutAsJsonAsync(
            requestUri: "api/notifications/subscribe",
            value: subscription);

        response.EnsureSuccessStatusCode();
    }
}
