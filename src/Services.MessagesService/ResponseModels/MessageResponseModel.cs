using Services.MessagesService.Entities;

namespace Services.MessagesService.ResponseModels;

public class MessageResponseModel
{
    public string Id { get; set; }
    public string TopicId { get; set; }
    public string Text { get; set; }
    public DateTime DateCreated { get; set; }
    public int CreatedBy { get; set; }
    public MessageCreators Creator { get; set; }
}