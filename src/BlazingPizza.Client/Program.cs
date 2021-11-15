var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddHttpClient<OrdersClient>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
builder.Services.AddScoped<OrderState>();

// Add auth services
builder.Services.AddApiAuthorization<PizzaAuthenticationState>(options =>
{
    options.AuthenticationPaths.LogOutSucceededPath = "";
});

await builder.Build().RunAsync();
