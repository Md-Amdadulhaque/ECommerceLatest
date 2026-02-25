using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace E_commerce.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; } = string.Empty;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = null!;
        public string? Roles { get; set; } = null!;
    }
}
