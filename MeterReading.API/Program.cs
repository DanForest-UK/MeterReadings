using Microsoft.EntityFrameworkCore;
using MeterReading.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using MeterReading.Domain;
using MeterReading.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework
builder.Services.AddDbContext<MeterReadingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Cache as Singleton - IMPORTANT: Must be singleton for shared state
builder.Services.AddSingleton<Cache>();

// Configure services - Cache will be injected automatically
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
    app.UseSwaggerUI();
}

app.UseCors("AllowAngular");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/test/trigger-cache-update", async ([FromServices] Cache cache) =>
{
    await cache.OnAccountsModifiedAsync();
    return Results.Ok("Cache updated.");
});

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