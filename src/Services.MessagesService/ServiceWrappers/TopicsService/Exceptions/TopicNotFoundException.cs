namespace Services.MessagesService.ServiceWrappers.TopicsService.Exceptions;

public class TopicNotFoundException(string topicId) : Exception($"Topic {topicId} was not found");