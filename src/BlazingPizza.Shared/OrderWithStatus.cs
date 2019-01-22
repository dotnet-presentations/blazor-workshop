using BlazingPizza.OpenStreetMap;
using System;
using System.Collections.Generic;

namespace BlazingPizza
{
    public class OrderWithStatus
    {
        public Order Order { get; set; }

        public string StatusText { get; set; }

        public List<Marker> MapMarkers { get; set; }

        public static OrderWithStatus FromOrder(Order order)
        {
            return new OrderWithStatus
            {
                Order = order,
                StatusText = "Fake status",
                MapMarkers = new List<Marker>
                {
                    new Marker { Description = "Driver", X = -0.124, Y = 51.5098 },
                    new Marker { Description = "You", X = order.DeliveryLocation.Longitude, Y = order.DeliveryLocation.Latitude },
                },
            };
        }
    }
}
