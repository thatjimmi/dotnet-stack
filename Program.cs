/*
 * Statelessness: 
 * Vigtigt i forhold til horizontal x-skalering, 
 * da hver anmodning kan håndterings af enhver instans af applikationen.
 * 
 * Cache: 
 * Redis distribueret cache, så de er tilgængelige på tværs af alle instanser
 * docker run -d --name redis-cache -p 6379:6379 redis
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
 */

using Application;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "RedisInstance";
});

var app = builder.Build();

// Opretter en InventoryManager
var inventoryManager = new InventoryManager();
inventoryManager.AddProduct(new Product(101, "Tastatur", 125.50, 10));
inventoryManager.AddProduct(new Product(102, "Mus", 80.00, 15));


app.MapGet("/", () =>
{
    return Results.Ok("HEads");
});

app.MapGet("/product/{id}", async (int id, IDistributedCache cache) =>
{
    string cacheKey = $"product_{id}";
    string? productData = await cache.GetStringAsync(cacheKey);
    // Søger først i cachen
    if (productData == null)
    {
        Console.WriteLine("Data ikke fundet i cachen, henter fra datakilde...");

        // Hvis ikke i cachen, hentes produktet
        var product = inventoryManager.GetProductById(id);
        Console.WriteLine("Venter i 10 sekunder");
        await Task.Delay(10000);
        if (product == null)
        {
            return Results.NotFound();
        }

        // Serialiser produktet til en string (for eksempel via JSON) før cachelagring
        productData = System.Text.Json.JsonSerializer.Serialize(product);

        var cacheEntryOptions = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(1));
        await cache.SetStringAsync(cacheKey, productData, cacheEntryOptions);

        Console.WriteLine("Data cachelagret.");       

        return Results.Ok(product);
    
    }
    else
    {
        Console.WriteLine("Data hentet fra cachen.");

        var cachedProduct = System.Text.Json.JsonSerializer.Deserialize<Product>(productData);
        return Results.Ok(cachedProduct);
    }

});

app.Run();
