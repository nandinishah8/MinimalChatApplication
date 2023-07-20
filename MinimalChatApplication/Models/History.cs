namespace MinimalChatApplication.Models
{
    public class History
    {
        public int Id { get; set; }
        public int userId { get; set; }
        public DateTime before { get; set; }
        public int count { get; set; }
        public string sort { get; set; }
    }
}
