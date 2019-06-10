using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace BlazingPizza.ComponentsLibrary
{
    public static class LocalStorage
    {
        public static Task<T> GetAsync<T>(IJSRuntime jsRuntime, string key)
            => jsRuntime.InvokeAsync<T>("blazorLocalStorage.get", key);

        public static Task SetAsync(IJSRuntime jsRuntime, string key, object value)
            => jsRuntime.InvokeAsync<object>("blazorLocalStorage.set", key, value);

        public static Task DeleteAsync(IJSRuntime jsRuntime, string key)
            => jsRuntime.InvokeAsync<object>("blazorLocalStorage.delete", key);
    }
}
