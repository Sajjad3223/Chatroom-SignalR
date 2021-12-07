using ChatRoom.Models;
using ChatRoom.utils;
using DataLayer.Entities;
using DataLayer.Interfaces;
using DataLayer.ViewModels;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatRoom.Hubs
{
    public class ChatHub : Hub,IChatHub
    {
        private readonly IGroupService _groupService;
        private readonly IChatService _chatservice;
        private readonly IUserService _userService;

        public ChatHub(IGroupService groupService, IChatService chatservice, IUserService userService)
        {
            _groupService = groupService;
            _chatservice = chatservice;
            _userService = userService;
        }

        public override Task OnConnectedAsync()
        {
            Clients.Caller.SendAsync("Setup",Context.User.GetUserId());
            return base.OnConnectedAsync();
        }

        public async Task JoinGroup(string token,int currentGroupId)
        {
            try
            {
                var userId = Context.User.GetUserId();
                var group = _groupService.GetChatGroupByToken(token);
                if (group == null)
                {
                    await Clients.Caller.SendAsync("Error", "Group not found!!!");
                    return;
                }

                if (!_groupService.IsUserInGroup(userId, token))
                {
                    _groupService.JoinGroup(userId, token);
                    await Clients.Caller.SendAsync("GroupCreated", group.group_title, group.group_token, group.group_image);
                }

                if (currentGroupId > 0)
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentGroupId.ToString());

                await Groups.AddToGroupAsync(Context.ConnectionId, group.group_id.ToString());
                group = FixGroup(group);
                var groupChats = _chatservice.GetChatsOfGroup(group.group_id);
                await Clients.Caller.SendAsync("JoinedGroup", group, groupChats);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Group not found!!!");
            }
        }

        public async Task JoinPrivateGroup(string receiverId, int currentGroupId)
        {
            try
            {
                var receiver = _userService.GetUser(int.Parse(receiverId));
                var user= _userService.GetUser(Context.User.GetUserId());
                var group = _groupService.GetPrivateChatGroup(user.user_id, receiver.user_id);
                if (group == null)
                {
                    group = _groupService.InsertGroup(new ChatGroup
                    {
                        group_title = Context.User.GetUserName(),
                        is_private = true,
                        owner_id = user.user_id,
                        receiver_id = receiver.user_id,
                        group_image = user.user_avatar
                    });
                }
                if (!_groupService.IsUserInGroup(user.user_id, group.group_token))
                {
                    _groupService.JoinGroup(user.user_id, group.group_token);
                    await Clients.Caller.SendAsync("GroupCreated", receiver.username, group.group_token, receiver.user_avatar);
                }
                if (!_groupService.IsUserInGroup(receiver.user_id, group.group_token))
                {
                    _groupService.JoinGroup(receiver.user_id, group.group_token);
                    await Clients.User(receiverId).SendAsync("GroupCreated", user.username, group.group_token, user.user_avatar);
                }

                if (currentGroupId > 0)
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, currentGroupId.ToString());

                await Groups.AddToGroupAsync(Context.ConnectionId, group.group_id.ToString());
                group = FixGroup(group);
                var groupChats = _chatservice.GetChatsOfGroup(group.group_id);
                await Clients.Caller.SendAsync("JoinedGroup", group, groupChats);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Group not found!!!");
            }

        }

        private ChatGroup FixGroup(ChatGroup group)
        {
            if(group.is_private)
            {
                var user = _userService.GetUser(group.owner_id);
                if (group.owner_id == Context.User.GetUserId() && group.receiver_id != null)
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
