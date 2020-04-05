using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace BlazingPizza.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            builder.Services.AddBaseAddressHttpClient();
            builder.Services.AddScoped<OrderState>();

            // Add auth services
            builder.Services.AddRemoteAuthentication<PizzaAuthenticationState, ApiAuthorizationProviderOptions>();
            builder.Services.AddApiAuthorization(options =>
            {
                options.AuthenticationPaths.LogOutSucceededPath = "";
                options.ProviderOptions.ConfigurationEndpoint = "_configuration/BlazingPizza.Client"; // temporary workaround
            });

            await builder.Build().RunAsync();
        }
    }
}
