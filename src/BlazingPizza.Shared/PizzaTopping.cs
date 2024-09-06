// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza;

public sealed class PizzaTopping
{
    public Topping? Topping { get; set; }

    public int ToppingId { get; set; }

    public int PizzaId { get; set; }
}
