// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza;

public sealed record class Marker(
    string Description,
    double X,
    double Y,
    bool ShowPopup) : Point(X, Y);
