namespace BlazingPizza.Server.Hubs;

[Authorize]
public class OrderStatusHub : Hub<IOrderStatusHub>
{
    /// <summary>
    /// Adds the current connection to the order's unique group identifier, where 
    /// order status changes will be notified in real-time.
    /// This method name should match <see cref="OrderStatusHubConsts.MethodNames.StartTrackingOrder"/>,
    /// which is shared with clients for discoverability.
    /// </summary>
    public Task StartTrackingOrder(Order order) =>
        Groups.AddToGroupAsync(
            Context.ConnectionId, order.ToOrderTrackingGroupId());

    /// <summary>
    /// Removes the current connection from the order's unique group identifier, 
    /// ending real-time change updates for this order.
    /// This method name should match <see cref="OrderStatusHubConsts.MethodNames.StopTrackingOrder"/>,
    /// which is shared with clients for discoverability.
    /// </summary>
    public Task StopTrackingOrder(Order order) =>
        Groups.RemoveFromGroupAsync(
            Context.ConnectionId, order.ToOrderTrackingGroupId());
}
