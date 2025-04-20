using EventBus;
using EventBus.RabbitMq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;
using Neo4j.Driver;
using RabbitMQ.Client;
using Services.LlmService;
using Services.LlmService.EventBus.EventHandlers;
using Services.LlmService.EventBus.Events;
using Services.LlmService.Neo4j;
using Services.LlmService.Services;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0070

var builder = Host.CreateDefaultBuilder(args);

var env = Environment.GetEnvironmentVariable("APP_ENVIRONMENT");
Console.WriteLine("Run Services.IdentityService.LlmService with Environment - " + env);

builder.ConfigureAppConfiguration(configurationBuilder =>
{
    configurationBuilder.SetBasePath(Environment.CurrentDirectory)
        .AddJsonFile($"appsettings.{env}.json", false, true)
        .AddJsonFile("appsettings.json", false, true)
        .AddEnvironmentVariables()
        .Build();
});

builder.ConfigureServices((context, services) =>
{
    services.AddSingleton<IEventBus>(serviceProvider =>
    {
        var connectionFactory = new ConnectionFactory();
        if (!string.IsNullOrEmpty(context.Configuration["RabbitMq:HostName"]))
        {
            connectionFactory.HostName = context.Configuration["RabbitMq:HostName"]!;
        }

        if (!string.IsNullOrEmpty(context.Configuration["RabbitMq:Port"]))
        {
            connectionFactory.Port = int.Parse(context.Configuration["RabbitMq:Port"]!);
        }

        return new EventBusRabbitMq(connectionFactory, serviceProvider, context.Configuration["RabbitMq:QueueName"]!);
    });

    services.AddTransient<ITextSplitter, RecursiveTextSplitter>();
    services.AddTransient<ICypherQueryGenerator, CypherQueryGenerator>();
    services.AddTransient<IKeywordExtractorService, KeywordExtractorService>();
    services.AddTransient<IEmbeddingGenerationService, EmbeddingGenerationService>();
    
    services.AddTransient<GenerateTextIntegrationEventHandler>();
    services.AddTransient<AddNewMessageIntegrationEventHandler>();
    services.AddTransient<CreateNewTopicIntegrationEventHandler>();
    
    services.AddDbContext<LlmServiceDbContext>(optionsBuilder =>
    {
        var host = context.Configuration["Database:Host"];
        var port = context.Configuration["Database:Port"];
        var password = context.Configuration["Database:Password"];
        var username = context.Configuration["Database:Username"];
        var databaseName = context.Configuration["Database:DatabaseName"];
        var connectionString = $"Host={host}:{port};Username={username};Password={password};Database={databaseName}";
        optionsBuilder.UseNpgsql(connectionString, options => options.UseVector());
    });

    

    services.AddSingleton<IDriver>(_ =>
    {
        var user = context.Configuration["NEO4j:NEO4J_USER"];
        var password = context.Configuration["NEO4j:NEO4J_PASSWORD"];
        var url = context.Configuration["NEO4j:NEO4J_URL"];
        var token = AuthTokens.Basic(user, password);
        return GraphDatabase.Driver(url, token);
    });

    var llmHost = new Uri(context.Configuration["Llm:Host"] ?? throw new ArgumentException());
    
    services.AddKernel()
        .AddOllamaChatCompletion(modelId: "gemma3:1b", endpoint: llmHost)
        .AddOllamaTextEmbeddingGeneration(modelId: "nomic-embed-text", endpoint: llmHost);
});

var host = builder.Build();

var eventBus = host.Services.GetRequiredService<IEventBus>();
await eventBus.Subscribe<GenerateTextIntegrationEvent, GenerateTextIntegrationEventHandler>();
await eventBus.Subscribe<AddNewMessageIntegrationEvent, AddNewMessageIntegrationEventHandler>();
await eventBus.Subscribe<CreateNewTopicIntegrationEvent, CreateNewTopicIntegrationEventHandler>();

var scope = host.Services.CreateScope();

var createVectorIndex = async () =>
{
    var driver = scope.ServiceProvider.GetRequiredService<IDriver>();
    var cypherTextGenerator = scope.ServiceProvider.GetRequiredService<ICypherQueryGenerator>();
    await Neo4jHelpers.CreateVectorIndex(driver, cypherTextGenerator);
};

await createVectorIndex.Invoke();


var handler = scope.ServiceProvider.GetRequiredService<CreateNewTopicIntegrationEventHandler>();
var createNewTopic = new CreateNewTopicIntegrationEvent
{
    TopicId = "1",
    TopicName = "Ukrainian Cossacks",
    Description = "This story unfolds in the late 16th to early 18th century, a time when the Ukrainian Cossacks defended their homeland against foreign invaders and internal threats. The setting is the vast steppes of what is now central and eastern Ukraine, with frequent mentions of the Dnipro River, frontier villages, and fortified camps known as \"sich.\" The main idea revolves around a group of Cossacks—free warriors and defenders of the Ukrainian people—who fight to protect their land, freedom, and honor. Their enemies include Tatar raiders, Polish nobles, and later, Ottoman and Muscovite forces, depending on the direction of the narrative. The Cossacks are portrayed as courageous, loyal, and deeply tied to the land and the people they protect.",
    AdditionalTopicDescription = "Players contribute to a continuous story, each message adding new twists, characters, or events while preserving historical mood and logical progression. The tone of the story should feel dramatic, grounded in history, with moments of heroism, sacrifice, and brotherhood. Messages can involve battles, rescues, strategy meetings, travels through the steppes, or even moments of reflection around the campfire. The story should respect the historical and cultural essence of the Cossack spirit while leaving room for creative storytelling."
};
//await handler.Handle(createNewTopic);


var generateTextIntegrationEventHandler = scope.ServiceProvider.GetRequiredService<GenerateTextIntegrationEventHandler>();

#pragma warning disable SKEXP0010
/*await generateTextIntegrationEventHandler.Handle(new GenerateTextIntegrationEvent
{
    LastMessageText = createNewTopic.Description,
    Description = createNewTopic.Description,
    AdditionalTopicDescription = createNewTopic.AdditionalTopicDescription
});*/
#pragma warning restore SKEXP0010

var textSplitter = scope.ServiceProvider.GetRequiredService<ITextSplitter>();

var textToTest =
    "The sun dipped below the Dnipro River as Hetman Ostap surveyed the horizon, his saber resting against his hip. Smoke curled in the distance—a sign that Tatar raiders had set another village ablaze. He turned to his brothers-in-arms, their faces hardened by years of war, and raised his fist. \"Tonight, we ride to defend our land, or die trying!";




host.Run();