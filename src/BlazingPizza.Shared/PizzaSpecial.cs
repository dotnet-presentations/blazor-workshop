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

        public string Description { get; set; }

        public List<PizzaSpecialTopping> Toppings { get; set; }
    }
}
