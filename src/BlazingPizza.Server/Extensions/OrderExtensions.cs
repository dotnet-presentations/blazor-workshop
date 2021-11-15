namespace BlazingPizza.Server.Extensions;

internal static class OrderExtensions
{
    /// <summary>
    /// Creates a unique identifier for the given <paramref name="order"/> instance.
    /// </summary>
    internal static string ToOrderTrackingGroupId(this Order order) =>
        $"{order.OrderId}:{order.UserId}:{order.CreatedTime.Ticks}";
}
