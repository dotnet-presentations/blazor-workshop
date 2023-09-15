namespace BlazingPizza;

public class NotificationSubscription
{
    public required int NotificationSubscriptionId { get; set; }

    public required string UserId { get; set; }

    public required string Url { get; set; }

    public required string P256dh { get; set; }

    public required string Auth { get; set; }
}
