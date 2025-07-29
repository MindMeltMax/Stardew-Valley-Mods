using StardewValley.GameData.BigCraftables;

namespace ParrotPerch
{
    public record Data
    {
        public BigCraftableData Object { get; set; }

        public string Recipe { get; set; }
    }

    public record ModData
    {
        public int Level { get; set; }

        public int Seed { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public Dictionary<string, string> Metadata { get; set; }
    }
}
