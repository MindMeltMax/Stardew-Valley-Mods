using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;

namespace ParrotPerch
{
    public class PerchInventoryHandler
    {
        private const string ParrotEggId = "(TR)ParrotEgg";

        private static string Id => instance.Id;
        private static string InventoryId => instance.ModManifest.UniqueID + ".InventoryId";

        private static bool IsShiftKeyDown => Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) || Game1.GetKeyboardState().IsKeyDown(Keys.RightShift);
        private static readonly Dictionary<string, float> squatTimerMap = [];
        private static readonly Dictionary<string, float> squawkTimerMap = [];
        private static readonly Dictionary<string, float> squawkDirectionMap = [];

        private static Texture2D parrotTexture;

        private static ModEntry instance;
        private static IModHelper helper;

        public static void Init(ModEntry entry)
        {
            instance = entry;
            helper = entry.Helper;
            parrotTexture = helper.GameContent.Load<Texture2D>("TileSheets\\companions");

            helper.Events.Content.AssetsInvalidated += onAssetInvalidated;

            Harmony harmony = new(entry.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                postfix: new(typeof(PerchInventoryHandler), nameof(Object_CheckForAction_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.performToolAction)),
                postfix: new(typeof(PerchInventoryHandler), nameof(Object_PerformToolAction_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.updateWhenCurrentLocation)),
                postfix: new(typeof(PerchInventoryHandler), nameof(Object_UpdateWhenCurrentLocation_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.draw), [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]),
                postfix: new(typeof(PerchInventoryHandler), nameof(Object_Draw_Postfix))
            );
        }

        private static void onAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo("TileSheets\\companions")))
                parrotTexture = helper.GameContent.Load<Texture2D>("TileSheets\\companions");
        }

        public static Item? GetHeldParrot(SObject perch)
        {
            if (perch is null || perch.QualifiedItemId != "(BC)" + Id || 
                !perch.modData.TryGetValue(InventoryId, out string id))
                return null;
            var inv = Game1.player.team.GetOrCreateGlobalInventory(id);
            var item = inv.GetById(ParrotEggId);
            return item.FirstOrDefault();
        }

        public static bool SetHeldParrot(SObject perch, Item parrot, out Item? previousParrot)
        {
            previousParrot = null;
            if (perch is null || perch.QualifiedItemId != "(BC)" + Id)
                return false;
            if (!perch.modData.TryGetValue(InventoryId, out string id))
                perch.modData[InventoryId] = id = instance.ModManifest.UniqueID + "_" + Guid.NewGuid().ToString();
            var inventory = Game1.player.team.GetOrCreateGlobalInventory(id);
            var mutex = Game1.player.team.GetOrCreateGlobalInventoryMutex(id);

            mutex.RequestLock();
            if (mutex.IsLocked() && !mutex.IsLockHeld())
                return false;
            if (GetHeldParrot(perch) is { } previous)
            {
                inventory.Remove(previous);
                previous.onDetachedFromParent();
                previousParrot = previous;
            }
            if (parrot is not null)
            {
                parrot.onDetachedFromParent();
                inventory.Add(parrot);
            }
            mutex.ReleaseLock();
            return true;
        }

        private static void Object_CheckForAction_Postfix(SObject __instance, Farmer who, bool justCheckingForActivity)
        {
            if (justCheckingForActivity || __instance is null || __instance.QualifiedItemId != "(BC)" + Id)
                return;
            if (who.ActiveItem?.QualifiedItemId == ParrotEggId && Game1.didPlayerJustRightClick() && SetHeldParrot(__instance, who.ActiveItem, out var previous))
            {
                who.removeItemFromInventory(who.ActiveItem);
                if (previous is not null)
                    Game1.createItemDebris(previous, who.getStandingPosition(), who.FacingDirection, who.currentLocation);
                return;
            }
            if (who.ActiveItem?.QualifiedItemId != ParrotEggId && Game1.didPlayerJustRightClick())
            {
                if (IsShiftKeyDown && SetHeldParrot(__instance, null, out previous))
                {
                    if (previous is not null)
                        Game1.createItemDebris(previous, who.getStandingPosition(), who.FacingDirection, who.currentLocation);
                    return;
                }
                if (__instance.modData.TryGetValue(InventoryId, out var id))
                {
                    squawk(__instance, id);
                }
            }
        }

        private static void Object_PerformToolAction_Postfix(SObject __instance)
        {
            if (GetHeldParrot(__instance) is { } parrot)
                Game1.createItemDebris(parrot, Game1.player.getStandingPosition(), Game1.player.FacingDirection, Game1.player.currentLocation);
        }

        private static void Object_UpdateWhenCurrentLocation_Postfix(SObject __instance, GameTime time)
        {
            if (GetHeldParrot(__instance) is not { } parrot || !__instance.modData.TryGetValue(InventoryId, out string id))
                return;
            if (squatTimerMap.ContainsKey(id))
            {
                if (squatTimerMap[id] <= 0)
                    squatTimerMap.Remove(id);
                else
                    squatTimerMap[id] -= (float)time.ElapsedGameTime.TotalMilliseconds;
            }
            if (squawkTimerMap.ContainsKey(id))
            {
                if (squawkTimerMap[id] <= 0)
                {
                    squawkTimerMap.Remove(id);
                    squawkDirectionMap.Remove(id);
                }
                else
                    squawkTimerMap[id] -= (float)time.ElapsedGameTime.TotalMilliseconds;
            }

            if (Game1.random.NextDouble() < 0.0005 && !squawkTimerMap.ContainsKey(id))
                squawk(__instance, id);
            else if (Game1.random.NextDouble() < 0.0015 && !squatTimerMap.ContainsKey(id) && !squawkTimerMap.ContainsKey(id))
                squatTimerMap[id] = Game1.random.Next(2, 6) * 1000;
        }

        private static void Object_Draw_Postfix(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (GetHeldParrot(__instance) is null || !__instance.modData.TryGetValue(InventoryId, out var id))
                return;
            Rectangle sourceRect = new(128, 160, 16, 16);
            SpriteEffects effects = SpriteEffects.None;
            if (squatTimerMap.ContainsKey(id))
                sourceRect = new((int)(squatTimerMap[id] % 1000) / 500 * 16 + 128, 160, 16, 16);
            if (squawkTimerMap.ContainsKey(id))
            {
                sourceRect = new(160, 160, 16, 16);
                if (squawkDirectionMap[id] == 1)
                    effects = SpriteEffects.FlipHorizontally;
            }
            spriteBatch.Draw(parrotTexture, __instance.getLocalPosition(Game1.viewport) + new Vector2(30 + (effects == SpriteEffects.FlipHorizontally ? 4 : 0), -68), sourceRect, Color.White * alpha, 0f, new(8f), 4f, effects, 7.77f);
        }

        private static void squawk(SObject obj, string id)
        {
            if (squawkTimerMap.ContainsKey(id))
                return;
            if (squatTimerMap.ContainsKey(id))
                squatTimerMap.Remove(id);
            squawkTimerMap[id] = 500f;
            squawkDirectionMap[id] = Game1.random.NextDouble() <= .25 ? 1 : 0;
            obj.Location.localSound("parrot_squawk");
        }
    }
}
