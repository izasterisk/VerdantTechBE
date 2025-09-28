using BLL.Interfaces.Infrastructure;
using System.Globalization;
using System.Text;
using BLL.DTO.Soil;

namespace Infrastructure.Soil;

public static class SoilGridsHelper
{
    /// <summary>
    /// Build URL for SoilGrids API query
    /// </summary>
    public static string BuildSoilGridsUrl(string baseUrl, decimal latitude, decimal longitude)
    {
        var properties = "property=phh2o&property=sand&property=silt&property=clay";
        var depths = "depth=0-5cm&depth=5-15cm&depth=15-30cm";
        var value = "value=mean";
        
        // Use InvariantCulture to ensure dot (.) as decimal separator, not comma (,)
        return $"{baseUrl}query?lon={longitude.ToString("F4", CultureInfo.InvariantCulture)}&lat={latitude.ToString("F4", CultureInfo.InvariantCulture)}&{properties}&{depths}&{value}";
    }
    
    /// <summary>
    /// Convert raw value to percentage or actual value based on d_factor
    /// For sand, silt, clay: divide by d_factor (10) to convert g/kg to %
    /// For pH: divide by d_factor (10) to get actual pH value
    /// </summary>
    public static decimal ConvertValue(decimal rawValue, int dFactor)
    {
        return rawValue / dFactor;
    }
    
    /// <summary>
    /// Validate that all soil parameters have valid values
    /// </summary>
    public static void ValidateSoilData(
        decimal sand0_5, decimal sand5_15, decimal sand15_30,
        decimal silt0_5, decimal silt5_15, decimal silt15_30,
        decimal clay0_5, decimal clay5_15, decimal clay15_30,
        decimal ph0_5, decimal ph5_15, decimal ph15_30)
    {
        // Check for any invalid percentage values (sand, silt, clay)
        if (HasInvalidPercentageValue(sand0_5) || HasInvalidPercentageValue(sand5_15) || HasInvalidPercentageValue(sand15_30) ||
            HasInvalidPercentageValue(silt0_5) || HasInvalidPercentageValue(silt5_15) || HasInvalidPercentageValue(silt15_30) ||
            HasInvalidPercentageValue(clay0_5) || HasInvalidPercentageValue(clay5_15) || HasInvalidPercentageValue(clay15_30))
        {
            throw new InvalidOperationException("Địa chỉ này chưa được hỗ trợ, vui lòng thử địa chỉ khác.");
        }
        
        // Check for any invalid pH values
        if (HasInvalidPhValue(ph0_5) || HasInvalidPhValue(ph5_15) || HasInvalidPhValue(ph15_30))
        {
            throw new InvalidOperationException("Địa chỉ này chưa được hỗ trợ, vui lòng thử địa chỉ khác.");
        }
    }
    
    /// <summary>
    /// Check if a percentage value (sand, silt, clay) is invalid
    /// </summary>
    private static bool HasInvalidPercentageValue(decimal value)
    {
        return value <= 0 || value > 100;
    }
    
    /// <summary>
    /// Check if a pH value is invalid (should be between 0 and 14)
    /// </summary>
    private static bool HasInvalidPhValue(decimal value)
    {
        return value <= 0 || value > 14;
    }
    
    /// <summary>
    /// Tính BBOX (CRS:84: lon,lat,lon,lat) và pixel I/J cho WMS GetFeatureInfo.
    /// delta: độ (độ kinh/vĩ) cho chiều rộng & chiều cao của BBOX, default 0.2
    /// width/height: kích thước ảnh WMS (px), default 256.
    /// </summary>
    public static (string BBox, int I, int J) ComputeWmsParameters(
        double lon, double lat, double delta = 0.2, int width = 256, int height = 256)
    {
        // 1) Tính min/max theo lon/lat (CRS:84 giữ thứ tự lon,lat)
        double half = delta / 2.0;
        double minLon = lon - half;
        double maxLon = lon + half;
        double minLat = lat - half;
        double maxLat = lat + half;

        // 2) Chuỗi BBOX theo InvariantCulture
        string bbox = string.Create(
            CultureInfo.InvariantCulture, $"{minLon},{minLat},{maxLon},{maxLat}");

        // 3) Tính I (0..width-1)
        double iCalc = (lon - minLon) / (maxLon - minLon) * (width - 1);
        int I = (int)Math.Round(iCalc);

        // 4) Tính J (0..height-1), gốc ảnh nằm góc trên trái
        double jCalc = (maxLat - lat) / (maxLat - minLat) * (height - 1);
        int J = (int)Math.Round(jCalc);

        // 5) Clamp an toàn
        I = Math.Clamp(I, 0, Math.Max(0, width - 1));
        J = Math.Clamp(J, 0, Math.Max(0, height - 1));

        return (bbox, I, J);
    }
    
    public static (string Map, string Layer) GetWmsMapAndLayer(string property, string depthLabel)
    {
        string prop = property.ToLowerInvariant();
        string map = prop switch
        {
            "sand"  => "/map/sand.map",
            "silt"  => "/map/silt.map",
            "clay"  => "/map/clay.map",
            "phh2o" => "/map/phh2o.map",
            _ => throw new ArgumentOutOfRangeException(nameof(property), $"Unknown property: {property}")
        };
        string layer = $"{prop}_{depthLabel}_mean"; // ví dụ: sand_0-5cm_mean
        return (map, layer);
    }

    public static string BuildWmsGetFeatureInfoUrl(
        string baseWms,
        string mapPath,
        string layer,
        string bbox,
        int i, int j,
        int width = 256, int height = 256,
        bool useCrs84 = true)
    {
        // Theo “2OC.txt”: REQUEST=GetFeatureInfo, INFO_FORMAT=application/geo+json, CRS=CRS:84
        var sb = new StringBuilder();
        sb.Append(baseWms);
        sb.Append("?map=").Append(Uri.EscapeDataString(mapPath));
        sb.Append("&REQUEST=GetFeatureInfo");
        sb.Append("&QUERY_LAYERS=").Append(Uri.EscapeDataString(layer));
        sb.Append("&SERVICE=WMS&VERSION=1.3.0");
        sb.Append("&FORMAT=image%2Fpng&STYLES=&TRANSPARENT=TRUE");
        sb.Append("&LAYERS=").Append(Uri.EscapeDataString(layer));
        sb.Append("&INFO_FORMAT=application%2Fgeo%2Bjson");
        sb.Append("&I=").Append(i).Append("&J=").Append(j);
        sb.Append("&WIDTH=").Append(width).Append("&HEIGHT=").Append(height);
        sb.Append("&CRS=").Append(useCrs84 ? "CRS:84" : "EPSG:4326");
        sb.Append("&BBOX=").Append(bbox);
        return sb.ToString();
    }
    
    public static SoilDataResult FromWmsTriplets(IEnumerable<(string property, string depth, double value)> triplets)
        {
            if (triplets == null)
                throw new ArgumentNullException(nameof(triplets));

            // Bộ đệm cho 4 thuộc tính x 3 độ sâu
            var sand = new decimal?[3];
            var silt = new decimal?[3];
            var clay = new decimal?[3];
            var ph   = new decimal?[3];

            int ToDepthIndex(string depth) => depth switch
            {
                "0-5cm"   => 0,
                "5-15cm"  => 1,
                "15-30cm" => 2,
                _ => throw new ArgumentOutOfRangeException(nameof(depth), $"Unknown depth label: {depth}")
            };

            foreach (var (property, depth, value) in triplets)
            {
                var prop = property?.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(prop))
                    throw new ArgumentException("Property name is empty.", nameof(triplets));

                var idx = ToDepthIndex(depth?.Trim() ?? string.Empty);
                var dec = Convert.ToDecimal(value);

                switch (prop)
                {
                    case "sand":
                        sand[idx] = dec; break;
                    case "silt":
                        silt[idx] = dec; break;
                    case "clay":
                        clay[idx] = dec; break;
                    case "phh2o":
                        ph[idx] = dec; break;
                    default:
                        // Bỏ qua property lạ, hoặc bạn có thể throw nếu muốn cứng rắn hơn
                        break;
                }
            }

            // Kiểm tra đủ 12 mảnh dữ liệu (4 thuộc tính x 3 lớp)
            if (!(AllFilled(sand) && AllFilled(silt) && AllFilled(clay) && AllFilled(ph)))
            {
                throw new InvalidOperationException("Thiếu lớp dữ liệu (depth layer) từ nguồn soil – không đủ 12 mảnh. Hãy fallback sang CO2.");
            }

            // Tạo kết quả
            var result = new SoilDataResult
            {
                SandLayers = sand!.Select(v => v!.Value).ToList(),
                SiltLayers = silt!.Select(v => v!.Value).ToList(),
                ClayLayers = clay!.Select(v => v!.Value).ToList(),
                PhLayers   = ph!  .Select(v => v!.Value).ToList()
            };

            return result;

            static bool AllFilled(decimal?[] arr) => arr.All(x => x.HasValue);
        }

        /// <summary>
        /// Kiểm tra nhanh SoilDataResult đã có đủ 3 lớp cho mỗi thuộc tính hay chưa.
        /// </summary>
        public static bool HasAllLayers(SoilDataResult r)
        {
            if (r == null) return false;

            return r.SandLayers?.Count == 3
                && r.SiltLayers?.Count == 3
                && r.ClayLayers?.Count == 3
                && r.PhLayers?.Count   == 3;
        }

        /// <summary>
        /// Tính trung bình theo độ sâu cho 4 thuộc tính (không ghi vào model – chỉ trả kết quả).
        /// Dùng khi bạn cần giá trị trung bình nhưng không muốn thay đổi DTO hiện có.
        /// </summary>
        public static (decimal sandAvg, decimal siltAvg, decimal clayAvg, decimal phAvg) ComputeDepthAverages(SoilDataResult r)
        {
            if (r == null) throw new ArgumentNullException(nameof(r));
            if (!HasAllLayers(r))
                throw new InvalidOperationException("SoilDataResult chưa đủ 3 lớp cho mỗi thuộc tính.");

            decimal Avg(IReadOnlyList<decimal> xs) => xs.Count == 0 ? 0m : xs.Average();

            var sandAvg = Avg(r.SandLayers);
            var siltAvg = Avg(r.SiltLayers);
            var clayAvg = Avg(r.ClayLayers);
            var phAvg   = Avg(r.PhLayers);

            return (sandAvg, siltAvg, clayAvg, phAvg);
        }

}