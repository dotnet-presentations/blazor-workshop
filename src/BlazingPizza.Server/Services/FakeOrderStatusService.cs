namespace BlazingPizza.Server.Services;

public sealed class FakeOrderStatusService : BackgroundService
{
    private readonly IBackgroundOrderQueue _orderQueue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FakeOrderStatusService> _logger;

    public FakeOrderStatusService(
        IBackgroundOrderQueue orderQueue,
        IServiceProvider serviceProvider,
        ILogger<FakeOrderStatusService> logger) =>
        (_orderQueue, _serviceProvider, _logger) = (orderQueue, serviceProvider, logger);

    [SuppressMessage(
        "Style",
        "IDE0063:Use simple 'using' statement",
        Justification = "We need explicit disposal of the IServiceScope to avoid errant conditions.")]
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _orderQueue.DequeueAsync(stoppingToken);
                var order = await workItem(stoppingToken);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var hubContext =
                        scope.ServiceProvider
                            .GetRequiredService<IHubContext<OrderStatusHub, IOrderStatusHub>>();

                    // This is not production code, this is intended to act as a
                    // fake backend order processing system. DO NOT DO THIS.
                    var trackingOrderId = order.ToOrderTrackingGroupId();
                    var orderWithStatus = await GetOrderAsync(scope.ServiceProvider, order.OrderId);
                    while (!orderWithStatus.IsDelivered)
                    {
                        await hubContext.Clients.Group(trackingOrderId).OrderStatusChanged(orderWithStatus);
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

                        orderWithStatus = OrderWithStatus.FromOrder(orderWithStatus.Order);
                    }

                    // Send final delivery status update.
                    await hubContext.Clients.Group(trackingOrderId).OrderStatusChanged(orderWithStatus);
                }
            }
            catch (OperationCanceledException)
            {
                // Prevent throwing if stoppingToken was signaled
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred executing task work item.");
            }
        }
    }

    static async Task<OrderWithStatus> GetOrderAsync(IServiceProvider serviceProvider, int orderId)
    {
        var pizzeStoreContext =
            serviceProvider.GetRequiredService<PizzaStoreContext>();

        var order = await pizzeStoreContext.Orders
            .Where(o => o.OrderId == orderId)
            .Include(o => o.DeliveryLocation)
            .Include(o => o.Pizzas).ThenInclude(p => p.Special)
            .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
            .SingleOrDefaultAsync();

        return OrderWithStatus.FromOrder(order);
    }
}
