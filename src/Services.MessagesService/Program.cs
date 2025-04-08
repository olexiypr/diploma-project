using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using EventBus;
using EventBus.RabbitMq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using RabbitMQ.Client;
using Services.MessagesService.EventBus.EventHandlers;
using Services.MessagesService.EventBus.Events;
using Services.MessagesService.Mappers;
using Services.MessagesService.Repositories;
using Services.MessagesService.Services;
using Services.MessagesService.ServiceWrappers.IdentityService.HttpClients;
using Services.MessagesService.Settings;
using Services.MessagesService.SignalR.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var awsCredentials = new BasicAWSCredentials(builder.Configuration["AWS:AccessKey"], builder.Configuration["AWS:SecretAccessKey"]);
var awsOptions = new AWSOptions
{
    Region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"]),
    Credentials = awsCredentials,
};
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonDynamoDB>();
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddTransient<IMessageMapper, MessageMapper>();
builder.Services.AddTransient<IMessagesRepository, DynamoDbMessagesRepository>();
builder.Services.Configure<DynamoDbSettings>(builder.Configuration.GetSection("DynamoDbSettings"));
builder.Services.Configure<NewMessageGenerationBackgroundJobSettings>(setting =>
{
    var minutesNumber = 15;
    if (!string.IsNullOrEmpty(builder.Configuration["GenerateNewMessageOffset"]))
    {
        minutesNumber = builder.Configuration.GetValue<int>("GenerateNewMessageOffset");
    }
    setting.GenerateNewMessageOffset = TimeSpan.FromMinutes(minutesNumber);
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Audience = builder.Configuration["AWS:AppClientId"];
        options.Authority = $"https://cognito-idp.{builder.Configuration["AWS:Region"]}.amazonaws.com/{builder.Configuration["AWS:UserPoolId"]}";
        options.MetadataAddress = $"https://cognito-idp.{builder.Configuration["AWS:Region"]}.amazonaws.com/{builder.Configuration["AWS:UserPoolId"]}/.well-known/openid-configuration";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true
        };
        /*options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/message-hub")))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };*/
    });

builder.Services.AddHttpClient<IdentityServiceHttpClient>(c => 
    c.BaseAddress = new Uri(builder.Configuration["IdentityService:BaseUrl"]!));

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


builder.Services.AddTransient<TextGenerationCompletedEventHandler>();
builder.Services.AddSignalR();


//TODO: Add checking when we run app if we should schedule job to generate new message
//TODO: handle case when app was off and quartz is memory storage has been reset
builder.Services.AddQuartz(configurator =>
{
    configurator.UseInMemoryStore();
});

builder.Services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
builder.Services.AddTransient<IBackgroundJobsSchedulerService, BackgroundJobsSchedulerService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowAll",
        opt =>
        {
            opt.AllowAnyOrigin();
            opt.AllowAnyHeader();
            opt.AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<MessagesHub>("message-hub");

var eventBus = app.Services.GetRequiredService<IEventBus>();
await eventBus.Subscribe<TextGenerationCompletedIntegrationEvent, TextGenerationCompletedEventHandler>();



app.Run();