using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chest_Displays.Utility
{
    public class Config
    {
        public string ChangeItemKey { get; set; } = "Quotes";

        public float ItemScale { get; set; } = 0.5f;

        public float Transparency { get; set; } = 1f;

        public bool DisplayQuality { get; set; } = true;

        public bool RetainItem { get; set; } = false;

        public Config() { }

        public Config(string key, float scale, float ghost, bool quality, bool keeper)
        {
            ChangeItemKey = key;
            ItemScale = scale;
            Transparency = ghost;
            DisplayQuality = quality;
            RetainItem = keeper;
        }
    }
}
