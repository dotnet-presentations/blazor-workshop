namespace BlazingPizza.Shared;

public interface IRepository
{

	Task<List<Topping>> GetToppings();

	Task<List<PizzaSpecial>> GetSpecials();

	Task<List<OrderWithStatus>> GetOrdersAsync();

	Task<List<OrderWithStatus>> GetOrdersAsync(string userId);

	Task<OrderWithStatus> GetOrderWithStatus(int orderId);

	Task<OrderWithStatus> GetOrderWithStatus(int orderId, string userId);

	Task<int> PlaceOrder(Order order);


}
