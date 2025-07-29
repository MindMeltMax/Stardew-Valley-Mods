using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects.Trinkets;
using StardewValley.TokenizableStrings;
using SObject = StardewValley.Object;

namespace ParrotPerch
{
    public class PerchInventoryHandler
    {
        private const string ParrotEggId = "(TR)ParrotEgg";

        private static bool IsShiftKeyDown => Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) || Game1.GetKeyboardState().IsKeyDown(Keys.RightShift);
        private static readonly Dictionary<string, float> squatTimerMap = [];
        private static readonly Dictionary<string, float> squawkTimerMap = [];
        private static readonly Dictionary<string, float> squawkDirectionMap = [];

        private static string Id => instance.Id;
        private static string ModDataPerchId => instance.ModManifest.UniqueID + "/PerchId";
        private static string ModDataId => instance.ModManifest.UniqueID + "/HeldItem";
        private static string SquawkMessageId => instance.ModManifest.UniqueID + "/Squawk";
        private static string SquatMessageId => instance.ModManifest.UniqueID + "/Squat";

        private static Texture2D parrotTexture;

        private static ModEntry instance;
        private static IModHelper helper;

        public static void Init(ModEntry entry)
        {
            instance = entry;
            helper = instance.Helper;
            parrotTexture = helper.GameContent.Load<Texture2D>("TileSheets\\companions");

            helper.Events.Content.AssetsInvalidated += onAssetInvalidated;
            helper.Events.World.ObjectListChanged += onObjectListChanged;
            helper.Events.Multiplayer.ModMessageReceived += onModMessageReceived;

            Harmony harmony = new(entry.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
                postfix: new(typeof(PerchInventoryHandler), nameof(Object_CheckForAction_Postfix))
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

        private static void onObjectListChanged(object? sender, ObjectListChangedEventArgs e)
        {
            if (!e.IsCurrentLocation)
                return;
            if (e.Removed.Any())
                foreach (var item in e.Removed)
                    if (GetHeldParrot(item.Value) is { } parrot)
                        Game1.createItemDebris(parrot, new(item.Key.X * 64 - 32, item.Key.Y * 64 - 32), Game1.player.FacingDirection, e.Location);
            if (e.Added.Any())
                foreach (var item in e.Added)
                    if (item.Value.QualifiedItemId == "(BC)" + Id && !item.Value.modData.ContainsKey(ModDataPerchId))
                        item.Value.modData[ModDataPerchId] = Guid.NewGuid().ToString();
        }

        private static void onModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != instance.ModManifest.UniqueID || e.FromPlayerID == Game1.player.UniqueMultiplayerID)
                return;
            foreach (var tile in Game1.currentLocation.Objects.Keys)
            {
                var obj = Game1.currentLocation.Objects[tile];
                if (obj.QualifiedItemId != "(BC)" + Id || obj.modData[ModDataPerchId] != e.ReadAs<string>())
                    continue;
                if (e.Type == SquawkMessageId)
                    doSquawk(obj);
                if (e.Type == SquatMessageId)
                    doSquat(obj);
                return;
            }
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
                if (IsShiftKeyDown)
                {
                    if (SetHeldParrot(__instance, null, out previous) && previous is not null)
                        Game1.createItemDebris(previous, new(__instance.TileLocation.X * 64 - 32, __instance.TileLocation.Y * 64 - 32), who.FacingDirection, who.currentLocation);
                    return;
                }
                if (__instance.modData.TryGetValue(ModDataId, out _))
                    broadcastSquawk(__instance);
            }
        }

        private static void Object_UpdateWhenCurrentLocation_Postfix(SObject __instance, GameTime time)
        {
            if (!__instance.modData.TryGetValue(ModDataId, out _) || !__instance.modData.TryGetValue(ModDataPerchId, out var perchId))
                return;
            if (squatTimerMap.ContainsKey(perchId))
            {
                if (squatTimerMap[perchId] <= 0)
                    squatTimerMap.Remove(perchId);
                else
                    squatTimerMap[perchId] -= (float)time.ElapsedGameTime.TotalMilliseconds;
            }
            if (squawkTimerMap.ContainsKey(perchId))
            {
                if (squawkTimerMap[perchId] <= 0)
                {
                    squawkTimerMap.Remove(perchId);
                    squawkDirectionMap.Remove(perchId);
                }
                else
                    squawkTimerMap[perchId] -= (float)time.ElapsedGameTime.TotalMilliseconds;
            }

            if (Game1.random.NextDouble() < (double)(.0005 * (1f / GetPlayersInLocation(__instance.Location))) && !squawkTimerMap.ContainsKey(perchId))
                broadcastSquawk(__instance);
            else if (Game1.random.NextDouble() < (double)(.0015 * (1f / GetPlayersInLocation(__instance.Location))) && !squatTimerMap.ContainsKey(perchId) && !squawkTimerMap.ContainsKey(perchId))
                broadcastSquat(__instance);
        }

        private static void Object_Draw_Postfix(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!__instance.modData.TryGetValue(ModDataId, out _) || !__instance.modData.TryGetValue(ModDataPerchId, out var perchId))
                return;
            Rectangle sourceRect = new(128, 160, 16, 16);
            SpriteEffects effects = SpriteEffects.None;
            if (squatTimerMap.ContainsKey(perchId))
                sourceRect = new((int)(squatTimerMap[perchId] % 1000) / 500 * 16 + 128, 160, 16, 16);
            if (squawkTimerMap.ContainsKey(perchId))
            {
                sourceRect = new(160, 160, 16, 16);
                if (squawkDirectionMap[perchId] == 1)
                    effects = SpriteEffects.FlipHorizontally;
            }
            spriteBatch.Draw(parrotTexture, __instance.getLocalPosition(Game1.viewport) + new Vector2(30 + (effects == SpriteEffects.FlipHorizontally ? 4 : 0), -68), sourceRect, Color.White * alpha, 0f, new(8f), 4f, effects, 7.77f);
        }

        public static Item? GetHeldParrot(SObject perch, bool remove = false)
        {
            if (perch is null || perch.QualifiedItemId != "(BC)" + Id ||
                !perch.modData.TryGetValue(ModDataId, out string modData))
                return null;
            var data = JsonConvert.DeserializeObject<ModData>(modData);
            if (data is null)
                return null;
            if (remove)
                SetHeldParrot(perch, null, out _);
            return CreateParrotEgg(data);
        }

        public static bool SetHeldParrot(SObject perch, Item? parrot, out Item? previousParrot)
        {
            previousParrot = null;
            if (perch is null || perch.QualifiedItemId != "(BC)" + Id)
                return false;
            var mutex = Game1.player.team.GetOrCreateGlobalInventoryMutex(Id);
            mutex.RequestLock();
            if (!mutex.IsLocked() || !mutex.IsLockHeld())
                return false;
            if (GetHeldParrot(perch) is { } held)
            {
                previousParrot = held;
                perch.modData.Remove(ModDataId);
            }
            parrot?.onDetachedFromParent();
            var modData = GetModData(parrot as Trinket);
            if (modData is not null)
            {
                perch.modData[ModDataId] = JsonConvert.SerializeObject(modData);
                mutex.ReleaseLock();
                return true;
            }
            mutex.ReleaseLock();
            return true;
        }

        private static Trinket CreateParrotEgg(ModData data)
        {
            var trinket = new Trinket(ParrotEggId.Split(')')[1], data.Seed);
            trinket.trinketMetadata.CopyFrom(data.Metadata);
            trinket.GetEffect().GeneralStat = data.Level;
            trinket.descriptionSubstitutionTemplates.Clear();
            trinket.descriptionSubstitutionTemplates.Add((data.Level + 1).ToString());
            trinket.descriptionSubstitutionTemplates.Add(TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:ParrotEgg_Chance_" + data.Level));
            trinket.modData.CopyFrom(data.Data);
            return trinket;
        }
        
        private static ModData? GetModData(Trinket? trinket)
        {
            if (trinket is null)
                return null;

            Dictionary<string, string> metadata = [];
            foreach (var item in trinket.trinketMetadata.Keys)
                metadata[item] = trinket.trinketMetadata[item];
            Dictionary<string, string> modData = [];
            foreach (var item in trinket.modData.Keys)
                modData[item] = trinket.modData[item];

            return new ModData()
            {
                Level = trinket.GetEffect().GeneralStat,
                Seed = trinket.generationSeed.Value,
                Data = modData,
                Metadata = metadata
            };
        }

        private static void doSquawk(SObject perch)
        {
            var id = perch.modData[ModDataPerchId];
            if (squawkTimerMap.ContainsKey(id))
                return;
            if (squatTimerMap.ContainsKey(id))
                squatTimerMap.Remove(id);
            squawkTimerMap[id] = 500f;
            squawkDirectionMap[id] = Game1.random.NextDouble() <= .25 ? 1 : 0;
            perch.Location.localSound("parrot_squawk");
        }

        private static void broadcastSquawk(SObject perch)
        {
            var id = perch.modData[ModDataPerchId];
            helper.Multiplayer.SendMessage(id, SquawkMessageId, [instance.ModManifest.UniqueID]);
            doSquawk(perch);
        }

        private static void doSquat(SObject perch)
        {
            var id = perch.modData[ModDataPerchId];
            squatTimerMap[id] = Game1.random.Next(2, 6) * 1000;
        }

        private static void broadcastSquat(SObject perch)
        {
            var id = perch.modData[ModDataPerchId];
            helper.Multiplayer.SendMessage(id, SquatMessageId, [instance.ModManifest.UniqueID]);
            doSquat(perch);
        }

        private static int GetPlayersInLocation(GameLocation location)
        {
            var players = Game1.getOnlineFarmers();
            int inLocation = 0;
            foreach (var player in players)
                if (player.currentLocation.NameOrUniqueName == location.NameOrUniqueName)
                    inLocation++;
            return inLocation;
        }
    }
}
