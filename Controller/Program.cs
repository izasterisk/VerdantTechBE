using DotNetEnv;
using DAL.Data;
using DAL.IRepository;
using DAL.Repository;
using BLL.Interfaces;
using BLL.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using DAL.Data.Models;

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

//Dependency Injection
builder.Services.AddScoped<IRepository<User>, Repository<User>>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// JWT Configuration
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(jwtIssuer) || string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException("JWT configuration variables not found in .env file.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Database recreation setup - for development/migration
if (app.Environment.IsDevelopment())
{
    var autoCreateDb = Environment.GetEnvironmentVariable("AUTO_CREATE_DB");
    if (string.Equals(autoCreateDb, "True", StringComparison.OrdinalIgnoreCase))
    {
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<VerdantTechDbContext>();
            
            // Delete existing database if it exists
            await context.Database.EnsureDeletedAsync();
            
            // Create new database with current schema
            await context.Database.EnsureCreatedAsync();
            
            Console.WriteLine("Database recreated successfully!");
        }
    }
    else
    {
        Console.WriteLine("AUTO_CREATE_DB is disabled. Skipping database recreation.");
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
