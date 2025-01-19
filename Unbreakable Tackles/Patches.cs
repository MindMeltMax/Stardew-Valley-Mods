using StardewModdingAPI;
using StardewValley.Tools;
using System;
using HarmonyLib;
using Object = StardewValley.Object;

namespace UnbreakableTackles
{
    internal static class Patches
    {
        internal static void Patch(string id)
        {
            Harmony harmony = new(id);

            harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), "doDoneFishing"),
                prefix: new(typeof(Patches), nameof(doDoneFishingPrefix)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(doDoneFishingPostfix))
            );
        }

        internal static void doDoneFishingPrefix(FishingRod __instance, ref int __state)
        {
            try
            {
                if (__instance.GetBait() is Object bait && !ModEntry.IConfig.consumeBait)
                {
                    __state = bait.Stack;
                    __instance.GetBait().Stack++;
                }
            }
            catch (Exception ex)
            {
                ModEntry.IMonitor.Log($"Failed prefixing FishingRod.doDoneFishing", LogLevel.Error);
                ModEntry.IMonitor.Log($"{ex.GetType().FullName} - {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static void doDoneFishingPostfix(FishingRod __instance, ref int __state)
        {
            try
            {
                if (!ModEntry.IConfig.consumeBait && __state > 0 && __instance.GetBait() is Object bait && bait.Stack != __state)
                    __instance.GetBait().Stack = __state;
                for (int i = 0; i < __instance.attachments.Count; i++)
                {
                    var attachment = __instance.attachments[i];
                    if (attachment is null || attachment.Category != Object.tackleCategory && !attachment.HasContextTag("category_tackle") || attachment.uses.Value <= 0)
                        continue;
                    --__instance.attachments[i].uses.Value;
                }
            }
            catch (Exception ex) 
            { 
                ModEntry.IMonitor.Log($"Failed postfixing FishingRod.doDoneFishing", LogLevel.Error); 
                ModEntry.IMonitor.Log($"{ex.GetType().FullName} - {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
