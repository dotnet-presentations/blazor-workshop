using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazingPizza.Server
{
    [Route("notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly PizzaStoreContext _db;

        public NotificationsController(PizzaStoreContext db)
        {
            _db = db;
        }

        [HttpPut("subscribe")]
        public async Task<NotificationSubscription> Subscribe(NotificationSubscription subscription)
        {
            // We're storing at most one subscription per user, so delete old ones.
            // Alternatively, you could let the user register multiple subscriptions from different browsers/devices.
            var userId = GetUserId();
            var oldSubscriptions = _db.NotificationSubscriptions.Where(e => e.UserId == userId);
            _db.NotificationSubscriptions.RemoveRange(oldSubscriptions);

            // Store new subscription
            subscription.UserId = userId;
            _db.NotificationSubscriptions.Attach(subscription);

            await _db.SaveChangesAsync();
            return subscription;
        }

        private string GetUserId()
        {
            // This will be the user's twitter username
            return HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value;
        }
    }
}
