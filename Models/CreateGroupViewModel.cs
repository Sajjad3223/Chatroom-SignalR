namespace ChatRoom.Models
{
    public class CreateGroupViewModel
    {
        public int OwnerId { get; set; }
        public string GroupName { get; set; }
        public IFormFile GroupImage { get; set; }
    }
}
