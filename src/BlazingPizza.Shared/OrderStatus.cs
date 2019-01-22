using System;

namespace BlazingPizza
{
    public enum OrderStatus
    {
        Processing,
        OutForDelivery,
        Delivered,
    }

    public static class OrderStatusExtensions
    {
        public static string ToDisplayString(this OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Processing:
                    return "Processing";
                case OrderStatus.OutForDelivery:
                    return "Out for delivery";
                case OrderStatus.Delivered:
                    return "Delivered";
                default:
                    throw new ArgumentException($"Unknown status: '{status}'");
            }
        }
    }
}
