namespace BlazingPizza.Server.Services;

public interface IBackgroundOrderQueue
{
    ValueTask QueueBackgroundOrderStatusAsync(
           Func<CancellationToken, ValueTask<Order>> workItem);

    ValueTask<Func<CancellationToken, ValueTask<Order>>> DequeueAsync(
        CancellationToken cancellationToken);
}
