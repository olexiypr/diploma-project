using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Diploma1.IdentityService;
using Diploma1.IdentityService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Console.WriteLine("Run Services.IdentityService.Cognito with Environment - " + env);
builder.Configuration.AddJsonFile($"appsettings.{env}.json", false, true).AddEnvironmentVariables();

builder.Services.Configure<AwsSettings>(builder.Configuration.GetSection("AWS"));
var awsCredentials = new BasicAWSCredentials(builder.Configuration["AWS:AccessKey"], builder.Configuration["AWS:SecretAccessKey"]);
var awsOptions = new AWSOptions
{
    Region = RegionEndpoint.GetBySystemName(builder.Configuration["AWS:Region"]),
    Credentials = awsCredentials,
};
builder.Services.AddDefaultAWSOptions(awsOptions);
builder.Services.AddAWSService<IAmazonCognitoIdentityProvider>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<IdentityServiceDbContext>(optionsBuilder =>
{
    var host = builder.Configuration["Database:Host"];
    var port = builder.Configuration["Database:Port"];
    var password = builder.Configuration["Database:Password"];
    var username = builder.Configuration["Database:Username"];
    var databaseName = builder.Configuration["Database:DatabaseName"];
    var connectionString = $"Host={host}:{port};Username={username};Password={password};Database={databaseName}";
    optionsBuilder.UseNpgsql(connectionString);
});

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IAuthService, AuthService>();


var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();