// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Extensions;

internal static class HttpContextExtensions
{
    internal static string? GetUserId(this HttpContext context) =>
        context.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
