using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace E_commerce.Models
{
    public class AddToCart
    {
        public string UserId { get; set; }
        public string ProductId { get; set; }
    }
}
