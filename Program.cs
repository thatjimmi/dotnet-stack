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
 */

using Models;
using Services;
using Interfaces;
using Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "RedisInstance";
});
    
builder.Services.AddScoped<ICacheService, RedisService>();

// Initialiser InventoryManager og tilføj seed data
var inventoryManager = new InventoryManager();
inventoryManager.AddProduct(new Product(101, "Tastatur", 125.50, 10));
inventoryManager.AddProduct(new Product(102, "Mus", 80.00, 15));

builder.Services.AddSingleton(inventoryManager);

var app = builder.Build();

// Endpoint defineret udenfor
ProductEndpoints.Map(app);

// Sundhedstjek endpoint
app.MapGet("/", () =>
{
    return Results.Ok("Healthy");
});

app.Run();
