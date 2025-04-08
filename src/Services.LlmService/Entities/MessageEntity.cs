using System.ComponentModel;
using Pgvector;

namespace Services.LlmService.Entities;

public class MessageEntity
{
    public int Id { get; set; }
    [Description("Message Id from MessageService")]
    public string MessageId { get; set; }
    public string TopicId { get; set; }
    public string Message { get; set; }
    public Vector Embedding { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateModified { get; set; }
}