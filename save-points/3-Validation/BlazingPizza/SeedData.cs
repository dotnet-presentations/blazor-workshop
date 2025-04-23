namespace BlazingPizza;

public static class SeedData
{
    public static void Initialize(PizzaStoreContext db)
    {
        var toppings = new Topping[]
        {
            new Topping()
            {
                    Id=1,
                    Name = "Extra cheese",
                    Price = 2.50m,
            },
            new Topping()
            {
                    Id=2,
                    Name = "American bacon",
                    Price = 2.99m,
            },
            new Topping()
            {
                    Id=3,
                    Name = "British bacon",
                    Price = 2.99m,
            },
            new Topping()
            {
                    Id=4,
                    Name = "Canadian bacon",
                    Price = 2.99m,
            },
            new Topping()
            {
                    Id=5,
                    Name = "Tea and crumpets",
                    Price = 5.00m
            },
            new Topping()
            {
                    Id=6,
                    Name = "Fresh-baked scones",
                    Price = 4.50m,
            },
            new Topping()
            {
                    Id=7,
                    Name = "Bell peppers",
                    Price = 1.00m,
            },
            new Topping()
            {
                    Id=8,
                    Name = "Onions",
                    Price = 1.00m,
            },
            new Topping()
            {
                    Id=9,
                    Name = "Mushrooms",
                    Price = 1.00m,
            },
            new Topping()
            {
                    Id=10,
                    Name = "Pepperoni",
                    Price = 1.00m,
            },
            new Topping()
            {
                    Id=11,
                    Name = "Duck sausage",
                    Price = 3.20m,
            },
            new Topping()
            {
                    Id=12,
                    Name = "Venison meatballs",
                    Price = 2.50m,
            },
            new Topping()
            {
                    Id=13,
                    Name = "Served on a silver platter",
                    Price = 250.99m,
            },
            new Topping()
            {
                    Id=14,
                    Name = "Lobster on top",
                    Price = 64.50m,
            },
            new Topping()
            {
                    Id=15,
                    Name = "Sturgeon caviar",
                    Price = 101.75m,
            },
            new Topping()
            {
                    Id=16,
                    Name = "Artichoke hearts",
                    Price = 3.40m,
            },
            new Topping()
            {
                    Id=17,
                    Name = "Fresh tomatoes",
                    Price = 1.50m,
            },
            new Topping()
            {
                    Id=18,
                    Name = "Basil",
                    Price = 1.50m,
            },
            new Topping()
            {
                    Id=19,
                    Name = "Steak (medium-rare)",
                    Price = 8.50m,
            },
            new Topping()
            {
                    Id=20,
                    Name = "Blazing hot peppers",
                    Price = 4.20m,
            },
            new Topping()
            {
                    Id=21,
                    Name = "Buffalo chicken",
                    Price = 5.00m,
            },
            new Topping()
            {
                    Id=22,
                    Name = "Blue cheese",
                    Price = 2.50m,
            },
        };

        var specials = new PizzaSpecial[]
        {
            new PizzaSpecial()
            {
                    Name = "Basic Cheese Pizza",
                    Description = "It's cheesy and delicious. Why wouldn't you want one?",
                    BasePrice = 9.99m,
                    ImageUrl = "img/pizzas/cheese.jpg",
            },
            new PizzaSpecial()
            {
                    Id = 2,
                    Name = "The Baconatorizor",
                    Description = "It has EVERY kind of bacon",
                    BasePrice = 11.99m,
                    ImageUrl = "img/pizzas/bacon.jpg",
            },
            new PizzaSpecial()
            {
                    Id = 3,
                    Name = "Classic pepperoni",
                    Description = "It's the pizza you grew up with, but Blazing hot!",
                    BasePrice = 10.50m,
                    ImageUrl = "img/pizzas/pepperoni.jpg",
            },
            new PizzaSpecial()
            {
                    Id = 4,
                    Name = "Buffalo chicken",
                    Description = "Spicy chicken, hot sauce and bleu cheese, guaranteed to warm you up",
                    BasePrice = 12.75m,
                    ImageUrl = "img/pizzas/meaty.jpg",
            },
            new PizzaSpecial()
            {
                    Id = 5,
                    Name = "Mushroom Lovers",
                    Description = "It has mushrooms. Isn't that obvious?",
                    BasePrice = 11.00m,
                    ImageUrl = "img/pizzas/mushroom.jpg",
            },
            new PizzaSpecial()
            {
                    Id = 6,
                    Name = "The Brit",
                    Description = "When in London...",
                    BasePrice = 10.25m,
                    ImageUrl = "img/pizzas/brit.jpg",
            },
            new PizzaSpecial()
            {
                    Id = 7,
                    Name = "Veggie Delight",
                    Description = "It's like salad, but on a pizza",
                    BasePrice = 11.50m,
                    ImageUrl = "img/pizzas/salad.jpg",
            },
            new PizzaSpecial()
            {
                    Id = 8,
                    Name = "Margherita",
                    Description = "Traditional Italian pizza with tomatoes and basil",
                    BasePrice = 9.99m,
                    ImageUrl = "img/pizzas/margherita.jpg",
            },
        };

        db.Toppings.AddRange(toppings);
        db.Specials.AddRange(specials);
        db.SaveChanges();
    }
}