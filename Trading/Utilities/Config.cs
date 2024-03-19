using StardewModdingAPI;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI.Utilities;

namespace Trading.Utilities
{
    public class Config
    {
        public KeybindList TradeMenuButton { get; set; } = new(new Keybind(SButton.G), new(SButton.LeftStick));

        //public string TradeMenuButton { get; set; } = "G, LeftStick";

        public bool Global { get; set; }

        /*[JsonIgnore]
        public IEnumerable<SButton> TradeMenuSButton => ParseButtons(TradeMenuButton);

        private IEnumerable<SButton> ParseButtons(string btn)
        {
            List<SButton> open = new();
            string[] buttons = btn.Split(',');
            for (int i = 0; i < buttons.Length; i++)
                if (Enum.TryParse(buttons[i].Trim(), out SButton sButton))
                    open.Add(sButton);

            return open;
        }*/
    }
}
