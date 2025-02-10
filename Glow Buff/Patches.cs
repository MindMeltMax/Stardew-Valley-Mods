using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using SObject = StardewValley.Object;

namespace GlowBuff
{
    internal static class Patches
    {
        private static ModEntry context => ModEntry.Instance;
        private static LightSourceCache cache => context.Cache;

        private static FieldInfo? buffManagerPlayer;

        public static void Patch(string id)
        {
            Harmony harmony = new(id);

            harmony.Patch(
                original: AccessTools.PropertySetter(typeof(Character), nameof(Character.currentLocation)),
                prefix: new(typeof(Patches), nameof(Farmer_CurrentLocation_Prefix)),
                postfix: new(typeof(Patches), nameof(Farmer_CurrentLocation_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.Update)),
                postfix: new(typeof(Patches), nameof(Farmer_Update_Postfix))
            );

            // --Bandaid fix for SpaceCore compat-- \\
            // I hate this with a passion, but it works, so I'll keep it around for now ¯\_(ツ)_/¯
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.GetFoodOrDrinkBuffs)),
                postfix: new(AccessTools.Method(typeof(Patches), nameof(Object_GetFoodOrDrinkBuffs_Postfix_Pre_SpaceCore)), priority: Priority.VeryHigh)
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.GetFoodOrDrinkBuffs)),
                postfix: new(AccessTools.Method(typeof(Patches), nameof(Object_GetFoodOrDrinkBuffs_Postfix_Post_SpaceCore)), priority: Priority.VeryLow)
            );
            // --Bandaid fix for SpaceCore compat-- \\

            harmony.Patch(
                original: AccessTools.Method(typeof(BuffManager), nameof(BuffManager.Update)),
                postfix: new(typeof(Patches), nameof(BuffManager_Update_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(BuffManager), nameof(BuffManager.Apply)),
                postfix: new(typeof(Patches), nameof(BuffManager_Apply_Postfix))
            );

            harmony.Patch(
                original: AccessTools.FirstMethod(typeof(IClickableMenu), x => x.Name == nameof(IClickableMenu.drawHoverText) && x.GetParameters().Any(y => y.ParameterType == typeof(StringBuilder))), //This is still a bad way of obtaining the method, but I'm not writing out >20 param types
                transpiler: new(typeof(Patches), nameof(IClickableMenu_DrawHoverText_Transpiler))
            );
            buffManagerPlayer = typeof(BuffManager).GetField("Player", BindingFlags.Instance | BindingFlags.NonPublic);
            
            //While I have since removed the code which required their help the most, I still want to include thanks to these people for helping push out the original version of the mod:
            //Rokugin
            //Adradis / Audri
            //Khloe Leclair
            //Atravita
            //Abagaianye
            //Ichor
            //Without them, I probably wouldn't have ever shipped this mod to begin with. While it still might be a mess, it's a mess which I worked hard on, and I don't regret a single second spend on it.
        }

        // --Bandaid fix for SpaceCore compat-- \\
        private static IEnumerable<Buff> Object_GetFoodOrDrinkBuffs_Postfix_Pre_SpaceCore(IEnumerable<Buff> values, SObject __instance, ref Buff __state)
        {
            if (__state is null || !context.Helper.ModRegistry.IsLoaded("spacechase0.SpaceCore"))
                foreach (var buff in values)
                    if (Utils.IsGlowBuff(buff))
                        __state = buff;
            return values;
        }

        private static IEnumerable<Buff> Object_GetFoodOrDrinkBuffs_Postfix_Post_SpaceCore(IEnumerable<Buff> values, SObject __instance, ref Buff __state)
        {
            if (__state is null || values.Any(x => x.id == context.ModManifest.UniqueID + "/Glow" || x.customFields.ContainsKey(context.ModManifest.UniqueID + "/Glow")))
                return values;
            List<Buff> buffs = [.. values];
            buffs.Add(__state);
            return buffs;
        }
        // --Bandaid fix for SpaceCore compat-- \\

        private static void Farmer_CurrentLocation_Prefix(Character __instance, ref GameLocation __state) => __state = __instance.currentLocation;

        private static void Farmer_CurrentLocation_Postfix(Character __instance, GameLocation __state)
        {
            if (__instance is not Farmer f)
                return;
            cache.UpdateLocation(f, Game1.player.currentLocation, __state);
        }

        private static void Farmer_Update_Postfix(Farmer __instance, GameLocation location)
        {
            if (!cache.FarmerToLightSourceId.TryGetValue(__instance.UniqueMultiplayerID, out var id))
                return;
            cache.LightSourceIdToData.TryGetValue(id, out var data);
            if (data is not null && data.Prismatic)
                location.sharedLights[id].color.Value = Utility.GetPrismaticColor();
            Vector2 offset = __instance.shouldShadowBeOffset ? __instance.drawOffset : Vector2.Zero;
            location.repositionLightSource(id, new Vector2(__instance.Position.X + 36f, __instance.Position.Y) + offset);
        }

        private static void BuffManager_Update_Postfix(BuffManager __instance, GameTime time)
        {
            if (!Game1.shouldTimePass())
                return;
            if (buffManagerPlayer?.GetValue(__instance) is not Farmer owner)
                owner = Game1.player;

            cache.Tick(owner, time);
        }

        private static void BuffManager_Apply_Postfix(BuffManager __instance, Buff buff)
        {
            if (buff.id == context.ModManifest.UniqueID + "/Glow")
            {
                LightSourceData? data = Utils.TryGetLightDataFromBuff(buff);
                if (data is null)
                    return;

                if (buffManagerPlayer?.GetValue(__instance) is not Farmer owner)
                    owner = Game1.player;

                if (cache.FarmerToLightSourceId.TryGetValue(owner.UniqueMultiplayerID, out var id))
                    cache.RemoveLightSource(owner, id);

                string lightSourceId = context.ModManifest.UniqueID + owner.UniqueMultiplayerID.ToString();
                cache.CreateOrUpdateLightSource(owner, lightSourceId, data);
                return;
            }
            if (buff.id == context.ModManifest.UniqueID + "/Glow" || !buff.customFields.ContainsKey(context.ModManifest.UniqueID + "/Glow"))
                return;
            if (buff.customFields.TryGetValue(DataIds.DisplayName, out string? displayName)) 
                displayName = TokenParser.ParseText(displayName);
            if (buff.customFields.TryGetValue(DataIds.Description, out string? description)) 
                description = TokenParser.ParseText(description);
            Buff glowBuff = new(context.ModManifest.UniqueID + "/Glow", buff.source, buff.displaySource, buff.millisecondsDuration, displayName: displayName, description: description);
            glowBuff.customFields.TryAddMany(buff.customFields);
            __instance.Apply(glowBuff);
        }

        private static IEnumerable<CodeInstruction> IClickableMenu_DrawHoverText_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);

            matcher.Start().InsertAndAdvance([
                new(OpCodes.Ldarg_S, 9),
                new(OpCodes.Ldarga_S, 8),
                new(OpCodes.Call, AccessTools.Method(typeof(Utils), nameof(Utils.TryUpdateBuffIconArray)))
            ]);

            matcher.End().MatchEndBackwards([
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldc_I4_S),
                new(OpCodes.Bne_Un),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldsfld, AccessTools.Field(typeof(Game1), nameof(Game1.mouseCursors)))
            ]).Advance(-1).InsertAndAdvance([
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldloc_S, 5),
                new(OpCodes.Ldloca_S, 6),
                new(OpCodes.Ldarg_S, 9),
                new(OpCodes.Call, AccessTools.Method(typeof(Utils), nameof(Utils.TryDrawGlowBuffHoverIcon)))
            ]);

            matcher.End().MatchStartBackwards([
                new(OpCodes.Ldloc_2),
                new(OpCodes.Ldc_I4_4),
                new(OpCodes.Add),
                new(OpCodes.Stloc_2)
            ]).InsertAndAdvance([
                new(OpCodes.Ldarg_S, 9),
                new(OpCodes.Ldloca_S, 2),
                new(OpCodes.Call, AccessTools.Method(typeof(Utils), nameof(Utils.TryUpdateHoverBoxHeight)))
            ]).MatchStartForward([
                new(OpCodes.Stloc_1),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Add),
                new(OpCodes.Stloc_S)
            ]).Advance(1).InsertAndAdvance([
                new(OpCodes.Ldarg_S, 9),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldloc_S, 17),
                new(OpCodes.Ldloca_S, 1),
                new(OpCodes.Call, AccessTools.Method(typeof(Utils), nameof(Utils.TryUpdateHoverBoxWidth)))
            ]);

            return matcher.Instructions();
        }
    }
}
