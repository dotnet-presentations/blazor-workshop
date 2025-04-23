namespace BlazingPizza.Shared;

public interface IRepository
{

	Task<List<Topping>> GetToppings();

	Task<List<PizzaSpecial>> GetSpecials();

	Task<List<OrderWithStatus>> GetOrdersAsync();

	Task<OrderWithStatus> GetOrderWithStatus(int orderId);

	Task<int> PlaceOrder(Order order);


}
