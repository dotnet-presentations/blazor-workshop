namespace BlazingPizza;

/// <summary>
/// Represents a customized pizza as part of an order
/// </summary>
public class Pizza
{
    public const int DefaultSize = 12;
    public const int MinimumSize = 9;
    public const int MaximumSize = 17;

    public int Id { get; set; }

    public int OrderId { get; set; }

    public PizzaSpecial Special { get; set; }

    public int SpecialId { get; set; }

    public int Size { get; set; }

    public List<PizzaTopping> Toppings { get; set; }

    public decimal GetBasePrice() =>
        (decimal)Size / DefaultSize * Special?.BasePrice ?? 1;

    public decimal GetTotalPrice() =>
        GetBasePrice() + Toppings?.Sum(t => t.Topping.Price) ?? 0;

    public string GetFormattedTotalPrice() =>
        GetTotalPrice().ToString("0.00");
}
