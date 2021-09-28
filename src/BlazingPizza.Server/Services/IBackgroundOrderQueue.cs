using System;
using System.Threading;
using System.Threading.Tasks;

namespace BlazingPizza.Server.Services
{
    public interface IBackgroundOrderQueue
    {
        ValueTask QueueBackgroundOrderStatusAsync(
               Func<CancellationToken, ValueTask<Order>> workItem);

        ValueTask<Func<CancellationToken, ValueTask<Order>>> DequeueAsync(
            CancellationToken cancellationToken);
    }
}
