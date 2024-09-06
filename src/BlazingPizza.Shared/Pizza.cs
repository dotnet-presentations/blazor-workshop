// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza;

public sealed class Pizza
{
    public const int DefaultSize = 12;
    public const int MinimumSize = 9;
    public const int MaximumSize = 17;

    public int Id { get; set; }

    public int OrderId { get; set; }

    public PizzaSpecial? Special { get; set; }

    public int SpecialId { get; set; }

    public int Size { get; set; }

    public List<PizzaTopping> Toppings { get; set; } = [];

    public decimal GetBasePrice()
    {
        if (Special is null)
        {
            throw new NullReferenceException(
                $"{nameof(Special)} was null when calculating Base Price.");
        }

        return Size / (decimal)DefaultSize * Special.BasePrice;
    }

    public decimal GetTotalPrice()
    {
        if (Toppings.Any(static t => t.Topping is null))
        {
            throw new NullReferenceException(
                $"{nameof(Toppings)} contained null when calculating the Total Price.");
        }

        return GetBasePrice() + Toppings.Sum(t => t.Topping!.Price);
    }

    public string GetFormattedTotalPrice()
    {
        return GetTotalPrice().ToString("0.00");
    }
}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(Pizza))]
public partial class PizzaContext : JsonSerializerContext { }
