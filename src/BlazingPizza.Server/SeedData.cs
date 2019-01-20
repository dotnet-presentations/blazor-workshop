using System.Collections.Generic;

namespace BlazingPizza.Server
{
    public static class SeedData
    {
        public static void Initialize(PizzaStoreContext db)
        {
            var toppings = new Topping[]
            {
                new Topping()
                {
                    Name = "Extra cheese",
                    Price = 2.50m,
                },
                new Topping()
                {
                    Name = "American bacon",
                    Price = 2.99m,
                },
                new Topping()
                {
                    Name = "British bacon",
                    Price = 2.99m,
                },
                new Topping()
                {
                    Name = "Canadian bacon",
                    Price = 2.99m,
                },
                new Topping()
                {
                    Name = "Tea and crumpets",
                    Price = 5.00m
                },
                new Topping()
                {
                    Name = "Fresh-baked scones",
                    Price = 4.50m,
                },
                new Topping()
                {
                    Name = "Bell peppers",
                    Price = 1.00m,
                },
                new Topping()
                {
                    Name = "Onions",
                    Price = 1.00m,
                },
                new Topping()
                {
                    Name = "Mushrooms",
                    Price = 1.00m,
                },
                new Topping()
                {
                    Name = "Pepperoni",
                    Price = 1.00m,
                },
                new Topping()
                {
                    Name = "Duck sausage",
                    Price = 3.20m,
                },
                new Topping()
                {
                    Name = "Venison meatballs",
                    Price = 2.50m,
                },
                new Topping()
                {
                    Name = "Served on a silver platter",
                    Price = 250.99m,
                },
                new Topping()
                {
                    Name = "Lobster on top",
                    Price = 64.50m,
                },
                new Topping()
                {
                    Name = "Sturgeon caviar",
                    Price = 101.75m,
                },
                new Topping()
                {
                    Name = "Artichoke hearts",
                    Price = 3.40m,
                },
                new Topping()
                {
                    Name = "Fresh tomatos",
                    Price = 1.50m,
                },
                new Topping()
                {
                    Name = "Basil",
                    Price = 1.50m,
                },
                new Topping()
                {
                    Name = "Steak (medium-rare)",
                    Price = 8.50m,
                },
                new Topping()
                {
                    Name = "Blazing hot peppers",
                    Price = 4.20m,
                },
                new Topping()
                {
                    Name = "Buffalo chicken",
                    Price = 5.00m,
                },
                new Topping()
                {
                    Name = "Blue cheese",
                    Price = 2.50m,
                },
            };

            var specials = new PizzaSpecial[]
            {
                new PizzaSpecial()
                {
                    Name = "Basic Cheese Pizza",
                    Description = "It's cheesy and delicious, why wouldn't you want one?",
                    Toppings = new List<PizzaSpecialTopping>()
                    {
                        new PizzaSpecialTopping(){ Topping = toppings[0], },
                    },
                },
                new PizzaSpecial()
                {
                    Id = 2,
                    Name = "The Bazonatorizor",
                    Description = "It has EVERY kind of bacon",
                    Toppings = new List<PizzaSpecialTopping>()
                    {
                        new PizzaSpecialTopping(){ Topping = toppings[1], },
                        new PizzaSpecialTopping(){ Topping = toppings[2], },
                        new PizzaSpecialTopping(){ Topping = toppings[3], },
                    },
                },
                new PizzaSpecial()
                {
                    Id = 3,
                    Name = "Classic pepperoni",
                    Description = "It's the pizza you grew up, but Blazing hot!",
                    Toppings = new List<PizzaSpecialTopping>()
                    {
                        new PizzaSpecialTopping(){ Topping = toppings[9], }
                    },
                },
                new PizzaSpecial()
                {
                    Id = 4,
                    Name = "Buffalo chicken",
                    Description = "Spicy chicken, hot sauce and bleu cheese, guaranteed to warm you up",
                    Toppings = new List<PizzaSpecialTopping>()
                    {
                        new PizzaSpecialTopping(){ Topping = toppings[20], },
                        new PizzaSpecialTopping(){ Topping = toppings[21], },
                    },
                },
                new PizzaSpecial()
                {
                    Id = 5,
                    Name = "Mushroom Lovers",
                    Description = "It's has mushrooms. Isn't that obvious?",
                    Toppings = new List<PizzaSpecialTopping>()
                    {
                        new PizzaSpecialTopping(){ Topping = toppings[6], },
                        new PizzaSpecialTopping(){ Topping = toppings[7], },
                        new PizzaSpecialTopping(){ Topping = toppings[8], },
                    },
                },
                new PizzaSpecial()
                {
                    Id = 6,
                    Name = "The Brit",
                    Description = "When in London...",
                    Toppings = new List<PizzaSpecialTopping>()
                    {
                        new PizzaSpecialTopping(){ Topping = toppings[4], },
                        new PizzaSpecialTopping(){ Topping = toppings[5], },
                        new PizzaSpecialTopping(){ Topping = toppings[12], },
                    },
                },
                new PizzaSpecial()
                {
                    Id = 7,
                    Name = "Veggie Delight",
                    Description = "It's like salad but on a pizza",
                    Toppings = new List<PizzaSpecialTopping>()
                    {
                        new PizzaSpecialTopping(){ Topping = toppings[6], },
                        new PizzaSpecialTopping(){ Topping = toppings[7], },
                        new PizzaSpecialTopping(){ Topping = toppings[8], },
                        new PizzaSpecialTopping(){ Topping = toppings[15], },
                        new PizzaSpecialTopping(){ Topping = toppings[16], },
                    },
                },
                new PizzaSpecial()
                {
                    Id = 8,
                    Name = "Margherita",
                    Description = "Traditional Italian pizza with tomatoes and basil",
                    Toppings = new List<PizzaSpecialTopping>()
                    {
                        new PizzaSpecialTopping(){ Topping = toppings[16], },
                        new PizzaSpecialTopping(){ Topping = toppings[17], },
                    },
                },
            };

            db.Toppings.AddRange(toppings);
            db.Specials.AddRange(specials);
            db.SaveChanges();
        }
    }
}
