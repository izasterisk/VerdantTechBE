using DotNetEnv;
using DAL.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Load .env file từ Controller folder
Env.Load();

// Get connection string from .env
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DATABASE_CONNECTION_STRING not found in .env file.");
}

// Configure DbContext
builder.Services.AddDbContext<VerdantTechDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.43-mysql"), 
        b => b.MigrationsAssembly("DAL")));

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Database recreation setup - for development/migration
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<VerdantTechDbContext>();
        
        // Delete existing database if it exists
        await context.Database.EnsureDeletedAsync();
        
        // Create new database with current schema
        await context.Database.EnsureCreatedAsync();
        
        // Optionally seed initial data here if needed
        Console.WriteLine("Database recreated successfully!");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    var openSwagger = Environment.GetEnvironmentVariable("OPEN_SWAGGER");
    if (string.Equals(openSwagger, "True", StringComparison.OrdinalIgnoreCase))
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
