using BLL.Interfaces.Infrastructure;
using Infrastructure.Address;
using Infrastructure.Cloudinary;
using Infrastructure.Email;
using Infrastructure.Soil;
using Infrastructure.Weather;
using Infrastructure.Courier;
using Infrastructure.Payment.PayOS;
using Infrastructure.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmail(this IServiceCollection services)
    {
        services.AddScoped<IEmailSender, EmailSender>();
        return services;
    }

    public static IServiceCollection AddWeather(this IServiceCollection services)
    {
        services.AddScoped<HttpClient>();
        services.AddScoped<IWeatherApiClient, WeatherApiClient>();
        return services;
    }

    public static IServiceCollection AddSoilGrids(this IServiceCollection services)
    {
        services.AddScoped<HttpClient>();
        services.AddScoped<ISoilGridsApiClient, SoilGridsApiClient>();
        return services;
    }

    public static IServiceCollection AddCourier(this IServiceCollection services)
    {
        services.AddScoped<HttpClient>();
        services.AddScoped<IGoshipCourierApiClient, GoshipCourierApiClient>();
        return services;
    }
    
    public static IServiceCollection AddAddress(this IServiceCollection services)
    {
        services.AddScoped<HttpClient>();
        services.AddScoped<IGoshipAddressApiClient, GoshipAddressApiClient>();
        return services;
    }
    
    public static IServiceCollection AddPayOS(this IServiceCollection services)
    {
        // services.AddScoped<HttpClient>();
        services.AddScoped<IPayOSApiClient, PayOSApiClient>();
        return services;
    }

    public static IServiceCollection AddSignalRNotification(this IServiceCollection services)
    {
        services.AddScoped<INotificationHub, NotificationHubService>();
        return services;
    }
    
    public static IServiceCollection AddCloudinary(this IServiceCollection services)
    {
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddEmail();
        services.AddWeather();
        services.AddSoilGrids();
        services.AddCourier();
        services.AddAddress();
        services.AddPayOS();
        services.AddSignalRNotification();
        services.AddCloudinary();
        return services;
    }
}