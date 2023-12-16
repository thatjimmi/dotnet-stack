using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Services;
using Interfaces;
using Repositories;
using Decorators;
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
    options.Configuration = builder.Configuration.GetConnectionString("Redis"); 
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
*/

/* Postgres */
builder.Services.AddDbContext<IDatabaseContext, DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("database")));

/* Brug InMemoryCacheService */
//builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

/* Brug RedisCacheService */
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

/* Registrer ProductService */
builder.Services.AddScoped<IProductService, ProductService>();

// Register ProductRepository as itself
//builder.Services.AddScoped<ProductRepository>();

// Register FakeProductRepository
builder.Services.AddSingleton<FakeProductRepository>();

/*
 * The part that makes up the Decorator here is the IRepository + CachingRepository +
 * any other class that implements IRepository, 
 * and thus can be wrapped by the CachingRepository. 
 * The idea behind Decorator is that it dynamically adds functionality on top of an object, 
 * and here, that functionality is caching. 
 * 
*/

// Register the decorator for IProductRepository
builder.Services.AddScoped<IProductRepository>(provider =>
{
    var baseRepository = provider.GetRequiredService<FakeProductRepository>();
    var cacheService = provider.GetRequiredService<ICacheService>();
    return new CachingProductRepositoryDecorator(baseRepository, cacheService);
});

var app = builder.Build();

/* Seeding the database */
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<DatabaseContext>();
   
    if (!dbContext.Products.Any())
    {
        var seedData = DatabaseContext.GenerateSeedData(10);
        dbContext.Products.AddRange(seedData);
        dbContext.SaveChanges();
    }
}
    
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/* Tilføjelse af Produktendpoints */
ProductEndpoints.Map(app);

/*
 * Sundhedstjek Endpoint:
 * Dette endpoint kan udvides til at tjekke vitale dele af applikationen, 
 * som for eksempel databaseforbindelser, eksterne afhængigheder, 
 * eller vigtige interne services
*/

app.MapGet("/health", () => "Healthy");

app.MapGet("/", async (HttpContext httpContext) =>
{
    await Task.Delay(TimeSpan.FromSeconds(2));
    httpContext.Response.Headers.CacheControl = "public, max-age=120";
    var db = builder.Configuration.GetConnectionString("Database");
    return Results.Ok($"Hello!");
});

app.MapGet("/image", async (HttpContext httpContext) =>
{
    var url = httpContext.Request.Query["url"].ToString();
    using var httpClient = new HttpClient();
    var response = await httpClient.GetAsync(url);

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem("Kunne ikke hente billedet.");
    }

    var imageContent = await response.Content.ReadAsByteArrayAsync();

    httpContext.Response.Headers.CacheControl = "public, max-age=120";

    return Results.File(imageContent, response.Content.Headers.ContentType?.ToString());
});

app.Run();
