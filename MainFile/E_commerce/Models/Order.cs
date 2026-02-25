namespace E_commerce.Models
{
    public class Order
    {
        public string Id { get; set; }
        public required string UserId { get; set; }
        public required string CartId { get; set; }
    }
}
