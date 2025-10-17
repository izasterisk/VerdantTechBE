using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

public sealed class JsonModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext ctx)
    {
        if (ctx == null) throw new ArgumentNullException(nameof(ctx));

        // Lấy giá trị từ form-data theo tên property (vd: "specifications")
        var value = ctx.ValueProvider.GetValue(ctx.ModelName);
        if (value == ValueProviderResult.None)
        {
            // Không có key -> để null để service biết là "không gửi"
            ctx.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        var raw = value.FirstValue;
        if (string.IsNullOrWhiteSpace(raw))
        {
            // Có key nhưng rỗng -> trả về dict rỗng
            ctx.Result = ModelBindingResult.Success(new Dictionary<string, object>());
            return Task.CompletedTask;
        }

        try
        {
            // Parse JSON string thành Dictionary<string, object>
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parsed = JsonSerializer.Deserialize<Dictionary<string, object>>(raw, options);

            // Convert JsonElement -> object .NET (số, bool, array, object…)
            ctx.Result = ModelBindingResult.Success(ToObjectDictionary(parsed));
        }
        catch (Exception)
        {
            ctx.ModelState.AddModelError(ctx.ModelName, "Invalid JSON for specifications.");
            ctx.Result = ModelBindingResult.Success(new Dictionary<string, object>());
        }
        return Task.CompletedTask;
    }

    private static Dictionary<string, object>? ToObjectDictionary(Dictionary<string, object>? raw)
    {
        if (raw == null) return null;

        object? Convert(object? val)
        {
            if (val is JsonElement je)
            {
                switch (je.ValueKind)
                {
                    case JsonValueKind.Object:
                        var dict = new Dictionary<string, object?>();
                        foreach (var p in je.EnumerateObject())
                            dict[p.Name] = Convert(p.Value);
                        return dict!;
                    case JsonValueKind.Array:
                        var list = new List<object?>();
                        foreach (var it in je.EnumerateArray())
                            list.Add(Convert(it));
                        return list;
                    case JsonValueKind.String:
                        return je.GetString()!;
                    case JsonValueKind.Number:
                        if (je.TryGetInt64(out var l)) return l;
                        if (je.TryGetDouble(out var d)) return d;
                        return je.ToString();
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        return je.GetBoolean();
                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                    default:
                        return null;
                }
            }
            return val;
        }

        var result = new Dictionary<string, object>();
        foreach (var kv in raw)
            result[kv.Key] = Convert(kv.Value)!;
        return result;
    }
}
