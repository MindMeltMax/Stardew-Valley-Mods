using StardewValley.GameData.BigCraftables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParrotPerch
{
    public record Data
    {
        public BigCraftableData Object { get; set; }

        public string Recipe { get; set; }
    }
}
