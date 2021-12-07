using DataLayer.Entities;
using DataLayer.ViewModels;

namespace DataLayer.Interfaces;

public interface IChatService
{
    IList<ChatViewModel> GetChatsOfGroup(int groupId);
    IList<ChatViewModel> GetChatsOfGroup(string groupToken);
    Chat GetChat(int chatId);
    Chat SendChat(Chat chat);
    Chat EditChat(Chat chat);
    void DeleteChat(int chatId);
}