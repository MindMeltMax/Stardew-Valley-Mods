using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterPigs
{
    internal class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Helper.Events.GameLoop.GameLaunched += (s, e) => Patches.Patch(Monitor, Helper);
        }
    }
}
