

using System;

namespace BlazingPizza
{
    public class Pizza
    {
        public static int GetSizeInInches(PizzaSize size)
        {
            switch (size)
            {
                case PizzaSize.Small:
                    return 9;

                case PizzaSize.Medium:
                    return 12;

                case PizzaSize.Large:
                    return 17;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
