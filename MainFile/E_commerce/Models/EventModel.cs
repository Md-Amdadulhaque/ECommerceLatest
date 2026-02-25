namespace E_commerce.Models
{
    public class EventModel : User
    {   public Command command { get; set; }

        public string queueName { get; set; }
        public DateTime dateTime { get; set; }

        public string message { get; set; }
    }
}
