using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlazingPizza.Server
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly PizzaStoreContext _db;

        public OrdersController(PizzaStoreContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<Order>>> GetOrders()
        {
            return await _db.Orders.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> PlaceOrder(Order order)
        {
            _db.Orders.Attach(order);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
