// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddLocalStorageJSInterop();

builder.Services.AddHttpClient<PizzaClient>(
    client => client.BaseAddress = new(builder.HostEnvironment.BaseAddress));

builder.Services.AddScoped<OrderState>();

builder.Services.AddHttpClient<OrdersClient>(
    client => client.BaseAddress = new(builder.HostEnvironment.BaseAddress));

await builder.Build().RunAsync();
