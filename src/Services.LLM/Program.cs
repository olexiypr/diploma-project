using EventBus;
using EventBus.RabbitMq;
using RabbitMQ.Client;
using Services.LLM.EventBus.EventHandlers;
using Services.LLM.EventBus.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddSingleton<IEventBus>(services =>
{
    var connectionFactory = new ConnectionFactory();
    if (!string.IsNullOrEmpty(builder.Configuration["RabbitMq:HostName"])) {
        connectionFactory.HostName = builder.Configuration["RabbitMq:HostName"];
    }

    if (!string.IsNullOrEmpty(builder.Configuration["RabbitMq:Port"]))
    {
        connectionFactory.Port = int.Parse(builder.Configuration["RabbitMq:Port"]);
    }
    return new EventBusRabbitMq(connectionFactory, services, builder.Configuration["RabbitMq:QueueName"]);
});

builder.Services.AddTransient<GenerateTextIntegrationEventHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var eventBus = app.Services.GetRequiredService<IEventBus>();
await eventBus.Subscribe<GenerateTextIntegrationEvent, GenerateTextIntegrationEventHandler>();

app.UseHttpsRedirection();

app.Run();