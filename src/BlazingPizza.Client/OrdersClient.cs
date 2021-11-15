namespace BlazingPizza.Client;

public class OrdersClient
{
    private readonly HttpClient _httpClient;

    public OrdersClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<IEnumerable<OrderWithStatus>> GetOrders() =>
        await _httpClient.GetFromJsonAsync<IEnumerable<OrderWithStatus>>("orders");


    public async Task<OrderWithStatus> GetOrder(int orderId) =>
        await _httpClient.GetFromJsonAsync<OrderWithStatus>($"orders/{orderId}");

    public async Task<int> PlaceOrder(Order order)
    {
        var response = await _httpClient.PostAsJsonAsync("orders", order);
        response.EnsureSuccessStatusCode();
        var orderId = await response.Content.ReadFromJsonAsync<int>();
        return orderId;
    }

    public async Task SubscribeToNotifications(NotificationSubscription subscription)
    {
        var response = await _httpClient.PutAsJsonAsync("notifications/subscribe", subscription);
        response.EnsureSuccessStatusCode();
    }
}
