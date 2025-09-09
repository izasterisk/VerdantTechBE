using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DAL.Data.Models;
using System.Text.Json;

namespace DAL.Data.Configurations;

public class WeatherDataCacheConfiguration : IEntityTypeConfiguration<WeatherDataCache>
{
    public void Configure(EntityTypeBuilder<WeatherDataCache> builder)
    {
        // Primary Key
        builder.HasKey(e => e.Id);

        // Table configuration
        builder.ToTable("weather_data_cache");

        // Property configurations
        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasColumnType("bigint unsigned")
            .IsRequired()
            .ValueGeneratedOnAdd();

        builder.Property(e => e.FarmProfileId)
            .HasColumnName("farm_profile_id")
            .HasColumnType("bigint unsigned")
            .IsRequired();

        builder.Property(e => e.ApiSource)
            .HasColumnName("api_source")
            .HasConversion<string>()
            .HasColumnType("enum('openweathermap','accuweather')")
            .IsRequired();

        builder.Property(e => e.WeatherDate)
            .HasColumnName("weather_date")
            .HasColumnType("date")
            .IsRequired();

        builder.Property(e => e.TemperatureMin)
            .HasColumnName("temperature_min")
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.TemperatureMax)
            .HasColumnName("temperature_max")
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.TemperatureAvg)
            .HasColumnName("temperature_avg")
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.HumidityPercentage)
            .HasColumnName("humidity_percentage")
            .HasColumnType("decimal(5,2)");

        builder.Property(e => e.PrecipitationMm)
            .HasColumnName("precipitation_mm")
            .HasColumnType("decimal(8,2)");

        builder.Property(e => e.WindSpeedKmh)
            .HasColumnName("wind_speed_kmh")
            .HasColumnType("decimal(6,2)");

        builder.Property(e => e.WindDirection)
            .HasColumnName("wind_direction")
            .HasMaxLength(10);

        builder.Property(e => e.UvIndex)
            .HasColumnName("uv_index")
            .HasColumnType("decimal(3,1)");

        builder.Property(e => e.WeatherCondition)
            .HasColumnName("weather_condition")
            .HasMaxLength(100);

        builder.Property(e => e.WeatherIcon)
            .HasColumnName("weather_icon")
            .HasMaxLength(50);

        builder.Property(e => e.SunriseTime)
            .HasColumnName("sunrise_time")
            .HasColumnType("time");

        builder.Property(e => e.SunsetTime)
            .HasColumnName("sunset_time")
            .HasColumnType("time");

        // Configure JSON property using JsonHelpers
        builder.Property(e => e.RawApiResponse)
            .HasColumnName("raw_api_response")
            .HasColumnType("json")
            .HasConversion(JsonHelpers.DictionaryStringObjectConverter())
            .Metadata.SetValueComparer(JsonHelpers.DictionaryStringObjectComparer());

        builder.Property(e => e.FetchedAt)
            .HasColumnName("fetched_at")
            .HasColumnType("timestamp")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Foreign Key relationships
        builder.HasOne(e => e.FarmProfile)
            .WithMany(p => p.WeatherDataCache)
            .HasForeignKey(e => e.FarmProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint
        builder.HasIndex(e => new { e.FarmProfileId, e.WeatherDate, e.ApiSource })
            .IsUnique()
            .HasDatabaseName("unique_farm_date_source");

        // Indexes
        builder.HasIndex(e => new { e.FarmProfileId, e.WeatherDate })
            .HasDatabaseName("idx_farm_date");

        builder.HasIndex(e => e.FetchedAt)
            .HasDatabaseName("idx_fetched");
    }
}
