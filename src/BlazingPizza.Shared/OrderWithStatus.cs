using System;

namespace BlazingPizza
{
    public class OrderWithStatus
    {
        public Order Order { get; set; }

        public string StatusText { get; set; }

        public LatLong TrackingLocation { get; set; }

        public static OrderWithStatus FromOrder(Order order)
        {
            return new OrderWithStatus
            {
                Order = order,
                StatusText = "Fake status",
            };
        }
    }
}
