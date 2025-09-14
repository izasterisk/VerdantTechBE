using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace DAL.Data;

/// <summary>
/// JSON conversion and comparison helpers for Entity Framework Core
/// Provides reusable converters and comparers for JSON properties
/// </summary>
public static class JsonHelpers
{
    private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions();
    private static readonly JsonSerializerOptions CamelCaseOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    // =====================================================
    // VALUE CONVERTERS
    // =====================================================
    
    /// <summary>
    /// Converter for Dictionary&lt;string, object&gt; to JSON string
    /// </summary>
    public static ValueConverter<Dictionary<string, object>, string> DictionaryStringObjectConverter()
    {
        return new ValueConverter<Dictionary<string, object>, string>(
            // Convert to database
            dict => dict.Count == 0 ? "{}" : JsonSerializer.Serialize(dict, DefaultOptions),
            // Convert from database  
            json => string.IsNullOrEmpty(json) || json == "{}" ? new Dictionary<string, object>() : 
                    JsonSerializer.Deserialize<Dictionary<string, object>>(json, DefaultOptions)!
        );
    }
    
    /// <summary>
    /// Converter for Dictionary&lt;string, decimal&gt; to JSON string
    /// </summary>
    public static ValueConverter<Dictionary<string, decimal>, string> DictionaryStringDecimalConverter()
    {
        return new ValueConverter<Dictionary<string, decimal>, string>(
            // Convert to database
            dict => dict.Count == 0 ? "{}" : JsonSerializer.Serialize(dict, DefaultOptions),
            // Convert from database
            json => string.IsNullOrEmpty(json) || json == "{}" ? new Dictionary<string, decimal>() : 
                    JsonSerializer.Deserialize<Dictionary<string, decimal>>(json, DefaultOptions)!
        );
    }
    
    /// <summary>
    /// Converter for List&lt;Dictionary&lt;string, object&gt;&gt; to JSON string
    /// </summary>
    public static ValueConverter<List<Dictionary<string, object>>, string> ListDictionaryConverter()
    {
        return new ValueConverter<List<Dictionary<string, object>>, string>(
            // Convert to database
            list => list.Count == 0 ? "[]" : JsonSerializer.Serialize(list, DefaultOptions),
            // Convert from database
            json => string.IsNullOrEmpty(json) || json == "[]" ? new List<Dictionary<string, object>>() : 
                    JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json, DefaultOptions)!
        );
    }
    
    /// <summary>
    /// Converter for List&lt;ContentBlock&gt; to JSON string (for forum posts content)
    /// </summary>
    public static ValueConverter<List<Models.ContentBlock>, string> ListContentBlockConverter()
    {
        return new ValueConverter<List<Models.ContentBlock>, string>(
            // Convert to database
            list => list.Count == 0 ? "[]" : JsonSerializer.Serialize(list, CamelCaseOptions),
            // Convert from database
            json => string.IsNullOrEmpty(json) || json == "[]" ? new List<Models.ContentBlock>() : 
                    JsonSerializer.Deserialize<List<Models.ContentBlock>>(json, CamelCaseOptions)!
        );
    }

    // =====================================================
    // VALUE COMPARERS
    // =====================================================
    
    /// <summary>
    /// Comparer for Dictionary&lt;string, object&gt; - enables change tracking for JSON properties
    /// </summary>
    public static ValueComparer<Dictionary<string, object>> DictionaryStringObjectComparer()
    {
        return new ValueComparer<Dictionary<string, object>>(
            // Equality comparison
            (left, right) => CompareByJson(left, right),
            // Hash code generation  
            dict => dict.Aggregate(0, (hash, kvp) => HashCode.Combine(hash, kvp.Key, kvp.Value ?? 0)),
            // Snapshot - deep clone
            dict => dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );
    }
    
    /// <summary>
    /// Comparer for Dictionary&lt;string, decimal&gt; - enables change tracking for JSON properties
    /// </summary>
    public static ValueComparer<Dictionary<string, decimal>> DictionaryStringDecimalComparer()
    {
        return new ValueComparer<Dictionary<string, decimal>>(
            // Equality comparison
            (left, right) => CompareByJson(left, right),
            // Hash code generation
            dict => dict.Aggregate(0, (hash, kvp) => HashCode.Combine(hash, kvp.Key, kvp.Value)),
            // Snapshot - deep clone
            dict => dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
        );
    }
    
    /// <summary>
    /// Comparer for List&lt;Dictionary&lt;string, object&gt;&gt; - for complex JSON structures
    /// </summary>
    public static ValueComparer<List<Dictionary<string, object>>> ListDictionaryComparer()
    {
        return new ValueComparer<List<Dictionary<string, object>>>(
            // Equality comparison
            (left, right) => CompareByJson(left, right),
            // Hash code generation
            list => list.Aggregate(0, (hash, dict) => HashCode.Combine(hash, dict)),
            // Snapshot - deep clone
            list => list.Select(d => d.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)).ToList()
        );
    }
    
    /// <summary>
    /// Comparer for List&lt;ContentBlock&gt; - for forum post content blocks
    /// </summary>
    public static ValueComparer<List<Models.ContentBlock>> ListContentBlockComparer()
    {
        return new ValueComparer<List<Models.ContentBlock>>(
            // Equality comparison
            (left, right) => CompareByJson(left, right),
            // Hash code generation
            list => list.Aggregate(0, (hash, cb) => HashCode.Combine(hash, cb.Order, cb.Type, cb.Content)),
            // Snapshot - deep clone
            list => list.Select(cb => new Models.ContentBlock 
            { 
                Order = cb.Order, 
                Type = cb.Type, 
                Content = cb.Content 
            }).ToList()
        );
    }

    // =====================================================
    // HELPER METHODS
    // =====================================================
    
    private static bool CompareByJson<T>(T left, T right)
    {
        var leftJson = JsonSerializer.Serialize(left, DefaultOptions);
        var rightJson = JsonSerializer.Serialize(right, DefaultOptions);
        return leftJson == rightJson;
    }
}

// =====================================================
// CONFIGURATION EXTENSIONS (Must be outside JsonHelpers class)
// =====================================================

/// <summary>
/// Extension methods for easy configuration of JSON properties
/// </summary>
public static class JsonHelpersExtensions
{
    /// <summary>
    /// Configure a Dictionary&lt;string, object&gt; property as JSON with converter and comparer
    /// </summary>
    public static PropertyBuilder<Dictionary<string, object>> 
        ConfigureAsJson(this PropertyBuilder<Dictionary<string, object>> propertyBuilder,
                       string columnName, string defaultValue = "{}")
    {
        propertyBuilder
            .HasConversion(JsonHelpers.DictionaryStringObjectConverter())
            .HasColumnType("json")
            .HasDefaultValueSql($"'{defaultValue}'")
            .HasColumnName(columnName);
        
        propertyBuilder.Metadata.SetValueComparer(JsonHelpers.DictionaryStringObjectComparer());
        return propertyBuilder;
    }
    
    /// <summary>
    /// Configure a Dictionary&lt;string, decimal&gt; property as JSON with converter and comparer
    /// </summary>
    public static PropertyBuilder<Dictionary<string, decimal>> 
        ConfigureAsJson(this PropertyBuilder<Dictionary<string, decimal>> propertyBuilder,
                       string columnName, string defaultValue = "{}")
    {
        propertyBuilder
            .HasConversion(JsonHelpers.DictionaryStringDecimalConverter())
            .HasColumnType("json")
            .HasDefaultValueSql($"'{defaultValue}'")
            .HasColumnName(columnName);
        
        propertyBuilder.Metadata.SetValueComparer(JsonHelpers.DictionaryStringDecimalComparer());
        return propertyBuilder;
    }
}
