// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza;

public sealed class PizzaSpecial
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public decimal BasePrice { get; set; }

    public string Description { get; set; } = "";

    public string ImageUrl { get; set; } = "img/pizzas/cheese.jpg";

    public string GetFormattedBasePrice() => BasePrice.ToString("0.00");
}
