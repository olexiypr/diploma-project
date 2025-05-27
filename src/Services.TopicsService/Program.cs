using Services.Topics;
using Services.Topics.Services;
using Services.Topics.ServiceWrappers.IdentityService.HttpClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
Console.WriteLine("Run Services.TopicsService with Environment - " + env);
builder.Configuration.AddJsonFile($"appsettings.{env}.json", false, true).AddEnvironmentVariables();

builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.AddTransient<ITopicsService, TopicsService>(); 
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddControllers();
builder.Services.AddHttpClient<IdentityServiceHttpClient>(c => 
    c.BaseAddress = new Uri(builder.Configuration["IdentityService:BaseUrl"]!));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminAccess", policy => policy.RequireAssertion(async context =>
    {
        /*var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(builder.Configuration["IdentityService:BaseUrl"]!);
        var identityServiceHttpClient = new IdentityServiceHttpClient(httpClient);
        return await identityServiceHttpClient.IsUserAdmin(context.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")!.Value);*/
        return true;
    }));
});

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

/*var mongoDbService = app.Services.GetRequiredService<MongoDbService>();
await mongoDbService.SeedData();*/



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();