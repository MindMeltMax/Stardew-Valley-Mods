using Harmony;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unbreakable_Tackles.Harmony
{
    public class Patcher
    {
        public static void Init()
        {
            HarmonyInstance harmony = HarmonyInstance.Create(ModEntry.IHelper.ModRegistry.ModID);

            if (ModEntry.IConfig.consumeBait)
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(FishingRod), "doDoneFishing", new[] { typeof(bool) }),
                    postfix: new HarmonyMethod(typeof(FishingRodPatches), nameof(FishingRodPatches.doDoneFishing_postfix))
                );
            }
            else
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.doneFishing), new[] { typeof(Farmer), typeof(bool) }),
                    prefix: new HarmonyMethod(typeof(FishingRodPatches), nameof(FishingRodPatches.doneFishing_prefix))
                );
            }
        }
    }
}
