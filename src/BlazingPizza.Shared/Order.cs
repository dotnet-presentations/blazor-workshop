using System.Collections.Generic;

namespace BlazingPizza
{
    public class Order
    {
        public int OrderId { get; set; }

        public List<Pizza> Pizzas { get; set; }
    }
}
