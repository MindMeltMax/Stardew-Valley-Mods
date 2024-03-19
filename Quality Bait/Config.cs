using Newtonsoft.Json;

namespace QualityBait
{
    internal class Config
    {
        public int ChancePercentage { get; set; } = 75;

        [JsonIgnore]
        public double Chance => ChancePercentage / 100.0;
    }
}
