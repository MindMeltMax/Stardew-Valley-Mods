using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace Fishnets.Patches
{
    internal static class Patcher
    {
        private static IModHelper helper;
        private static Harmony harmony;

        internal static void Patch(IModHelper helper)
        {
            Patcher.helper = helper;
            harmony = new(helper.ModRegistry.ModID);

            harmony.Patch(
                AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
                new(typeof(SObjectPatches), nameof(SObjectPatches.placementActionPrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Object), nameof(Object.canBePlacedInWater)),
                new(typeof(SObjectPatches), nameof(SObjectPatches.canBePlacedInWaterPrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Object), nameof(Object.canBePlacedHere)),
                new(typeof(SObjectPatches), nameof(SObjectPatches.canBePlacedHerePrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Object), nameof(Object.isPlaceable)),
                new(typeof(SObjectPatches), nameof(SObjectPatches.isPlaceablePrefix))
            );
        }
    }
}
