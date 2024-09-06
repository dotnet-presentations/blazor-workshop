// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Pizzas;

internal static class PizzaEndpoints
{
    internal static WebApplication MapPizzaEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("api");

        var pizzas = api.MapGroup("pizzas");

        // Get api/pizzas/specials
        pizzas.MapGet("/specials", OnGetSpecialAsync)
            .Produces<PizzaSpecial[]>(200);

        // Get api/pizzas/toppings
        pizzas.MapGet("/toppings", OnGetToppingsAsync)
            .Produces<Topping[]>(200);

        return app;
    }

    static async Task<IResult> OnGetSpecialAsync(PizzaStoreContext db)
    {
        var specials = await db.Specials.ToListAsync();

        return Results.Ok(specials);
    }

    static async Task<IResult> OnGetToppingsAsync(PizzaStoreContext db)
    {
        var toppings = await db.Toppings.OrderBy(t => t.Name).ToListAsync();

        return Results.Ok(toppings);
    }
}
