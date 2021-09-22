using System.Threading.Tasks;

namespace BlazingPizza.Server.Hubs
{
    public interface IOrderStatusHub
    {
        Task OrderStatusChanged(OrderWithStatus order);
    }
}
