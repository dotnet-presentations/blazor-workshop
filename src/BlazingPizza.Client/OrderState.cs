using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlazingPizza.Client
{
    public class OrderState
    {
        public event EventHandler StateChanged;

        public bool ShowingConfigureDialog { get; private set; }

        public Pizza ConfiguringPizza { get; private set; }

        public Order Order { get; private set; } = new Order();

        public void ShowConfigurePizzaDialog(PizzaSpecial special)
        {
            ConfiguringPizza = new Pizza()
            {
                Special = special,
                SpecialId = special.Id,
                Size = Pizza.DefaultSize,
                Toppings = new List<PizzaTopping>(),
            };

            ShowingConfigureDialog = true;
        }

        public void CancelConfigurePizzaDialog()
        {
            ConfiguringPizza = null;

            ShowingConfigureDialog = false;
            StateHasChanged();
        }

        public void ConfirmConfigurePizzaDialog()
        {
            Order.Pizzas.Add(ConfiguringPizza);
            ConfiguringPizza = null;

            ShowingConfigureDialog = false;
            StateHasChanged();
        }

        public void RemoveConfiguredPizza(Pizza pizza)
        {
            Order.Pizzas.Remove(pizza);
            StateHasChanged();
        }

        public void ResetOrder()
        {
            Order = new Order();
        }

        private void StateHasChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
