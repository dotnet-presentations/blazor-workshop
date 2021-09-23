using System.Threading.Tasks;

namespace BlazingPizza.Server.Hubs
{
    public interface IOrderStatusHub
    {
        /// <summary>
        /// This event name should match this: <see cref="OrderStatusHubConsts.EventNames.OrderStatusChanged"/>.
        /// </summary>
        Task OrderStatusChanged(OrderWithStatus order);
    }
}
