using Microsoft.EntityFrameworkCore;
using MeterReading.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using MeterReading.Domain;
using MeterReading.Infrastructure.Services;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger/OpenAPI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Meter Reading API",
        Version = "v1",
        Description = "API for managing meter reading uploads and data validation",
        Contact = new OpenApiContact
        {
            Name = "Support",
            Email = "Support@test.com"
        }
    });

    // Add support for file uploads
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });

    // Additional configuration to handle multipart forms
    c.OperationFilter<MultipartFormDataOperationFilter>();

    // Include XML comments if available
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure Entity Framework
builder.Services.AddDbContext<MeterReadingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Cache as Singleton
builder.Services.AddSingleton<Cache>();

// Configure services 
builder.Services.AddScoped<IMeterReadingService, MeterReadingService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Meter Reading API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Test endpoint for resetting cache - would be done where DB is updated normally for account table
app.MapGet("/test/trigger-cache-update", async ([FromServices] Cache cache) =>
{
    await cache.OnAccountsModifiedAsync();
    return Results.Ok("Cache updated.");
})
.WithTags("Testing")
.WithSummary("Trigger cache update")
.WithDescription("Forces a refresh of the account cache for testing purposes");

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MeterReadingContext>();

    // Ensure database is created
    await context.Database.EnsureCreatedAsync();

    // Path to your CSV file
    var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Test_Accounts.csv");

    // Run the seeder
    await DataSeeder.SeedTestAccountsAsync(context, csvPath);

    // Refresh cache after seeding data using async version
    var cache = app.Services.GetRequiredService<Cache>();
    await cache.OnAccountsModifiedAsync();
}

app.Run();

// Operation filter to handle multipart form data for file uploads
public class MultipartFormDataOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody == null) return;

        var formFileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile))
            .ToArray();

        if (!formFileParams.Any()) return;

        operation.RequestBody.Content["multipart/form-data"] = new OpenApiMediaType
        {
            Schema = new OpenApiSchema
            {
                Type = "object",
                Properties = formFileParams.ToDictionary(
                    p => p.Name!,
                    p => new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                )
            }
        };
    }
}