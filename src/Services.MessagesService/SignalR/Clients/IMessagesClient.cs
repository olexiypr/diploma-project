using Services.MessagesService.ResponseModels;

namespace Services.MessagesService.SignalR.Clients;

public interface IMessagesClient
{
    Task ReceiveMessage(MessageResponseModel responseModel);
}