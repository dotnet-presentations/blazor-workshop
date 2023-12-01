namespace BlazingPizza;

public class Address
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Line1 { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Line2 { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Region { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;
}
