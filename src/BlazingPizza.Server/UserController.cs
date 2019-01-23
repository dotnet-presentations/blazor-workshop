using BlazingPizza.ComponentsLibrary.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BlazingPizza.Server
{
    [ApiController]
    public class UserController : Controller
    {
        private static UserState LoggedOutState = new UserState { IsLoggedIn = false };

        [HttpGet("user")]
        public UserState GetUser()
        {
            return User.Identity.IsAuthenticated
                ? new UserState { IsLoggedIn = true, DisplayName = User.Identity.Name }
                : LoggedOutState;
        }

        [HttpGet("user/signin")]
        public async Task SignIn()
        {
            await HttpContext.ChallengeAsync(
                TwitterDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = "/user/signincompleted" });
        }

        [Authorize]
        [HttpGet("user/signincompleted")]
        public IActionResult SignInCompleted()
        {
            var userState = GetUser();
            return Content($@"
                <script>
                    window.opener.onLoginPopupFinished({JsonConvert.SerializeObject(userState)});
                    window.close();
                </script>", "text/html");
        }

        [HttpPut("user/signout")]
        public async Task<UserState> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LoggedOutState;
        }
    }
}
