using DataLayer.Entities;
using DataLayer.ViewModels;

namespace DataLayer.Interfaces;

public interface IGroupService
{
    ChatGroup GetChatGroupById(int groupId);
    ChatGroup GetChatGroupByToken(string token);
    IList<ChatGroup> GetChatGroups();
    IList<ChatGroup> GetChatGroupsOfUser(int userId);
    IList<string> GetUserIdsOfGroup(int groupId);
    ChatGroup JoinGroup(int userId, string groupToken);
    ChatGroup InsertGroup(ChatGroup chatGroup);
    ChatGroup UpdateGroup(ChatGroup chatGroup);
    void DeleteGroup(int groupId);
    IList<SearchResultViewModel> SearchGroup(string text,int userId);
    bool IsUserInGroup(int userId, int groupId);
    bool IsUserInGroup(int userId, string groupToken);
    ChatGroup GetPrivateChatGroup(int userId, int receiver);
}
