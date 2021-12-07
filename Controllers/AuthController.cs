using ChatRoom.Models;
using DataLayer.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatRoom.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel register)
        {
            if(!ModelState.IsValid)
                return View("Index",register);

            var user = _userService.InsertUser(new DataLayer.Entities.User
            {
                username = register.UserName,
                password = register.Password
            });

            return Redirect("/auth#login");
        }

        public IActionResult Login()
        {
            return Redirect("/auth#login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
                return View("/auth#login", login);

            var user = _userService.GetUser(login.UserName, login.Password);

            if(user == null)
                return View("/auth#login", login);

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier,user.user_id.ToString()),
                new Claim(ClaimTypes.Name,user.username.ToString()),
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(principal, new AuthenticationProperties() { IsPersistent = true });

            return Redirect("/");
        }
    }
}
