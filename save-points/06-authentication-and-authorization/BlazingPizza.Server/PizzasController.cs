using Microsoft.AspNetCore.Mvc;

namespace BlazingPizza.Server
{
    [Route("pizzas")]
    [ApiController]
    public class PizzasController : Controller
    {
        private readonly PizzaStoreContext _db;

        public PizzasController(PizzaStoreContext db)
        {
            _db = db;
        }
    }
}
