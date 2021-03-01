using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazingPizza.Client
{
    public class PizzaAuthenticationState : RemoteAuthenticationState
    {
        public Order Order { get; set; }
    }
}
