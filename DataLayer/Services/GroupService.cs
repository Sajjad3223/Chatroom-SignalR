using Dapper;
using DataLayer.Entities;
using DataLayer.Interfaces;
using DataLayer.ViewModels;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;

namespace DataLayer.Services
{
    public class GroupService : IGroupService
    {
        //private readonly MySqlConnection db;
        private readonly SqlConnection db;
        public GroupService(IConfiguration configuration)
        {
           // db = new MySqlConnection(configuration.GetConnectionString("MySqlConnection"));
            db = new SqlConnection(configuration.GetConnectionString("SqlConnection"));
        }

        public void DeleteGroup(int groupId)
        {
            string query = "DELETE FROM ChatGroups WHERE group_id = @groupId";
            db.Query(query, new { @groupId = groupId });
        }

        public ChatGroup GetChatGroupById(int groupId)
        {
            string query = "SELECT * FROM ChatGroups WHERE group_id = @groupId";
            return db.Query<ChatGroup>(query, new { @groupId = groupId }).Single();
        }

        public ChatGroup GetChatGroupByToken(string token)
        {
            string query = "SELECT * FROM ChatGroups WHERE group_token = @token";
            return db.Query<ChatGroup>(query, new { @token = token }).Single();
        }

        public IList<ChatGroup> GetChatGroups()
        {
            string query = "SELECT * FROM ChatGroups";
            return db.Query<ChatGroup>(query).ToList();
        }

        public IList<ChatGroup> GetChatGroupsOfUser(int userId)
        {
            //string query = "SELECT * FROM UserGroups JOIN ChatGroups USING (group_id) WHERE user_id = @userId ORDER BY join_date DESC";
            string query = "SELECT * FROM UserGroups u JOIN ChatGroups c ON u.group_id = c.group_id WHERE user_id = @userId ORDER BY join_date DESC";
            return db.Query<ChatGroup>(query, new { @userId = userId }).ToList();
        }

        public ChatGroup GetPrivateChatGroup(int userId, int receiver)
        {
            string query = "SELECT * FROM ChatGroups WHERE (owner_id = @userId AND receiver_id = @receiverId) OR (owner_id = @receiverId AND receiver_id = @userId)";
            return db.Query<ChatGroup>(query, new { @userId = userId, @receiverId = receiver }).SingleOrDefault();
        }

        public IList<string> GetUserIdsOfGroup(int groupId)
        {
            string query = "SELECT * FROM UserGroups WHERE group_id = @groupId";
            return db.Query<UserGroup>(query, new { @groupId = groupId }).Select(g=>g.user_id.ToString()).ToList();
        }

        public ChatGroup InsertGroup(ChatGroup chatGroup)
        {
            string token = Guid.NewGuid().ToString();
            var createGroup = "INSERT INTO ChatGroups (owner_id,group_title,group_token,group_image,receiver_id,is_private) " +
                "VALUES(@OwnerId,@GroupTitle,@GroupToken,@GroupImage,@ReceiverId,@IsPrivate);" +
                "SELECT SCOPE_IDENTITY();";
            var result = db.Query<int>(createGroup, new
            {
                @OwnerId = chatGroup.owner_id,
                @GroupTitle = chatGroup.group_title,
                @GroupToken = token,
                @GroupImage = chatGroup.group_image,
                @ReceiverId = chatGroup.receiver_id,
                @IsPrivate = chatGroup.is_private
            }).Single();
            chatGroup.group_id = result;
            chatGroup.group_token = token;

            JoinGroup(chatGroup.owner_id, chatGroup.group_token);

            return chatGroup;
        }

        public bool IsUserInGroup(int userId, int groupId)
        {
            string query = "SELECT * FROM UserGroups WHERE user_id = @userId AND group_id = @groupId";
            return db.Query<ChatGroup>(query, new { @userId = userId , @groupId = groupId }).Any();
        }

        public bool IsUserInGroup(int userId, string groupToken)
        {
            var group = GetChatGroupByToken(groupToken);
            if (group == null)
                return false;
            return IsUserInGroup(userId, group.group_id);
        }

        public ChatGroup JoinGroup(int userId , string groupToken)
        {
            try
            {
                ChatGroup group = GetChatGroupByToken(groupToken);
                var joinUser = "INSERT INTO UserGroups (user_id,group_id,join_date) VALUES(@UserId,@GroupId,@JoinDate);";

                db.Query(joinUser, new
                {
                    @UserId = userId,
                    @GroupId = group.group_id,
                    @JoinDate = DateTime.Now
                });

                return group;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public IList<SearchResultViewModel> SearchGroup(string text, int userId)
        {
            var result = new List<SearchResultViewModel>();
            string query = "SELECT * FROM ChatGroups WHERE group_title LIKE @text ";
            var groups = db.Query<ChatGroup>(query, new { text = "%" + text + "%" }).Select(g => new SearchResultViewModel()
            {
                Title = g.group_title,
                Token = g.group_token,
                ImageName = g.group_image,
                IsUser = false
            }).ToList();

            string query2 = "SELECT * FROM Users WHERE user_id != @userId AND username LIKE @text ";
            var users = db.Query<User>(query2, new { text = "%" + text + "%" , @userId = userId}).Select(u => new SearchResultViewModel()
            {
                Title = u.username,
                Token = u.user_id.ToString(),
                ImageName = u.user_avatar,
                IsUser = true
            }).ToList();

            result.AddRange(groups);
            result.AddRange(users);

            return result;
        }

        public ChatGroup UpdateGroup(ChatGroup chatGroup)
        {
            var query = "Update ChatGroups SET group_title = @GroupTitle , group_image = @GroupImage WHERE group_id = @GroupId;";
            return db.Query<ChatGroup>(query, new
            {
                @GroupTitle = chatGroup.group_title,
                @GroupImage = chatGroup.group_image,
                @GroupId = chatGroup.group_id
            }).Single();
        }
    }

}

