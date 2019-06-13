using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazingPizza.Client
{
    public static class JSRuntimeExtensions
    {
        public static Task<bool> Confirm(this IJSRuntime jsRuntime, string message)
        {
            return jsRuntime.InvokeAsync<bool>("confirm", message);
        }
    }
}
