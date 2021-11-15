namespace BlazingPizza.Server.Services;

public class DefaultBackgroundOrderQueue : IBackgroundOrderQueue
{
    private readonly Channel<Func<CancellationToken, ValueTask<Order>>> _queue;

    public DefaultBackgroundOrderQueue() =>
        _queue = Channel.CreateBounded<Func<CancellationToken, ValueTask<Order>>>(
            new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

    public ValueTask QueueBackgroundOrderStatusAsync(
        Func<CancellationToken, ValueTask<Order>> workItem) =>
        _queue.Writer.WriteAsync(workItem);

    public ValueTask<Func<CancellationToken, ValueTask<Order>>> DequeueAsync(
        CancellationToken cancellationToken) =>
        _queue.Reader.ReadAsync(cancellationToken);
}
