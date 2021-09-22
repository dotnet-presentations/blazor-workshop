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
        /// </summary>
        public Task StartTrackingOrder(Order order) =>
            Groups.AddToGroupAsync(
                Context.ConnectionId, ToOrderTrackingGroupId(order));

        /// <summary>
        /// Removes the current connection from the order's unique group identifier, 
        /// ending real-time change updates for this order.
        /// </summary>
        public Task StopTrackingOrder(Order order) =>
            Groups.RemoveFromGroupAsync(
                Context.ConnectionId, ToOrderTrackingGroupId(order));

        /// <summary>
        /// Call to notify connected clients about changes to an order.
        /// </summary>
        public Task OrderUpdated(OrderWithStatus orderWithStatus) =>
             Clients.Group(ToOrderTrackingGroupId(orderWithStatus.Order))
                .OrderStatusChanged(orderWithStatus);

        private static string ToOrderTrackingGroupId(Order order) =>
            $"{order.OrderId}:{order.UserId}";
    }
}
