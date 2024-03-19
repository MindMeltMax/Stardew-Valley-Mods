using StardewValley.GameData.BigCraftables;
using StardewValley.GameData.Shops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PiggyBank.Data
{
    public class ObjectInformation
    {
        public string Id { get; set; }

        public BigCraftableData Object { get; set; }

        public string Recipe { get; set; }

        public ShopItemData ShopItem { get; set; }
    }
}
