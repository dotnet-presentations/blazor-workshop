namespace BlazingPizza.Server;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews();
        services.AddRazorPages();

        services.AddDbContext<PizzaStoreContext>(options =>
            options.UseSqlite("Data Source=pizza.db"));

        services.AddDefaultIdentity<PizzaStoreUser>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<PizzaStoreContext>();

        services.AddIdentityServer()
            .AddApiAuthorization<PizzaStoreUser, PizzaStoreContext>();

        services.AddAuthentication()
            .AddIdentityServerJwt();

        services.AddSignalR(options => options.EnableDetailedErrors = true)
            .AddMessagePackProtocol();

        services.AddHostedService<FakeOrderStatusService>();
        services.AddSingleton<IBackgroundOrderQueue, DefaultBackgroundOrderQueue>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<OrderStatusHub>("/orderstatus");
            endpoints.MapRazorPages();
            endpoints.MapControllers();
            endpoints.MapFallbackToFile("index.html");
        });
    }
}
