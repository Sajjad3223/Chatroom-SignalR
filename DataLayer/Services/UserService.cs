using Dapper;
using DataLayer.Entities;
using DataLayer.Interfaces;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.Services
{
    public class UserService : IUserService
    {
        //private readonly MySqlConnection db;
        private readonly SqlConnection db;
        public UserService(IConfiguration configuration)
        {
            db = new SqlConnection(configuration.GetConnectionString("SqlConnection"));
        }

        public void DeleteUser(int id)
        {
            string query = "DELETE FROM Users WHERE user_id = @userId";
            db.Query(query, new {@userId = id });
        }

        public IList<User> GetAllUsers()
        {
            string query = "SELECT * FROM Users";
            return db.Query<User>(query).ToList();
        }

        public User GetUser(int id)
        {
            string query = "SELECT * FROM Users WHERE user_id = @userId";
            return db.Query<User>(query,new {@userId = id }).Single();
        }

        public User GetUser(string userName, string password)
        {
            string query = "SELECT * FROM Users WHERE username = @userName AND password = @password";
            return db.Query<User>(query, new { @userName = userName , @password = password}).SingleOrDefault();
        }

        public IList<User> GetUsersOfGroup(int groupId)
        {
            string query = "SELECT * FROM UserGroups JOIN Users USING (user_id) WHERE group_id = @groupId ";
            return db.Query<User>(query, new { @groupId = groupId }).ToList();
        }

        public User InsertUser(User user)
        {
            var query = "INSERT INTO Users (username,password) VALUES(@UserName,@Password);" +
                "SELECT SCOPE_IDENTITY();";
            var result = db.Query<int>(query, new 
            {
                @UserName = user.username,
                @Password = user.password
            }).Single();

            user.user_id = result;
            return user;
        }

        public User UpdateUser(User user)
        {
            var query = "UPDATE Users SET username = @UserName,password = @Password,user_avatar = @Avatar WHERE user_id = @userId";
            db.Query(query, new
            {
                @userId = user.user_id,
                @UserName = user.username,
                @Password = user.password,
                @Avatar = user.user_avatar
            });
            return user;
        }
    }

}

