// ===== MeterReading.API/Program.cs =====
using Microsoft.EntityFrameworkCore;
using MeterReading.Infrastructure.Data;
//using MeterReading.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework
builder.Services.AddDbContext<MeterReadingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure services
//builder.Services.AddScoped<IMeterReadingService, MeterReadingService>();

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
}

app.Run();