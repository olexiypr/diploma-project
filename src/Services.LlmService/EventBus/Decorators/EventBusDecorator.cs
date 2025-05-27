using EventBus.RabbitMq;
using RabbitMQ.Client;

namespace Services.LlmService.EventBus.Decorators;

public class EventBusDecorator : EventBusRabbitMq
{
    public List<Task> TasksToDo = new List<Task>();
    public EventBusDecorator(ConnectionFactory factory, IServiceProvider serviceProvider, string queueName) : base(factory, serviceProvider, queueName)
    {
    }

    protected override async Task HandleEvent(string eventName, string eventText)
    {
        var task = base.HandleEvent(eventName, eventText);
        TasksToDo.Add(task);
        await task.WaitAsync(CancellationToken.None);
        
    }
}