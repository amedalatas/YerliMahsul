namespace Mahsul.Models
{
    public class Messages
    {
        public int Id { get; set; }
        public string SenderId { get; set; }
        public string ?SenderUserName { get; set; }

        public string ReceiverId { get; set; }
        public string ?ReceiverUsername { get; set; }

        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsRead { get; set; } // changed from bool? to bool
        public string? ProductName { get; set; }
        public string? ProductCategory { get; set; }
    }
}
