using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FairyFix
{
    public class Config
    {
        public SelectionMode SelectMode { get; set; } = SelectionMode.ConnectedSameCrop;

        public bool ReviveDeadCrops { get; set; } = true;

        public bool ResetOnSeasonChange { get; set; } = true;

        public KeybindList ToggleButton { get; set; } = new(SButton.U);
    }
}
