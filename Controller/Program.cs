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
using BLL.Helpers;
using DAL.Data.Models;
using Infrastructure.Extensions;
using BLL.DTO;
using System.Net;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
try 
{
    var envPath = File.Exists("Controller/.env") ? "Controller/.env" : 
        File.Exists(".env") ? ".env" : 
        throw new FileNotFoundException(".env file not found");
    Env.Load(envPath);
}
catch (Exception ex)
{
    Console.WriteLine($"Cảnh báo: Không thể tải tệp .env: {ex.Message}. Sử dụng biến môi trường.");
}

// Get connection string from .env
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DATABASE_CONNECTION_STRING not found in .env file.");
}

// Get database timeout from .env (default to 30 seconds)
var databaseTimeoutStr = Environment.GetEnvironmentVariable("DATABASE_TIMEOUT") ?? "30";
if (!int.TryParse(databaseTimeoutStr, out int databaseTimeout))
{
    databaseTimeout = 30; // fallback to 30 seconds
    Console.WriteLine($"Warning: Invalid DATABASE_TIMEOUT value '{databaseTimeoutStr}'. Using default 30 seconds.");
}

// Configure DbContext
builder.Services.AddDbContext<VerdantTechDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.43-mysql"), 
        b => b.MigrationsAssembly("DAL")
            .CommandTimeout(databaseTimeout)));

//Dependency Injection
builder.Services.AddScoped<IRepository<User>, Repository<User>>();
builder.Services.AddScoped<IRepository<VendorProfile>, Repository<VendorProfile>>();
builder.Services.AddScoped<IRepository<FarmProfile>, Repository<FarmProfile>>();
builder.Services.AddScoped<IRepository<Address>, Repository<Address>>();
builder.Services.AddScoped<IRepository<UserAddress>, Repository<UserAddress>>();
builder.Services.AddScoped<IRepository<Fertilizer>, Repository<Fertilizer>>();
builder.Services.AddScoped<IRepository<EnergyUsage>, Repository<EnergyUsage>>();
builder.Services.AddScoped<IRepository<EnvironmentalDatum>, Repository<EnvironmentalDatum>>();
builder.Services.AddScoped<IRepository<ProductCategory>, Repository<ProductCategory>>();
builder.Services.AddScoped<IRepository<Product>, Repository<Product>>();
builder.Services.AddScoped<IRepository<Cart>, Repository<Cart>>();
builder.Services.AddScoped<IRepository<CartItem>, Repository<CartItem>>();
builder.Services.AddScoped<IRepository<Order>, Repository<Order>>();
builder.Services.AddScoped<IRepository<OrderDetail>, Repository<OrderDetail>>();
builder.Services.AddScoped<IRepository<ProductRegistration>, Repository<ProductRegistration>>();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IFarmProfileRepository, FarmProfileRepository>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IFertilizerRepository, FertilizerRepository>();
builder.Services.AddScoped<IEnergyUsageRepository, EnergyUsageRepository>();
builder.Services.AddScoped<IEnvironmentalDataRepository, EnvironmentalDataRepository>();
builder.Services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddScoped<IProductRegistrationRepository, ProductRegistrationRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFarmProfileService, FarmProfileService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<ICO2Service, CO2Service>();
builder.Services.AddScoped<ICourierService, CourierService>();
builder.Services.AddScoped<IProductCategoryService, ProductCategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Infrastructure registrations
builder.Services.AddInfrastructure();

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperConfig));

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

    // Cấu hình events để trả về message tùy chỉnh
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            // Ngăn không cho default challenge response
            context.HandleResponse();
            // Tạo custom response cho 401 Unauthorized
            var response = APIResponse.Error("Bạn cần đăng nhập để truy cập chức năng này.", HttpStatusCode.Unauthorized);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";
            var jsonResponse = JsonConvert.SerializeObject(response);
            return context.Response.WriteAsync(jsonResponse);
        },
        
        OnForbidden = context =>
        {
            // Tạo custom response cho 403 Forbidden
            var response = APIResponse.Error("Bạn không có quyền truy cập chức năng này.", HttpStatusCode.Forbidden);
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            context.Response.ContentType = "application/json";
            var jsonResponse = JsonConvert.SerializeObject(response);
            return context.Response.WriteAsync(jsonResponse);
        }
    };
});

builder.Services.AddAuthorization();

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "VerdantTech API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header. Just enter your token (without 'Bearer' prefix)",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

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

// Enable CORS
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
