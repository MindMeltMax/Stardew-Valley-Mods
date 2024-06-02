using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CraftAnything
{
    internal class ModEntry : Mod
    {
        internal static IMonitor IMonitor;

        public override void Entry(IModHelper helper)
        {
            IMonitor = Monitor;

            Helper.Events.GameLoop.GameLaunched += (_, _) => Patches.Patch(ModManifest.UniqueID);
        }
    }
}
