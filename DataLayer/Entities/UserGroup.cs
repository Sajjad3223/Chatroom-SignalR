namespace DataLayer.Entities
{
    public class UserGroup
    {
        public int usergroup_id { get; set; }
        public int user_id { get; set; }
        public int group_id { get; set; }
        public DateTime join_date { get; set; }
    }
}
