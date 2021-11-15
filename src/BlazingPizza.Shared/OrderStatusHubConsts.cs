namespace BlazingPizza;

public class OrderStatusHubConsts
{
    public class MethodNames
    {
        public const string StartTrackingOrder = nameof(StartTrackingOrder);
        public const string StopTrackingOrder = nameof(StopTrackingOrder);
    }

    public class EventNames
    {
        public const string OrderStatusChanged = nameof(OrderStatusChanged);
    }
}
