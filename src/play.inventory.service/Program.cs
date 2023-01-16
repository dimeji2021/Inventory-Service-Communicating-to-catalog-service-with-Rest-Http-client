using play.common.MongoDB;
using play.inventory.service.Clients;
using play.inventory.service.Entities;
using Polly;
using Polly.Timeout;

var builder = WebApplication.CreateBuilder(args);
var serviceProvider = builder.Services.BuildServiceProvider();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddMongo()
    .AddMongoRepository<InventoryItem>("Inventoryitems");
Random jitterer = new Random();
//The first thing we do when we configure our httpclient to receive resources from the other microservice is :
// Add policy to retry should in case of network faliure, for specified amount of time and wait a bit longer for every retry
// We also added a jitterer so as to be sure the request is not made at the same time with another request been involke to avoid collision
// We added a circuit breaker to avoid resource exhaustion(so thatto many request will not be made to a failed service)
builder.Services.AddHttpClient<CatalogClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5240");
})
.AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
    5,
    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)/* what this does is to make sure it wait a bit longer for every attempt*/)
        + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000))
    // ,onRetry: (outcome, timespan, retryAttempt) =>
    // {
    //     serviceProvider.GetService<ILogger<CatalogClient>>()
    //     .LogWarning($"Delaying fro {timespan.TotalSeconds} seconds, then making retry {retryAttempt}"); // Not neccessary, just to check plus, look for a way to bring in ILogger
    // }
))
.AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
    3,
    TimeSpan.FromSeconds(15)
    // ,onBreak: (outcome,timespan) => {
    //     serviceProvider.GetService<ILogger<CatalogClient>>()
    //     .LogWarning($"Opening the Circuit for {timespan.TotalSeconds} seconds...");
    // },
    // onReset: ()=>{
    //     serviceProvider.GetService<ILogger<CatalogClient>>()
    //     .LogWarning($"Closing the circuit.......");
    // }
))
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));// Anytime we involke anything on this local host, we will wat as much as 1sec before giving up
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
