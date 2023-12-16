/*
 * Statelessness: 
 * Vigtigt i forhold til horizontal x-skalering, 
 * da hver anmodning kan håndterings af enhver instans af applikationen,
 * hvilket er afgørende for effektiv load balancing.
 * 
 * Cache: 
 * Redis distribueret cache, så de er tilgængelige på tværs af alle instanser.
 * A single Redis instance is not a distributed system. It is a remote centralized store.
 * 
 * Hvad menes der helt konkret med distrueret cache.
 * docker run -d --name redis-cache -p 6379:6379 redis
 * 
 * Cache Invalidering og Opdatering: Definér en mekanisme for, 
 * hvordan og hvornår cache skal invalideres eller opdateres, 
 * så du sikrer, at dine applikationsinstanser arbejder med aktuelle data.
 * 
 *  Cache på Forskellige Lag
 *  Brug Nginx til at cache statiske ressourcer og enkle, sjældent ændrede API-anmodninger.
 *  Brug Redis til mere komplekse og dynamisk genererede data, hvor du har brug for finere kontrol og hurtigere adgang.
 * 
 * Load balancing:
 * Man kan køre flere instanser på forskellige porte 
 * og bruge en simpel load balancer til
 * at distribuere anmodninger til de forskellige porte
 * 
 * NGINX:
 * cd nginx && docker-compose up --build
 *
 * Postgres:
 * docker run --name mypostgres -e POSTGRES_USER=myuser -e POSTGRES_PASSWORD=mypassword -p 5433:5432 -d postgres
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
 * By applying DI, you gain the following advantages:
 * Decoupling: Your classes are not directly dependent on concrete implementations of their dependencies, making the system more modular.
 * Easier Testing: You can easily mock or replace dependencies like ShopContext when writing unit tests.
 * Managed Lifecycle: The DI container takes care of the lifecycle of the dependencies, which is especially important for resources like database contexts.
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
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Services;
using Interfaces;
using Endpoints;
using Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {       
        Version = "v1",
        Title = "API",
        Description = "Description"
    });
});

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
    options.Configuration = "redis:6379";
    options.InstanceName = "RedisInstance";
});


/* Entity Framework
 * 
 * Brug EF Core migrations til at oprette din database. 
 * Først, generer en migration baseret på dine modeller:
 * dotnet ef migrations add InitialCreate
 * 
 * Derefter anvender du migrationen for at oprette eller opdatere databasen:
 * dotnet ef database update
 * 
 * Dette vil oprette myapp.db SQLite-filen i dit projekt (eller i den output-mappe, hvor din applikation kører), 
 * og anvende den definerede schema-struktur baseret på dine modeller.
*/

/* In-Memory "Database" */
//builder.Services.AddSingleton<IDatabaseContext, InMemoryDatabaseContext>();

/* Postgres */
builder.Services.AddDbContext<IDatabaseContext, DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("database")));

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
//builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

// Brug af RedisService
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

var app = builder.Build();

app.UseStaticFiles();

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

app.MapGet("/", async (HttpContext httpContext) =>
{
    await Task.Delay(TimeSpan.FromSeconds(2));
    httpContext.Response.Headers.CacheControl = "public, max-age=120"; // Tillader caching i 120 sekunder
    var db = builder.Configuration.GetConnectionString("Database");
    return Results.Ok($"Hello! {db}.");
});

/* Brug http:\//localhost/image?url=https:\//assets.unileversolutions.com/v1/30733653.png */
app.MapGet("/image", async (HttpContext httpContext) =>
{
    // Få URL-parameteren fra query string
    var url = httpContext.Request.Query["url"].ToString();

    using var httpClient = new HttpClient();
    var response = await httpClient.GetAsync(url);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem("Kunne ikke hente billedet.");
    }

    var imageContent = await response.Content.ReadAsByteArrayAsync();

    //Simulerer en langsommere behandling eller et tungt beregningsarbejde
    //await Task.Delay(TimeSpan.FromSeconds(2));

    httpContext.Response.Headers.CacheControl = "public, max-age=120"; // Tillader caching i 120 sekunder

    Console.WriteLine("Her får du billedet fra project 2 instans");

    return Results.File(imageContent, response.Content.Headers.ContentType?.ToString());
});

app.Run();
