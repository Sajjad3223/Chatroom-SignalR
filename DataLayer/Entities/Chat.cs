namespace DataLayer.Entities
{
    public class Chat
    {
        public int chat_id { get; set; }
        public int group_id { get; set; }
        public int sender_id { get; set; }
        public string chat_body { get; set; }
        public DateTime chat_date { get; set; }
        public string attach_file { get; set; }
    }
}
