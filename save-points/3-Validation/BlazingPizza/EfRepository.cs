
using Microsoft.EntityFrameworkCore;

namespace BlazingPizza;

public class EfRepository : IRepository
{
	private readonly PizzaStoreContext _Context;

	public EfRepository(PizzaStoreContext context)
	{
		_Context = context;
	}

	public async Task<List<OrderWithStatus>> GetOrdersAsync()
	{
		var orders = await _Context.Orders
						.Include(o => o.DeliveryLocation)
						.Include(o => o.Pizzas).ThenInclude(p => p.Special)
						.Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
						.OrderByDescending(o => o.CreatedTime)
						.ToListAsync();

		return orders.Select(o => OrderWithStatus.FromOrder(o)).ToList();
	}

	public async Task<OrderWithStatus> GetOrderWithStatus(int orderId)
	{

		//await Task.Delay(5000);

		var order = await _Context.Orders
						.Where(o => o.OrderId == orderId)
						.Include(o => o.DeliveryLocation)
						.Include(o => o.Pizzas).ThenInclude(p => p.Special)
						.Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
						.SingleOrDefaultAsync();

		if (order is null) throw new ArgumentNullException(nameof(order));

		return OrderWithStatus.FromOrder(order);

	}

	public async Task<List<PizzaSpecial>> GetSpecials()
	{
		return await _Context.Specials.ToListAsync();
	}

	public async Task<List<Topping>> GetToppings()
	{
		return await _Context.Toppings.OrderBy(t => t.Name).ToListAsync();
	}

	public Task<int> PlaceOrder(Order order)
	{
		throw new NotImplementedException();
	}
}