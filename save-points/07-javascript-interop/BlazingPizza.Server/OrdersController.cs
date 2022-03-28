using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebPush;

namespace BlazingPizza.Server;

[Route("orders")]
[ApiController]
[Authorize]
public class OrdersController : Controller
{
    private readonly PizzaStoreContext _db;

    public OrdersController(PizzaStoreContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderWithStatus>>> GetOrders()
    {
        var orders = await _db.Orders
                .Where(o => o.UserId == GetUserId())
                .Include(o => o.DeliveryLocation)
                .Include(o => o.Pizzas).ThenInclude(p => p.Special)
                .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
                .OrderByDescending(o => o.CreatedTime)
                .ToListAsync();

        return orders.Select(o => OrderWithStatus.FromOrder(o)).ToList();
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderWithStatus>> GetOrderWithStatus(int orderId)
    {
        var order = await _db.Orders
                .Where(o => o.OrderId == orderId)
                .Where(o => o.UserId == GetUserId())
                .Include(o => o.DeliveryLocation)
                .Include(o => o.Pizzas).ThenInclude(p => p.Special)
                .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
                .SingleOrDefaultAsync();

        if (order == null)
        {
            return NotFound();
        }

        return OrderWithStatus.FromOrder(order);
    }

    [HttpPost]
    public async Task<ActionResult<int>> PlaceOrder(Order order)
    {
        order.CreatedTime = DateTime.Now;
        order.DeliveryLocation = new LatLong(51.5001, -0.1239);
        order.UserId = GetUserId();

        // Enforce existence of Pizza.SpecialId and Topping.ToppingId
        // in the database - prevent the submitter from making up
        // new specials and toppings
        foreach (var pizza in order.Pizzas)
        {
            pizza.SpecialId = pizza.Special.Id;
            pizza.Special = null;

            foreach (var topping in pizza.Toppings)
            {
                topping.ToppingId = topping.Topping.Id;
                topping.Topping = null;
            }
        }

        _db.Orders.Attach(order);
        await _db.SaveChangesAsync();

        // In the background, send push notifications if possible
        var subscription = await _db.NotificationSubscriptions.Where(e => e.UserId == GetUserId()).SingleOrDefaultAsync();
        if (subscription != null)
        {
            _ = TrackAndSendNotificationsAsync(order, subscription);
        }

        return order.OrderId;
    }

    private string GetUserId()
    {
        return HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
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
        var webPushClient = new WebPushClient();
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                message,
                url = $"myorders/{order.OrderId}",
            });
            await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error sending push notification: " + ex.Message);
        }
    }
}
