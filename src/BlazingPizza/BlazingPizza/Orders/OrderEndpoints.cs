// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Orders;

internal static class OrderEndpoints
{
    internal static WebApplication MapOrderEndpoints(this WebApplication app)
    {
        var orders = app.MapGroup("api/orders");

        orders.MapGet("/{orderId:int}", OnGetOrderAsync)
            .Produces<OrderWithStatus>(200)
            .Produces<NotFound>(404);

        orders.MapPost("/", OnPostOrderAsync)
            .ProducesValidationProblem()
            .Produces<int>(200);

        return app;
    }

    static async Task<IResult> OnGetOrderAsync(int orderId, HttpContext context, PizzaStoreContext db)
    {
        var userId = context.GetUserId();

        var order = await db.Orders
            .Where(o => o.OrderId == orderId)
            .Where(o => o.UserId == userId)
            .Include(o => o.DeliveryLocation)
            .Include(o => o.Pizzas).ThenInclude(p => p.Special)
            .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
            .SingleOrDefaultAsync();

        if (order is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(OrderWithStatus.FromOrder(order));
    }

    static async Task<IResult> OnPostOrderAsync(Order order, HttpContext context, PizzaStoreContext db)
    {
        var userId = context.GetUserId();

        order.CreatedTime = DateTime.Now;
        order.DeliveryLocation = new LatLong(51.5001, -0.1239);
        order.UserId = userId;

        // Enforce existence of Pizza.SpecialId and Topping.ToppingId
        // in the database - prevent the submitter from making up
        // new specials and toppings
        foreach (var pizza in order.Pizzas)
        {
            pizza.SpecialId = pizza.Special?.Id ?? 0;
            pizza.Special = null;

            foreach (var topping in pizza.Toppings)
            {
                topping.ToppingId = topping.Topping?.Id ?? 0;
                topping.Topping = null;
            }
        }

        db.Orders.Attach(order);

        await db.SaveChangesAsync();

        // In the background, send push notifications if possible
        var subscription = await db.NotificationSubscriptions.Where(e => e.UserId == userId).SingleOrDefaultAsync();

        if (subscription is not null)
        {
            _ = TrackAndSendNotificationsAsync(order, subscription);
        }

        return Results.Ok(order.OrderId);
    }

    private static async Task TrackAndSendNotificationsAsync(Order order, NotificationSubscription subscription)
    {
        // In a realistic case, some other backend process would track
        // order delivery progress and send us notifications when it
        // changes. Since we don't have any such process here, fake it.
        await Task.Delay(OrderWithStatus.PreparationDuration);
        await SendNotificationAsync(order, subscription, "Your order has been dispatched!");

        await Task.Delay(OrderWithStatus.DeliveryDuration);
        await SendNotificationAsync(order, subscription, "Your order is now delivered. Enjoy!");
    }

    private static async Task SendNotificationAsync(Order order, NotificationSubscription subscription, string message)
    {
        // For a real application, generate your own
        var publicKey = "BLC8GOevpcpjQiLkO7JmVClQjycvTCYWm6Cq_a7wJZlstGTVZvwGFFHMYfXt6Njyvgx_GlXJeo5cSiZ1y4JOx1o";
        var privateKey = "OrubzSz3yWACscZXjFQrrtDwCKg-TGFuWhluQ2wLXDo";

        var pushSubscription = new PushSubscription(subscription.Url, subscription.P256dh, subscription.Auth);
        var vapidDetails = new VapidDetails("mailto:<someone@example.com>", publicKey, privateKey);
        using var webPushClient = new WebPushClient();

        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                message,
                url = $"orders/{order.OrderId}",
            });

            await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error sending push notification: " + ex.Message);
        }
    }
}
