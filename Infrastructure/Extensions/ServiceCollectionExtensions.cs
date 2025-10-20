using BLL.Interfaces.Infrastructure;
using Infrastructure.Address;
using Infrastructure.Email;
using Infrastructure.Soil;
using Infrastructure.Weather;
using Infrastructure.Courier;
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
        services.AddScoped<IGHNCourierApiClient, GHNCourierApiClient>();
        return services;
    }
    
    public static IServiceCollection AddAddress(this IServiceCollection services)
    {
        services.AddScoped<HttpClient>();
        services.AddScoped<IGHNAddressApiClient, GHNAddressApiClient>();
        services.AddScoped<IGoshipAddressApiClient, GoshipAddressApiClient>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddEmail();
        services.AddWeather();
        services.AddSoilGrids();
        services.AddCourier();
        services.AddAddress();
        return services;
    }
}