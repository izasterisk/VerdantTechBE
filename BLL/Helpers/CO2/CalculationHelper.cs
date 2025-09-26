namespace BLL.Helpers.CO2
{
    public static class CalculationHelper
    {
        /// <summary>
        /// Business logic: Calculate weighted average for soil properties across depth layers
        /// Formula: (layer0-5 × 5 + layer5-15 × 10 + layer15-30 × 15) ÷ 30
        /// </summary>
        public static decimal CalculateWeightedAverage(decimal layer0_5, decimal layer5_15, decimal layer15_30)
        {
            return (layer0_5 * 5 + layer5_15 * 10 + layer15_30 * 15) / 30;
        }

        /// <summary>
        /// Business logic: Calculate average from weather data list, filtering out null values
        /// </summary>
        /// <param name="values">List of nullable decimal values from weather API</param>
        /// <returns>Average of non-null values, or 0 if all values are null</returns>
        public static decimal CalculateAverage(IEnumerable<decimal?> values)
        {
            var validValues = values.Where(x => x.HasValue).Select(x => x!.Value).ToList();

            if (!validValues.Any())
            {
                return 0;
            }

            return validValues.Average();
        }

        /// <summary>
        /// Business logic: Calculate historical weather averages from simple arrays
        /// </summary>
        public static (decimal precipitationAvg, decimal et0Avg) CalculateHistoricalWeatherAverages(decimal?[] precipitationData, decimal?[] et0Data)
        {
            var precipitationAvg = CalculateAverage(precipitationData);
            var et0Avg = CalculateAverage(et0Data);

            // Business validation: check if location is supported
            if (precipitationAvg == 0 && et0Avg == 0)
            {
                throw new InvalidOperationException("Địa chỉ này chưa được hỗ trợ, vui lòng thử địa chỉ khác.");
            }

            return (precipitationAvg, et0Avg);
        }

        // ============================================================
        // CO2e CALCULATION (Tier-2-lite, EF fix cứng cho Việt Nam)
        // Không phụ thuộc DAL/DTO: nhận đủ từng trường một.
        // ============================================================

        /// <summary>
        /// Tính tổng CO2e (kg) trong 1 lần từ toàn bộ dữ liệu đầu vào (không phụ thuộc DAL/DTO).
        /// EF fix cứng (VN grid 2023 + cấu trúc IPCC 2006/2019; đủ để gọi là Tier 2 nếu EF là quốc gia).
        /// </summary>
        /// <param name="SandPct">(% cát, 0–100)</param>
        /// <param name="SiltPct">(% limon, 0–100)</param>
        /// <param name="ClayPct">(% sét, 0–100)</param>
        /// <param name="Phh2o">pH(H2O)</param>
        /// <param name="PrecipitationSum">Tổng mưa (mm) trong kỳ đo</param>
        /// <param name="Et0FaoEvapotranspiration">ET0 FAO (mm) trong kỳ đo</param>
        /// <param name="ElectricityKwh">Điện tiêu thụ (kWh)</param>
        /// <param name="GasolineLiters">Xăng (lít)</param>
        /// <param name="DieselLiters">Dầu diesel (lít)</param>
        /// <param name="OrganicFertilizer">Phân hữu cơ (kg)</param>
        /// <param name="NpkFertilizer">Phân NPK (kg)</param>
        /// <param name="UreaFertilizer">Phân urê (kg)</param>
        /// <param name="PhosphateFertilizer">Phân lân (kg)</param>
        /// <returns>Tổng CO2e (kg)</returns>
        public static decimal ComputeCo2Footprint(
            decimal? SandPct,
            decimal? SiltPct,
            decimal? ClayPct,
            decimal? Phh2o,
            decimal? PrecipitationSum,
            decimal? Et0FaoEvapotranspiration,
            decimal? ElectricityKwh,
            decimal? GasolineLiters,
            decimal? DieselLiters,
            decimal? OrganicFertilizer,
            decimal? NpkFertilizer,
            decimal? UreaFertilizer,
            decimal? PhosphateFertilizer
        )
        {
            // ===========================
            // EMISSION FACTORS (hard-coded)
            // ===========================
            // VN grid 2023: 0.6592 tCO2/MWh = 0.6592 kgCO2/kWh
            const decimal EF_Electricity_KgPerKWh = 0.6592m;

            // Fuel CO2 (combustion)
            const decimal EF_Gasoline_KgPerL = 2.31m;
            const decimal EF_Diesel_KgPerL   = 2.68m;

            // Soil N2O method (IPCC 2006 + 2019 Refinement)
            const decimal EF1_Direct_N2O_N        = 0.01m;   // fraction of N applied → N2O–N (direct)
            const decimal FracGASF_Volatilization = 0.10m;   // fraction of N volatilized
            const decimal EF4_Deposition_N2O_N    = 0.01m;   // N2O–N from atmospheric deposition
            const decimal FracLEACH_Runoff        = 0.24m;   // fraction of N leached/runoff
            const decimal EF5_Leached_N2O_N       = 0.0075m; // N2O–N from leached N
            const decimal N2O_N_to_N2O            = 44m / 28m;
            const decimal GWP100_N2O              = 273m;    // IPCC AR6

            // Nitrogen content by fertilizer type (mass fraction of N)
            const decimal Npct_Organic   = 0.015m; // 1.5%
            const decimal Npct_Npk       = 0.16m;  // assume NPK ≈ 16% N
            const decimal Npct_Urea      = 0.46m;  // Urea 46% N
            const decimal Npct_Phosphate = 0.00m;  // ~0% N

            // Leaching toggle heuristics (dùng dữ liệu environment hiện có)
            const decimal PRECIP_MM_LEACH_THRESHOLD = 100m;
            const decimal SAND_HIGH_PCT             = 60m;
            const decimal CLAY_LOW_PCT              = 20m;

            // ===== Extract activity data with safe defaults =====
            var elecKwh = ElectricityKwh   ?? 0m;
            var gasL    = GasolineLiters   ?? 0m;
            var dieL    = DieselLiters     ?? 0m;

            var mOrg  = OrganicFertilizer  ?? 0m;
            var mNpk  = NpkFertilizer      ?? 0m;
            var mUrea = UreaFertilizer     ?? 0m;
            var mPhos = PhosphateFertilizer?? 0m;

            var precip = PrecipitationSum;
            var sand   = SandPct;
            var clay   = ClayPct;
            // Phh2o & Et0FaoEvapotranspiration hiện chưa tham gia công thức Tier-1/2-lite (có thể mở rộng sau)

            // ===== 1) Energy CO2e =====
            var co2eEnergy =
                (elecKwh * EF_Electricity_KgPerKWh) +
                (gasL   * EF_Gasoline_KgPerL) +
                (dieL   * EF_Diesel_KgPerL);

            // ===== 2) Soil N2O (direct + indirect) =====
            // Total N applied (kg N)
            var N_applied =
                (mOrg  * Npct_Organic) +
                (mNpk  * Npct_Npk) +
                (mUrea * Npct_Urea) +
                (mPhos * Npct_Phosphate);

            if (N_applied < 0m) N_applied = 0m;

            // Direct N2O–N
            var N2O_N_direct = N_applied * EF1_Direct_N2O_N;

            // Indirect via volatilization & deposition
            var N2O_N_vol = N_applied * FracGASF_Volatilization * EF4_Deposition_N2O_N;

            // Indirect via leaching/runoff (bật theo heuristic)
            var leachOn =
                (precip.HasValue && precip.Value >= PRECIP_MM_LEACH_THRESHOLD) ||
                (sand.HasValue && sand.Value >= SAND_HIGH_PCT &&
                 (!clay.HasValue || clay.Value <= CLAY_LOW_PCT));

            var fracLeachEff = leachOn ? FracLEACH_Runoff : 0m;
            var N2O_N_leach  = N_applied * fracLeachEff * EF5_Leached_N2O_N;

            // Convert N2O–N → N2O → CO2e
            var N2O_total = (N2O_N_direct + N2O_N_vol + N2O_N_leach) * N2O_N_to_N2O;
            var co2eN2O   = N2O_total * GWP100_N2O;

            // ===== 3) Total =====
            var total = co2eEnergy + co2eN2O;

            // Optional: round to 2 decimals for storage/UX
            return Math.Round(total, 2, MidpointRounding.AwayFromZero);
        }
    }
}