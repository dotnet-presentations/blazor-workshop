using System.Net.Http.Json;
using System.Text.Json;

namespace BlazingPizza.Client;

public class OrdersClient
{
    private readonly HttpClient httpClient;

    public OrdersClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IEnumerable<OrderWithStatus>> GetOrders() =>
            await httpClient.GetFromJsonAsync("orders", OrderContext.Default.ListOrderWithStatus);


    public async Task<OrderWithStatus> GetOrder(int orderId) =>
            await httpClient.GetFromJsonAsync($"orders/{orderId}", OrderContext.Default.OrderWithStatus);


    public async Task<int> PlaceOrder(Order order)
    {
        var response = await httpClient.PostAsJsonAsync("orders", order, OrderContext.Default.Order);
        response.EnsureSuccessStatusCode();
        var orderId = await response.Content.ReadFromJsonAsync<int>();
        return orderId;
    }

    public async Task SubscribeToNotifications(NotificationSubscription subscription)
    {
        var response = await httpClient.PutAsJsonAsync("notifications/subscribe", subscription);
        response.EnsureSuccessStatusCode();
    }
}
