/*
 * Statelessness: 
 * Vigtigt i forhold til horizontal x-skalering, 
 * da hver anmodning kan håndterings af enhver instans af applikationen,
 * hvilket er afgørende for effektiv load balancing.
 * 
 * Cache: 
 * Redis distribueret cache, så de er tilgængelige på tværs af alle instanser.
 * Hvad menes der helt konkret med distrueret cache.
 * docker run -d --name redis-cache -p 6379:6379 redis
 * 
 * Cache Invalidering og Opdatering: Definér en mekanisme for, 
 * hvordan og hvornår cache skal invalideres eller opdateres, 
 * så du sikrer, at dine applikationsinstanser arbejder med aktuelle data.
 * 
 * Load balancing:
 * Man kan køre flere instanser på forskellige porte 
 * og bruge en simpel load balancer til
 * at distribuere anmodninger til de forskellige porte
 * 
 * NGINX:
 * cd nginx && docker-compose up --build
 *
 *
 * Centraliseret Logging: Sørg for, at din applikation
 * kan logge til en centraliseret logningsløsning, 
 * så du kan aggregere og analysere logs fra alle instanser.
 *
 * 
 * Afkoble løsningen ved at lægge redis i sin egen klasse. 
 * Dermed kan man nemt skifte den ud, hvis det skulle være
 * 
 * 
 * Dependency Injection:
 * Hvordan kan det ses her?
 * 
 * Flexability:
 * Kan nemt skifte cachingmekanisme ud.
 * 
 * 
 * Asynkron Programmering:
 * Asynkron programmering i C# er særligt nyttig i situationer, 
 * hvor du har operationer, der kan tage tid at fuldføre, 
 * og du ikke ønsker at blokere den tråd, der kører operationen.
 * 
 * 
 */

using Models;
using Services;
using Interfaces;
using Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/* 
 * This adds and configures the Redis distributed cache implementation to the 
 * application's services collection. This allows the application to use 
 * Redis for caching by injecting IDistributedCache into classes, like your RedisService.
 * 
 * When RedisService is instantiated, it will receive an instance of 
 * IDistributedCache that is configured to use Redis as the backing store, 
 * thanks to the configuration in AddStackExchangeRedisCache.
 * 
 * This configuration sets up Redis as the distributed cache implementation 
 * for your application and ensures that when your RedisService asks for an 
 * IDistributedCache, it gets one that is backed by Redis.
*/
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "RedisInstance";
});

/*
 * The difference between AddScoped and AddSingleton in ASP.NET Core's dependency injection (DI)
 * 
 * AddScoped (Scoped Lifecycle):
 * A new instance of the service is created for each request (or scope) in your application.
 * In the context of a web application, a new instance is created for each HTTP request.
 * 
 * AddSingleton (Singleton Lifecycle):
 * Only one instance of the service is created for the entire application's lifetime. 
 * The same instance is used across all requests and scopes.
 * The singleton instance is disposed of when the application shuts down.
 * 
 * 
 * Database Contexts in EF Core:
 * In Entity Framework Core, DbContext is usually added as scoped. 
 * This ensures that each request gets its own context instance, 
 * which is ideal for handling transactions 
 * and database operations within the scope of a request.
 * Example: services.AddScoped<MyDbContext>();
*/

// Brug af InMemoryCacheService
builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

// Brug af RedisService
//builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Initialiser InventoryManager og tilføjer Seed Data
var inventoryManager = new InventoryManager();
inventoryManager.AddProduct(new Product(101, "Tastatur", 125.50, 10));
inventoryManager.AddProduct(new Product(102, "Mus", 80.00, 15));

builder.Services.AddSingleton(inventoryManager);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Tilføjelse af Produktendpoints
ProductEndpoints.Map(app);

/*
 * Sundhedstjek Endpoint:
 * Dette endpoint kan udvides til at tjekke vitale dele af din applikation, 
 * som for eksempel databaseforbindelser, eksterne afhængigheder, 
 * eller vigtige interne services
*/

app.MapGet("/health", () => "Healthy");

app.Run();
