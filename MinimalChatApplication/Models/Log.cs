namespace MinimalChatApplication.Models
{
    public class Log
    {
        public int Id { get; set; }
        public string IpAddress { get; set; }
        public string RequestBody { get; set; }
        public string TimeStamp { get; set; }
        public string Username  { get; set;}
    }
}
