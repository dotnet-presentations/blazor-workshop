// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace BlazingPizza.Notifications;

internal static class NotificationEndpoints
{
    internal static WebApplication MapNotificationEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("api");

        // Subscribe to notifications
        api.MapPut("/notifications/subscribe", OnSubscribeToNotificationsAsync)
            .Produces<NotificationSubscription>(200)
            .Produces<UnauthorizedHttpResult>(401);

        return app;
    }

    [Authorize]
    static async Task<IResult> OnSubscribeToNotificationsAsync(
    HttpContext context,
    PizzaStoreContext db,
    NotificationSubscription subscription)
    {
        // We're storing at most one subscription per user, so delete old ones.
        // Alternatively, you could let the user register multiple subscriptions from different browsers/devices.
        var userId = context.GetUserId();

        if (userId is null)
        {
            return Results.Unauthorized();
        }

        var oldSubscriptions = db.NotificationSubscriptions.Where(e => e.UserId == userId);
        db.NotificationSubscriptions.RemoveRange(oldSubscriptions);

        // Store new subscription
        subscription = subscription with { UserId = userId };

        db.NotificationSubscriptions.Attach(subscription);

        await db.SaveChangesAsync();

        return Results.Ok(subscription);
    }
}
