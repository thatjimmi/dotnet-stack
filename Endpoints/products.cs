using Interfaces;
using Models;

namespace Endpoints;

public static class ProductEndpoints
{    
    public static void Map(WebApplication app)
    {
        app.MapGet("/products/{id}", async (int id, InventoryManager inventoryManager, ICacheService cacheService) =>
        {            
            string cacheKey = $"product_{id}";
            var cachedProduct = await cacheService.GetCacheAsync<Product>(cacheKey);

            if (cachedProduct != null)
            {
                Console.WriteLine("Data hentet fra cachen.");
                return Results.Ok(cachedProduct);
            }

            Console.WriteLine("Data ikke fundet i cachen, henter fra datakilde...");
            //var product = await dbContext.MyModels.FindAsync(id);
            var product = inventoryManager.GetProductById(id);
            await Task.Delay(TimeSpan.FromSeconds(3));

            if (product == null)
            {
                Console.WriteLine("Produkt ikke fundet");
                return Results.NotFound();
            }

            await cacheService.SetCacheAsync(cacheKey, product, TimeSpan.FromSeconds(5));

            Console.WriteLine("Data er nu opbevaret i cachen.");
            return Results.Ok(product);
                               
        });       
    }
}