namespace Services.Topics.RequestModels;

public class CreateTopicRequestModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string AdditionalDescription { get; set; }
}