namespace ChatRoom.Models
{
    public class SendChatViewModel
    {
        public int GroupId { get; set; }
        public string Message { get; set; }
        public IFormFile AttachFile { get; set; }
        
    }
}
