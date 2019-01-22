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
            // To simulate a real backend process, we fake status updates based on the amount
            // of time since the order was placed
            string statusText;
            List<Marker> mapMarkers;
            var dispatchTime = order.CreatedTime.AddSeconds(10);
            var deliveryDuration = TimeSpan.FromMinutes(1); // Unrealistic, but more interesting to watch

            if (DateTime.Now < dispatchTime)
            {
                statusText = "Preparing";
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("You", order.DeliveryLocation, showPopup: true)
                };
            }
            else if (DateTime.Now < dispatchTime + deliveryDuration)
            {
                statusText = "Out for delivery";

                var startPosition = new LatLong(51.5098, -0.124);
                var proportionOfDeliveryCompleted = Math.Min(1, (DateTime.Now - dispatchTime).TotalMilliseconds / deliveryDuration.TotalMilliseconds);
                var driverPosition = LatLong.Interpolate(startPosition, order.DeliveryLocation, proportionOfDeliveryCompleted);
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("You", order.DeliveryLocation),
                    ToMapMarker("Driver", driverPosition, showPopup: true),
                };
            }
            else
            {
                statusText = "Delivered";
                mapMarkers = new List<Marker>
                {
                    ToMapMarker("Delivery location", order.DeliveryLocation, showPopup: true),
                };
            }

            return new OrderWithStatus
            {
                Order = order,
                StatusText = statusText,
                MapMarkers = mapMarkers,
            };
        }

        static Marker ToMapMarker(string description, LatLong coords, bool showPopup = false)
            => new Marker { Description = description, X = coords.Longitude, Y = coords.Latitude, ShowPopup = showPopup };
    }
}
