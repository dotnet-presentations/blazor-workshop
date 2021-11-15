var host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
    .Build();

// Initialize the database
var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PizzaStoreContext>();
    if (await db.Database.EnsureCreatedAsync())
    {
        await SeedData.InitializeAsync(db);
    }
}

await host.RunAsync();
