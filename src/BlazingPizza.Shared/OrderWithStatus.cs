// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza;

public sealed class OrderWithStatus
{
    public static readonly TimeSpan PreparationDuration = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan DeliveryDuration = TimeSpan.FromMinutes(1); // Unrealistic, but more interesting to watch

    // Set from DB
    public Order Order { get; set; } = null!;

    // Set from Order
    public string StatusText { get; set; } = null!;

    // Case sensitive compare
    public bool IsDelivered => StatusText is "Delivered";

    public List<Marker> MapMarkers { get; set; } = null!;

    public static OrderWithStatus FromOrder(Order order)
    {
        ArgumentNullException.ThrowIfNull(order.DeliveryLocation);

        // To simulate a real backend process, we fake status updates based on the amount
        // of time since the order was placed
        string statusText;
        List<Marker> mapMarkers;
        var dispatchTime = order.CreatedTime.Add(PreparationDuration);

        if (DateTime.Now < dispatchTime)
        {
            statusText = "Preparing";
            mapMarkers =
            [
                ToMapMarker("You", order.DeliveryLocation, showPopup: true)
            ];
        }
        else if (DateTime.Now < dispatchTime + DeliveryDuration)
        {
            statusText = "Out for delivery";

            var startPosition = ComputeStartPosition(order);
            var proportionOfDeliveryCompleted = Math.Min(1, (DateTime.Now - dispatchTime).TotalMilliseconds / DeliveryDuration.TotalMilliseconds);
            var driverPosition = LatLong.Interpolate(startPosition, order.DeliveryLocation, proportionOfDeliveryCompleted);
            mapMarkers =
            [
                ToMapMarker("You", order.DeliveryLocation),
                ToMapMarker("Driver", driverPosition, showPopup: true),
            ];
        }
        else
        {
            statusText = "Delivered";
            mapMarkers =
            [
                ToMapMarker("Delivery location", order.DeliveryLocation, showPopup: true),
            ];
        }

        return new OrderWithStatus
        {
            Order = order,
            StatusText = statusText,
            MapMarkers = mapMarkers,
        };
    }

    private static LatLong ComputeStartPosition(Order order)
    {
        ArgumentNullException.ThrowIfNull(order.DeliveryLocation);

        // Random but deterministic based on order ID
        var rng = new Random(order.OrderId);
        var distance = 0.01 + rng.NextDouble() * 0.02;
        var angle = rng.NextDouble() * Math.PI * 2;
        var offset = (distance * Math.Cos(angle), distance * Math.Sin(angle));

        return new LatLong(
            order.DeliveryLocation.Latitude + offset.Item1,
            order.DeliveryLocation.Longitude + offset.Item2);
    }

    private static Marker ToMapMarker(string description, LatLong coords, bool showPopup = false) =>
        new(description, coords.Longitude, coords.Latitude, showPopup);
}
