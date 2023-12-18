using Interfaces;
using Models;

namespace Endpoints;

public static class ProductEndpoints
{    
    public static void Map(WebApplication app)
    {
        app.MapGet("/products", async (IProductService productService) =>
        {
            var products = await productService.GetAllProductsAsync();
            return Results.Ok(products);
        });

        app.MapGet("/products/{id}", async (int id, IProductService productService) =>
        {
            var product = await productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(product);
        });

        app.MapPost("/products", async (ProductDto productDto, IProductService productService) =>
        {   
            try 
            {
                var product = await productService.AddProductAsync(productDto);                      
                return Results.Created($"/products/{product.ProductID}", product);
            }
            catch (ArgumentException e)
            {
                return Results.BadRequest(e.Message);
            }
        });

        app.MapDelete("products/{id}", async (int id, IProductService productService) =>
        {
            var product = await productService.DeleteProductAsync(id);

            if (product == null)
            {
                return Results.NotFound();
            }

            return Results.NoContent(); // 204 No Content svar er passende for en DELETE operation
        });
    }
}