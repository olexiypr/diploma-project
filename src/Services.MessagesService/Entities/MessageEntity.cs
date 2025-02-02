using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DataModel;

namespace Services.MessagesService.Entities;

public class MessageEntity
{
    [JsonPropertyName("id")]
    [DynamoDBRangeKey]
    public string Id { get; set; }
    public int CreatedBy { get; set; }
    public string Text { get; set; }
    public DateTime DateCreated { get; } = DateTime.UtcNow;
    public MessageCreators MessageCreator { get; set; }
    [JsonPropertyName("topicId")]
    [DynamoDBHashKey]
    public int TopicId {get; set;}
}