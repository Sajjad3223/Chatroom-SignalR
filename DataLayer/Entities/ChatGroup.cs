namespace DataLayer.Entities
{
    public class ChatGroup
    {
        public int group_id { get; set; }
        public int owner_id { get; set; }
        public string group_title { get; set; }
        public string group_token { get; set; }
        public string group_image { get; set; }
        public int? receiver_id { get; set; } = null;
        public bool is_private { get; set; } = false;
    }
}
