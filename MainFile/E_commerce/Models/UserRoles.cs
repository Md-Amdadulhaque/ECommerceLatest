using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace E_commerce.Models
{
    public class UserRoles
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }
        public string UserId { get; set; }
      
        public string RoleId { get; set; }
    }
}
