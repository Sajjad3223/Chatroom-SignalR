using DataLayer.Entities;

namespace DataLayer.ViewModels
{
    public class ChatViewModel
    {
        public string chat_body { get; set; }
        public string username { get; set; }
        public string groupName { get; set; }
        public int sender_id { get; set; }
        public int group_id { get; set; }
        public string user_avatar { get; set; }
        public string chat_date { get; set; }
        public string attach_file { get; set; }
    }
}
