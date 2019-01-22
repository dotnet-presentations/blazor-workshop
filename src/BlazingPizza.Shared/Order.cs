using System;
using System.Collections.Generic;

namespace BlazingPizza
{
    public class Order
    {
        public int OrderId { get; set; }

        public DateTime CreatedTime { get; set; }


        public List<Pizza> Pizzas { get; set; }
    }
}
