// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza;

public sealed class Address
{
    public int Id { get; set; }

    [Required, StringLength(100, MinimumLength = 3)]
    public string? Name { get; set; }

    [Required, StringLength(100, MinimumLength = 3)]
    public string? Line1 { get; set; }

    [MaxLength(100)]
    public string? Line2 { get; set; }

    [Required, StringLength(50, MinimumLength = 3)]
    public string? City { get; set; }

    [Required, StringLength(20, MinimumLength = 3)]
    public string? Region { get; set; }

    [Required, StringLength(20, MinimumLength = 2)]
    public string? PostalCode { get; set; }
}
