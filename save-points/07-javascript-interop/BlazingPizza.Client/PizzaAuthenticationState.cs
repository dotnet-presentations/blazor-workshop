using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazingPizza.Client;

public class PizzaAuthenticationState : RemoteAuthenticationState
{
    public Order Order { get; set; }
}
