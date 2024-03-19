using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StardewModdingAPI;

namespace ChestDisplays.Utility
{
    public class Config
    {
        public string ChangeItemKey { get; set; } = "OemQuotes, LeftStick";

        public float ItemScale { get; set; } = 0.42f;

        public float Transparency { get; set; } = 1f;

        public bool DisplayQuality { get; set; } = true;

        public bool ShowFirstIfNoneSelected { get; set; } = true;

        [JsonIgnore]
        public IEnumerable<SButton> ChangeItemButtons => Utils.ParseSButton(ChangeItemKey);
    }
}
