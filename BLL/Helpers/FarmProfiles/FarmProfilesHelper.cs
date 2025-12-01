using DAL.Data;

namespace BLL.Helpers.FarmProfiles;

public class FarmProfilesHelper
{
    // Tối ưu: Khởi tạo Dictionary một lần duy nhất (Static)
    private static readonly Dictionary<PlantingMethod, List<CropType>> InvalidPlantingCropCombinations = new()
    {
        // Rau củ (rễ cọc/củ) không nên ươm khay hoặc cấy vì dễ bị rễ cong, chẻ củ
        { 
            PlantingMethod.TrayNursery, 
            new List<CropType> { CropType.RootVegetable } 
        },
        { 
            PlantingMethod.Transplanting, 
            new List<CropType> { CropType.RootVegetable } 
        },
        
        // Sinh sản sinh dưỡng (củ, ngó) thường không dùng cho rau ăn lá (trừ 1 số loại đặc biệt đã tính vào giâm cành)
        // Lưu ý: Dâu tây (Fruiting) dùng sinh dưỡng, nhưng ở đây tạm thời chặn để đơn giản hóa nếu cần
        { 
            PlantingMethod.VegetativePropagation, 
            new List<CropType> { CropType.LeafyGreen } 
        },

        // Giâm cành:
        // - Cho phép: Herb (Rau thơm), LeafyGreen (Rau muống, ngót, lang...)
        // - Chặn: RootVegetable (Củ), Fruiting (Cà chua/Bầu bí thường gieo hạt, dù cà chua có thể giâm cành nhưng ít dùng thương mại)
        { 
            PlantingMethod.Cutting, 
            new List<CropType> { CropType.RootVegetable, CropType.Fruiting } 
        }
    };

    public static void ValidateCropCombination(
        PlantingMethod plantingMethod, 
        CropType cropType, 
        FarmingType farmingType)
    {
        ValidatePlantingMethodAndCropType(plantingMethod, cropType);
        ValidatePlantingMethodAndFarmingType(plantingMethod, farmingType);
        ValidateCropTypeAndFarmingType(cropType, farmingType);
    }

    private static void ValidatePlantingMethodAndCropType(PlantingMethod plantingMethod, CropType cropType)
    {
        if (InvalidPlantingCropCombinations.TryGetValue(plantingMethod, out var invalidCropTypes))
        {
            if (invalidCropTypes.Contains(cropType))
            {
                throw new ArgumentException(
                    $"Phương pháp trồng '{GetPlantingMethodDisplay(plantingMethod)}' " +
                    $"không phù hợp với loại cây trồng '{GetCropTypeDisplay(cropType)}'.");
            }
        }
    }

    private static void ValidatePlantingMethodAndFarmingType(PlantingMethod plantingMethod, FarmingType farmingType)
    {
        // Thủy canh bắt buộc phải có giá thể (ươm khay/cấy), không gieo hạt trực tiếp vào nước
        if (plantingMethod == PlantingMethod.DirectSeeding && 
            farmingType == FarmingType.Hydroponics)
        {
            throw new ArgumentException(
                $"Phương pháp trồng '{GetPlantingMethodDisplay(plantingMethod)}' " +
                $"không thể áp dụng cho '{GetFarmingTypeDisplay(farmingType)}'.");
        }
    }

    // Mới thêm: Logic check Loại cây vs Kiểu canh tác
    private static void ValidateCropTypeAndFarmingType(CropType cropType, FarmingType farmingType)
    {
        // Rau lấy củ (Cà rốt, khoai tây) thường không trồng thủy canh (trừ khí canh - Aeroponics, nhưng enum chưa có)
        if (cropType == CropType.RootVegetable && farmingType == FarmingType.Hydroponics)
        {
             throw new ArgumentException(
                $"Loại cây '{GetCropTypeDisplay(cropType)}' thường không phù hợp với " +
                $"mô hình '{GetFarmingTypeDisplay(farmingType)}' (dễ gây thối củ/rễ).");
        }
    }

    // Các hàm GetDisplay giữ nguyên như cũ
    private static string GetPlantingMethodDisplay(PlantingMethod method) => method switch
    {
        PlantingMethod.DirectSeeding => "Gieo hạt trực tiếp",
        PlantingMethod.TrayNursery => "Ươm trong khay",
        PlantingMethod.Transplanting => "Cấy cây con",
        PlantingMethod.VegetativePropagation => "Sinh sản sinh dưỡng",
        PlantingMethod.Cutting => "Giâm cành",
        _ => method.ToString()
    };

    private static string GetCropTypeDisplay(CropType type) => type switch
    {
        CropType.LeafyGreen => "Rau ăn lá",
        CropType.Fruiting => "Rau ăn quả",
        CropType.RootVegetable => "Rau củ",
        CropType.Herb => "Rau thơm",
        _ => type.ToString()
    };

    private static string GetFarmingTypeDisplay(FarmingType type) => type switch
    {
        FarmingType.Intensive => "Thâm canh",
        FarmingType.CropRotation => "Luân canh",
        FarmingType.Intercropping => "Xen canh",
        FarmingType.Greenhouse => "Nhà lưới/nhà màng",
        FarmingType.Hydroponics => "Thủy canh",
        _ => type.ToString()
    };
}