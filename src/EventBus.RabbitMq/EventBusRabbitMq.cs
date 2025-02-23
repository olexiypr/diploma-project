using System.Reflection;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus.RabbitMq;

public class EventBusRabbitMq : IEventBus, IDisposable, IAsyncDisposable
{
    private IChannel _channel;
    private IConnection _connection;
    private readonly ConnectionFactory _connectionFactory;
    private const string ExchangeName = "diploma-exchange";
    private readonly string _queueName;
    private readonly IServiceProvider _serviceProvider;
    public bool IsConnected => _channel != null && _connection.IsOpen;

    public EventBusRabbitMq(ConnectionFactory factory, IServiceProvider serviceProvider, string queueName)
    {
        _connectionFactory = factory;
        _queueName = queueName;
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> TryConnect()
    {
        try
        {
            _connection = await _connectionFactory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public IChannel GetChannel()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("RabbitMQ is not connected");
        }

        return _channel;
    }
    

    public void Dispose()
    {
        _connection.Dispose();
        _channel?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _channel.DisposeAsync();
    }

    public async Task Publish<TEvent>(TEvent @event) where TEvent : IntegrationEvent
    {
        if (_connection is not {IsOpen: true})
        {
            await TryConnect();
        }
        
        await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct);
        
        var eventName = @event.GetType().Name;
        var body = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType());
        await _channel.BasicPublishAsync(
            exchange: ExchangeName,
            routingKey: eventName,
            mandatory: true,
            body: body);
    }

    public async Task Subscribe<TEvent, TEventHandler>() where TEvent : IntegrationEvent where TEventHandler : IIntegrationEventHandler<TEvent>
    {
        await CreateConsumerChannel();
        await BindQueue(typeof(TEvent).Name);
        await StartConsume();
    }

    private async Task StartConsume()
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += ConsumerOnReceivedAsync;

        await _channel.BasicConsumeAsync(
            queue: _queueName,
            autoAck: false,
            consumer: consumer);
    }
    
    private async Task ConsumerOnReceivedAsync(object sender, BasicDeliverEventArgs @event)
    {
        if (@event.CancellationToken.IsCancellationRequested)
        {
            return;
        }
        var eventMessage = Encoding.UTF8.GetString(@event.Body.Span);
        await HandleEvent(@event.RoutingKey, eventMessage);
    }

    private async Task BindQueue(string eventName)
    {
        if (_connection is not {IsOpen: true})
        {
            await TryConnect();
        }
        
        await _channel.QueueBindAsync(queue: _queueName, exchange: ExchangeName, routingKey: eventName);
    }

    private async Task CreateConsumerChannel()
    {
        if (_connection is not {IsOpen: true})
        {
            await TryConnect();
        }

        try
        {
            await _channel.ExchangeDeclareAsync(ExchangeName, ExchangeType.Direct);
            await _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task HandleEvent(string eventName, string eventText)
    {
        var eventType = Assembly.GetEntryAssembly()?.GetTypes().FirstOrDefault(t => t.Name == eventName);
        var eventHandlerType = Assembly.GetEntryAssembly()?.GetTypes()
            .FirstOrDefault(t => t.GetInterfaces().Length > 0 && 
                                 t.GetInterfaces()
                                     .FirstOrDefault(i => i.IsGenericType &&
                                                          i.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>) &&
                                                          i.GetGenericArguments()[0] == eventType) != null);
        if (eventHandlerType is null)
        {
            return;
        }
        var handler = _serviceProvider.GetService(eventHandlerType);
        var method = eventHandlerType.GetMethod("Handle");
        if (method is null)
        {
            throw new NotImplementedException($"Event handler for event {eventName} does not exists");
        }
        
        var integrationEvent = JsonSerializer.Deserialize(eventText, eventType, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        await (Task)method.Invoke(handler, new []{integrationEvent});
    }
}