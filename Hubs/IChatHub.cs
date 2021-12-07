namespace ChatRoom.Hubs
{
    public interface IChatHub
    {
        Task JoinGroup(string token, int currentGroupId);
        Task JoinPrivateGroup(string receiverId, int currentGroupId);
    }
}
