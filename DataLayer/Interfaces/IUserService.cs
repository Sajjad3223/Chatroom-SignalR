using DataLayer.Entities;

namespace DataLayer.Interfaces;

public interface IUserService
{
    User GetUser(int id);
    User GetUser(string userName, string password);
    IList<User> GetAllUsers();

    IList<User> GetUsersOfGroup(int groupId);

    User InsertUser(User user);
    User UpdateUser(User user);
    void DeleteUser(int id);
}
