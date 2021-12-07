using DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ChatRoom.utils;
using System.Security.Claims;

namespace ChatRoom.ViewComponents
{
    public class UserInformationsViewComponent : ViewComponent
    {
        private readonly IUserService _userService;

        public UserInformationsViewComponent(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = _userService.GetUser((User as ClaimsPrincipal).GetUserId());
            return View(user);
        }
    }
}
