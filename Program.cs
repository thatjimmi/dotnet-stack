using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Services;
using Interfaces;
using Repositories;
using Decorators;
using Models;
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

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis"); 
    options.InstanceName = "RedisInstance";
});

/* Postgres */
builder.Services.AddDbContext<IDatabaseContext, DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("database")));

/* Brug InMemoryCacheService */
//builder.Services.AddSingleton<ICacheService, InMemoryCacheService>();

/* Brug RedisCacheService */
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

/* Registrer ProductService */
builder.Services.AddScoped<IProductService, ProductService>();

// Register ProductRepository
builder.Services.AddScoped<ProductRepository>();

// Register FakeProductRepository
//builder.Services.AddSingleton<FakeProductRepository>();

// Register the decorator for IProductRepository
builder.Services.AddScoped<IRepository<Product>>(provider =>
{
    var baseRepository = provider.GetRequiredService<ProductRepository>();
    var cacheService = provider.GetRequiredService<ICacheService>();
    return new CachingRepositoryDecorator<Product>(baseRepository, cacheService);
});

var app = builder.Build();

/* Seeding the database */
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var productRepository = services.GetRequiredService<IRepository<Product>>();

    if (await productRepository.IsEmpty())
    {
        var seedData = IRepository<Product>.GenerateSeedData(10, index =>
            new Product
            {
                Name = Faker.Internet.DomainWord(),
                Price = Faker.RandomNumber.Next(100, 10000),
                Quantity = Faker.RandomNumber.Next(1, 100)
            }
        );

        foreach (var product in seedData)
        {
            await productRepository.AddAsync(product);
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

/* Tilføjelse af Produktendpoints */
ProductEndpoints.Map(app);

app.MapGet("/", async (HttpContext httpContext) =>
{
    await Task.Delay(TimeSpan.FromSeconds(3));
    httpContext.Response.Headers.CacheControl = "public, max-age=120";

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
