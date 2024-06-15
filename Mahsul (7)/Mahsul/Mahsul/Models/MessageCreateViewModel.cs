namespace Mahsul.Models
{
    public class MessageCreateViewModel
    {
        public string ?SenderId { get; set; }

        public string ?ReceiverId { get; set; }
        public string ?Content { get; set; }
        public string ?SenderUserName {  get; set; }
        public string ?ReceiverUsername { get; set; }
        public string ?ProductName { get; set; }
        public string ?ProductCategory { get; set; }

        public DateTime Timestamp { get; set; }
    }

}
