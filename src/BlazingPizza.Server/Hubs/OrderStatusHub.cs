using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BlazingPizza.Server.Hubs
{
    [Authorize]
    public class OrderStatusHub : Hub<IOrderStatusHub>
    {
        /// <summary>
        /// Adds the current connection to the order's unique group identifier, where 
        /// order status changes will be notified in real-time.
        /// This method name should match this: <see cref="OrderStatusHubConsts.MethodNames.StartTrackingOrder"/>.
        /// </summary>
        public Task StartTrackingOrder(Order order) =>
            Groups.AddToGroupAsync(
                Context.ConnectionId, ToOrderTrackingGroupId(order));

        /// <summary>
        /// Removes the current connection from the order's unique group identifier, 
        /// ending real-time change updates for this order.
        /// This method name should match this: <see cref="OrderStatusHubConsts.MethodNames.StopTrackingOrder"/>.
        /// </summary>
        public Task StopTrackingOrder(Order order) =>
            Groups.RemoveFromGroupAsync(
                Context.ConnectionId, ToOrderTrackingGroupId(order));

        private static string ToOrderTrackingGroupId(Order order) =>
            $"{order.OrderId}:{order.UserId}";
    }
}
