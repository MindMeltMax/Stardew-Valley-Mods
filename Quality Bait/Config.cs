using Newtonsoft.Json;

namespace QualityBait
{
    internal class Config
    {
        public int ChancePercentage { get; set; } = 75;

        public bool BaitMakerQuality { get; set; } = true;

        public bool EnableBetterCraftingIntegration { get; set; } = true;

        public bool ForceLowerQuality { get; set; } = true;

        [JsonIgnore]
        public double Chance => ChancePercentage / 100.0;
    }
}
