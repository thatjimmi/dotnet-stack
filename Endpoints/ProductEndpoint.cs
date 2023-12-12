using Interfaces;
using Models;

namespace Endpoints;

public static class ProductEndpoints
{    
    public static void Map(WebApplication app)
    {
        app.MapGet("/products", async (IDatabaseContext context) =>
        {            
            var products = await context.GetProductsAsync();
            return Results.Ok(products);
        });

        app.MapGet("/products/{id}", async (
            int id,
            IDatabaseContext context,
            ICacheService cacheService
            ) =>
        {
            string cacheKey = $"product_{id}";
            var cachedProduct = await cacheService.GetFromCacheAsync<Product>(cacheKey);

            if (cachedProduct != null)
            {
                Console.WriteLine("Data hentet fra cachen.");
                return Results.Ok(cachedProduct);
            }

            Console.WriteLine("Data ikke fundet i cachen, henter fra datakilde...");
            var product = await context.GetProductByIdAsync(id);

            // Tilføjer delay for at simulere at der er en masse arbejde med at hente
            await Task.Delay(TimeSpan.FromSeconds(3));

            if (product == null)
            {
                Console.WriteLine("Produkt ikke fundet");
                return Results.NotFound();
            }

            await cacheService.AddToCacheAsync(cacheKey, product, TimeSpan.FromSeconds(10));

            Console.WriteLine("Data er nu opbevaret i cachen.");
            return Results.Ok(product);

        });

        app.MapPost("/products", async (ProductDto productDto, IDatabaseContext context) =>
        {
            var product = new Product
            {
                Name = productDto.Name,
                Price = productDto.Price,
                Quantity = productDto.Quantity
            };

            await context.AddProductAsync(product);

            return Results.Created($"/products/{product.ProductID}", product);
        });

        app.MapDelete("products/{id}", async (int id, IDatabaseContext context) =>
        {
            try
            {
                await context.DeleteProductAsync(id);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }

            return Results.NoContent(); // 204 No Content svar er passende for en DELETE operation
        });
    }
}