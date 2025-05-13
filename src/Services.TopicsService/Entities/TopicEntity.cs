using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Services.Topics.Entities;

public class TopicEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public string Name { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public string Description { get; set; }
    
    [BsonRepresentation(BsonType.String)]
    public string AdditionalDescription { get; set; }
    
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime DateCreated { get; set; }
    
    [BsonRepresentation(BsonType.DateTime)]
    public DateTime DateModified { get; set; }
}