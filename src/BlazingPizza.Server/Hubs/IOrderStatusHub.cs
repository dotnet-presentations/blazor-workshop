namespace BlazingPizza.Server.Hubs;

public interface IOrderStatusHub
{
    /// <summary>
    /// This event name should match <see cref="OrderStatusHubConsts.EventNames.OrderStatusChanged"/>,
    /// which is shared with clients for discoverability.
    /// </summary>
    Task OrderStatusChanged(OrderWithStatus order);
}
