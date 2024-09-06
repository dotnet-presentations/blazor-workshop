// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza;

public sealed class Order
{
    public int OrderId { get; set; }

    // Set by the server during POST
    public string? UserId { get; set; }

    public DateTime CreatedTime { get; set; }

    public Address DeliveryAddress { get; set; } = new();

    // Set by server during POST
    public LatLong? DeliveryLocation { get; set; }

    public List<Pizza> Pizzas { get; set; } = [];

    public decimal GetTotalPrice() => Pizzas.Sum(p => p.GetTotalPrice());

    public string GetFormattedTotalPrice() => GetTotalPrice().ToString("0.00");
}

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(Order))]
[JsonSerializable(typeof(OrderWithStatus))]
[JsonSerializable(typeof(List<OrderWithStatus>))]
[JsonSerializable(typeof(Pizza))]
[JsonSerializable(typeof(List<PizzaSpecial>))]
[JsonSerializable(typeof(List<Topping>))]
[JsonSerializable(typeof(Topping))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class OrderContext : JsonSerializerContext { }
