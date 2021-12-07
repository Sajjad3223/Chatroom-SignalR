using Dapper;
using DataLayer.Entities;
using DataLayer.Interfaces;
using DataLayer.ViewModels;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace DataLayer.Services
{
    public class ChatService : IChatService
    {
        //private readonly MySqlConnection db;
        private readonly SqlConnection db;
        private readonly IGroupService _groupService;
        public ChatService(IConfiguration configuration, IGroupService groupService)
        {
            //db = new MySqlConnection(configuration.GetConnectionString("MySqlConnection"));
            db = new SqlConnection(configuration.GetConnectionString("SqlConnection"));
            _groupService = groupService;
        }

        public void DeleteChat(int chatId)
        {
            string query = "DELETE FROM Chats WHERE chat_id = @ChatId";
            db.Query(query, new { @ChatId = chatId });
        }

        public Chat EditChat(Chat chat)
        {
            var query = "Update Chats SET chat_body = @ChatBody WHERE chat_id = @ChatId;";
            return db.Query<Chat>(query, new
            {
                @ChatBody = chat.chat_body,
                @ChatId = chat.chat_id
            }).Single();
        }

        public Chat GetChat(int chatId)
        {
            string query = "SELECT * FROM Chats WHERE chat_id = @ChatId";
            return db.Query<Chat>(query, new { @ChatId = chatId }).Single();
        }

        public IList<ChatViewModel> GetChatsOfGroup(int groupId)
        {
            string query = "SELECT chat_body,username,sender_id,user_avatar,chat_date,attach_file FROM Chats c JOIN Users u ON c.sender_id = u.user_id WHERE group_id = @groupId ORDER BY chat_date";
            var data = db.Query(query, new { @groupId = groupId }).ToList();
            List<ChatViewModel> chats = new List<ChatViewModel>();
            if(data.Any())
            {
                foreach (var c in data)
                {
                    chats.Add(new ChatViewModel
                    {
                        sender_id = c.sender_id,
                        chat_body = c.chat_body,
                        chat_date = $"{c.chat_date.Hour.ToString("00")}:{c.chat_date.Minute.ToString("00")}",
                        username = c.username,
                        user_avatar = c.user_avatar,
                        group_id = groupId,
                        attach_file = c.attach_file
                    });
                }
            }
            return chats;
        }

        public IList<ChatViewModel> GetChatsOfGroup(string groupToken)
        {
            var group = _groupService.GetChatGroupByToken(groupToken);
            return GetChatsOfGroup(group.group_id);
        }

        public Chat SendChat(Chat chat)
        {
            var query = "INSERT INTO Chats (sender_id,group_id,chat_body,chat_date,attach_file) " +
                "VALUES(@SenderId,@GroupId,@ChatBody,@ChatDate,@AttachFile);" +
                "SELECT SCOPE_IDENTITY();";
            var result = db.Query<int>(query, new
            {
                @SenderId = chat.sender_id,
                @GroupId = chat.group_id,
                @ChatBody = chat.chat_body,
                @ChatDate = DateTime.Now,
                @AttachFile = chat.attach_file
            }).Single();

            chat.chat_id = result;

            return chat;
        }
    }

}

