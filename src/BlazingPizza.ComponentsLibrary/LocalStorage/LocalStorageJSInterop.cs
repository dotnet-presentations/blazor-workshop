// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.JSInterop;

namespace BlazingPizza.ComponentsLibrary;

public sealed class LocalStorageJSInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask =
        new(valueFactory: () => jsRuntime.InvokeAsync<IJSObjectReference>(
            identifier: "import", 
            args: "./_content/BlazingPizza.ComponentsLibrary/localStorage.js").AsTask());

    public async ValueTask<T> GetLocalStorageItemAsync<T>(string key)
    {
        var module = await _moduleTask.Value;

        return await module.InvokeAsync<T>("getLocalStorageItem", key);
    }

    public async ValueTask SetLocalStorageItemAsync<T>(string key, T value)
    {
        var module = await _moduleTask.Value;

        await module.InvokeVoidAsync("setLocalStorageItem", key, value);
    }

    public async ValueTask DeleteLocalStorageItemAsync(string key)
    {
        var module = await _moduleTask.Value;

        await module.InvokeVoidAsync("deleteLocalStorageItem", key);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;

            await module.DisposeAsync();
        }
    }
}
