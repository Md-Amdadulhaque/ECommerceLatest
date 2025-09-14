using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace E_commerce.Models;

[BsonIgnoreExtraElements]
public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ?Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int? Price { get; set; } = 0;

    public string? ImageData { get; set; } = null!;
    public string? ParentId { get; set; }= null!;

}