using Newtonsoft.Json;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockChests.Utilities
{
    public class Config
    {
        public string OpenButton { get; set; } = "F1,LeftStick";

        [JsonIgnore]
        public IEnumerable<SButton> Open => ParseButtons(OpenButton);

        public bool LockOnTransfer { get; set; } = false;

        public Config() { }

        [JsonConstructor]
        public Config(string open, bool lockOnTransfer)
        {
            OpenButton = open;
            LockOnTransfer = lockOnTransfer;
        }

        private IEnumerable<SButton> ParseButtons(string btn)
        {
            List<SButton> open = new List<SButton>();
            string[] buttons = btn.Split(',');
            for (int i = 0; i < buttons.Length; i++)
                if (Enum.TryParse(buttons[i].Trim(), out SButton sButton))
                    open.Add(sButton);

            return open;
        }
    }
}
