// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Client.Extensions;

internal static class JSRuntimeExtensions
{
    internal static ValueTask<bool> ConfirmAsync(
        this IJSRuntime jSRuntime, string message) =>
        jSRuntime.InvokeAsync<bool>("confirm", message);
}
