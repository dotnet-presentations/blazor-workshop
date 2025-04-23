global using BlazingPizza.Shared;
global using BlazingPizza.Client;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Configure HttpClient to use the base address of the server project
builder.Services.AddScoped<HttpClient>(sp => 
	new HttpClient
	{
		BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
	});

// Add Security
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSingleton<AuthenticationStateProvider, PersistentAuthenticationStateProvider>();

builder.Services.AddScoped<IRepository, HttpRepository>();
builder.Services.AddScoped<OrderState>();

await builder.Build().RunAsync();
