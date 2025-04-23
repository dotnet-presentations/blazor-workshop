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
