using System;
using System.Collections.Generic;

namespace BlazingPizza
{
    /// <summary>
    /// Represents a pre-configured template for a pizza a user can order
    /// </summary>
    public class PizzaSpecial
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal BasePrice { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public List<PizzaSpecialTopping> Toppings { get; set; }

        public decimal GetPriceForSize(PizzaSize size)
        {
            switch (size)
            {
                case PizzaSize.Small:
                    return BasePrice * .75m;

                case PizzaSize.Medium:
                    return BasePrice * 1.00m;

                case PizzaSize.Large:
                    return BasePrice * 1.25m;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
