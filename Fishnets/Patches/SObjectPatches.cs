using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using Object = StardewValley.Object;

namespace Fishnets.Patches
{
    internal static class SObjectPatches
    {
        private static IMonitor IMonitor => ModEntry.IMonitor;

        public static bool isPlaceablePrefix(Object __instance, ref bool __result)
        {
            try
            {
                if (__instance.ParentSheetIndex == ModEntry.FishNetId)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
            catch (Exception ex) { IMonitor.Log($"Faild patching {nameof(Object.isPlaceable)}", LogLevel.Error); IMonitor.Log($"{ex.Message}\n{ex.StackTrace}"); return true; }
        }

        public static bool canBePlacedHerePrefix(Object __instance, GameLocation l, Vector2 tile, ref bool __result)
        {
            try
            {
                if (__instance.ParentSheetIndex == ModEntry.FishNetId)
                {
                    __result = FishNet.IsValidPlacementLocation(l, (int)tile.X, (int)tile.Y);
                    return false;
                }
                return true;
            }
            catch (Exception ex) { IMonitor.Log($"Faild patching {nameof(Object.canBePlacedHere)}", LogLevel.Error); IMonitor.Log($"{ex.Message}\n{ex.StackTrace}"); return true; }
        }

        public static bool canBePlacedInWaterPrefix(Object __instance, ref bool __result)
        {
            try
            {
                if (__instance.ParentSheetIndex == ModEntry.FishNetId)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
            catch (Exception ex) { IMonitor.Log($"Faild patching {nameof(Object.canBePlacedInWater)}", LogLevel.Error); IMonitor.Log($"{ex.Message}\n{ex.StackTrace}"); return true; }
        }
        
        public static bool placementActionPrefix(Object __instance, GameLocation location, int x, int y, Farmer who = null)
        {
            try
            {
                if (__instance.ParentSheetIndex == ModEntry.FishNetId)
                {
                    if (!FishNet.IsValidPlacementLocation(location, x / 64, y / 64))
                        return true;
                    new FishNet(new Vector2(x / 64f, y / 64f)).placementAction(location, x, y, who);
                    __instance.Stack--;
                    if (__instance.Stack <= 0)
                        Game1.player.removeItemFromInventory(__instance);
                    return false;
                }

                return true;
            }
            catch(Exception ex) { IMonitor.Log($"Faild patching {nameof(Object.placementAction)}", LogLevel.Error); IMonitor.Log($"{ex.Message}\n{ex.StackTrace}"); return true; }
        }
    }
}
