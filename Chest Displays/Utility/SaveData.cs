using StardewValley;

namespace Chest_Displays.Utility
{
    public class SaveData
    {
        public string Item { get; set; }
        public string ItemDescription { get; set; }
        public int ItemQuality { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Location { get; set; }

        public SaveData() { }

        public SaveData(string item, string descript, int quality, int x, int y, string location)
        {
            Item = item;
            ItemDescription = descript;
            ItemQuality = quality;
            X = x;
            Y = y;
            Location = location;
        }
    }
}
