using Sounds_Patcher.Utility;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Linq;

namespace Sounds_Patcher.Patches
{
    public class CuePatches
    {
        private static Config config = ModEntry.StaticConfig;
        private static IMonitor monitor = ModEntry.StaticMonitor;

        public static bool Play_prefix(ICue __instance)
        {
            try
            {
                var attempted = __instance.Name;

                if (config.Sounds.Count > 0 && config.Sounds.Where(x => x.Key == attempted).FirstOrDefault().Key != null && config.Sounds.Where(x => x.Key == attempted).FirstOrDefault().Value) return false;
                else if (config.Songs.Count > 0 && config.Songs.Where(x => x.Key == attempted).FirstOrDefault().Key != null && config.Songs.Where(x => x.Key == attempted).FirstOrDefault().Value) return false;

                return true;
            }
            catch (Exception ex) { monitor.Log($"CuePatches : Something went wrong while trying to disable the sound {__instance.Name} - {ex.Message}"); return true; }
        }
    }
}
