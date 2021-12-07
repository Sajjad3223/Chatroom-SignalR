using ChatRoom.Hubs;
using ChatRoom.Models;
using ChatRoom.utils;
using DataLayer.Entities;
using DataLayer.Interfaces;
using DataLayer.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace ChatRoom.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IUserService _userService;
        private readonly IGroupService _groupService;
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _chatHub;

        public HomeController(IUserService userService, IGroupService groupService, IHubContext<ChatHub> chatHub, IChatService chatService)
        {
            _userService = userService;
            _groupService = groupService;
            _chatHub = chatHub;
            _chatService = chatService;
        }

        public IActionResult Index()
        {
            var groups = _groupService.GetChatGroupsOfUser(User.GetUserId());
            if(groups.Any())
                for (int i = 0; i < groups.Count; i++)
                {
                    groups[i] = FixGroup(groups[i]);
                }

            return View(groups);
        }

        [HttpPost]
        public async Task CreateGroup([FromForm]CreateGroupViewModel group)
        {
            try
            {
                string image_name = String.Empty;
                if (group.GroupImage != null)
                {
                    image_name = SaveFile(group.GroupImage,"wwwroot/images/");
                }

                var addedGroup = _groupService.InsertGroup(new ChatGroup()
                {
                    group_title = group.GroupName,
                    group_image = image_name,
                    owner_id = User.GetUserId()
                });

                if (addedGroup != null)
                    await _chatHub.Clients.User(User.GetUserId().ToString()).SendAsync("GroupCreated", addedGroup.group_title, addedGroup.group_token, addedGroup.group_image);
                else
                    await _chatHub.Clients.User(User.GetUserId().ToString()).SendAsync("ERROR");
            }
            catch (Exception ex)
            {
                await _chatHub.Clients.User(User.GetUserId().ToString()).SendAsync("ERROR");
            }
        }

        private string SaveFile(IFormFile file, string path)
        {
            string file_name = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string save_path = Path.Combine(Directory.GetCurrentDirectory(), path, file_name);
            using FileStream stream = new FileStream(save_path, FileMode.Create);
            file.CopyTo(stream);
            return file_name;
        }

        [HttpPost]
        public async Task SendMessage([FromForm] SendChatViewModel sendChatViewModel)
        {
            try
            {
                var group = _groupService.GetChatGroupById(sendChatViewModel.GroupId);
                if (group == null)
                    return;
                var chat = new Chat
                {
                    chat_body = sendChatViewModel.Message,
                    group_id = group.group_id,
                    sender_id = User.GetUserId()
                };
                if(sendChatViewModel.AttachFile != null)
                {
                    chat.chat_body = sendChatViewModel.AttachFile.FileName;
                    string fileName = SaveFile(sendChatViewModel.AttachFile, "wwwroot/files/");
                    chat.attach_file = fileName;
                }

                chat = _chatService.SendChat(chat);
                var user = _userService.GetUser(User.GetUserId());

                ChatViewModel chatViewModel = new ChatViewModel
                {
                    chat_body = chat.chat_body,
                    sender_id = User.GetUserId(),
                    chat_date = $"{DateTime.Now.Hour.ToString("00")}:{DateTime.Now.Minute.ToString("00")}",
                    username = user.username,
                    user_avatar = user.user_avatar,
                    group_id = group.group_id,
                    groupName = group.group_title,
                    attach_file = chat.attach_file
                };

                List<string> userIds = _groupService.GetUserIdsOfGroup(group.group_id).ToList();
                await _chatHub.Clients.Users(userIds).SendAsync("ReceiveNotification", chatViewModel);

                await _chatHub.Clients.Group(group.group_id.ToString()).SendAsync("ReceiveMessage", chatViewModel);
            }
            catch (Exception ex)
            {
                await _chatHub.Clients.User(User.GetUserId().ToString()).SendAsync("Error",ex.Message);
            }
        }

        public async Task<IActionResult> Search(string text)
        {
            return new ObjectResult(_groupService.SearchGroup(text,User.GetUserId()));
        }

        [HttpPost]
        public IActionResult SetUserAvatar([FromForm] IFormFile image)
        {
            if (image == null)
                return BadRequest();
            var user = _userService.GetUser(User.GetUserId());
            var imageName = SaveFile(image, "wwwroot/images/users");
            user.user_avatar = imageName;

            _userService.UpdateUser(user);
            return ViewComponent("UserInformations");
        }

        private ChatGroup FixGroup(ChatGroup group)
        {
            if (group.is_private)
            {
                var user = _userService.GetUser(group.owner_id);
                if (group.owner_id == User.GetUserId() && group.receiver_id != null)
                {
                    var receiver = _userService.GetUser(group.receiver_id.Value);
                    group.group_title = receiver.username;
                    group.group_image = receiver.user_avatar;
                    return group;
                }
                else
                {
                    group.group_title = user.username;
                    group.group_image = user.user_avatar;
                    return group;
                }
            }
            return group;
        }
    }
}