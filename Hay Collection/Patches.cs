using HarmonyLib;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Reflection.Emit;

namespace HayCollection
{
    internal static class Patches
    {
        internal static void Patch(string id)
        {
            Harmony harmony = new(id);

            harmony.Patch(
                original: AccessTools.Method(typeof(Grass), nameof(Grass.TryDropItemsOnCut)),
                transpiler: new(typeof(Patches), nameof(Grass_TryDropItemsOnCut_Transpiler))
            );
        }

        internal static IEnumerable<CodeInstruction> Grass_TryDropItemsOnCut_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator); //My first time using a code matcher, please bear with me \\Figured it out now, still black magic but ¯\_(ツ)_/¯ Guess this means I'm a wizard now :D
            var meth = AccessTools.Method(typeof(Patches), nameof(tryAddHayToInventory));
            CodeInstruction nop = new(OpCodes.Nop);

            matcher.Start().MatchEndForward([
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(TerrainFeature), nameof(TerrainFeature.Location))),
                new(OpCodes.Call, AccessTools.Method(typeof(GameLocation), nameof(GameLocation.StoreHayInAnySilo)))
            ]).RemoveInstruction().InsertAndAdvance([new(OpCodes.Call, meth)]);

            matcher.End().MatchStartBackwards([
                new(OpCodes.Ldstr, "(O)178"),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Call),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldnull),
                new(OpCodes.Call),
                new(OpCodes.Call, AccessTools.Method(typeof(Game1), nameof(Game1.addHUDMessage)))
            ]).Instruction.MoveLabelsTo(nop);
            matcher.RemoveInstructions(9).Insert(nop);

            return matcher.Instructions();
        }

        internal static bool tryAddHayToInventory(int count, GameLocation currentLocation)
        {
            int remainder = GameLocation.StoreHayInAnySilo(count, currentLocation);
            if (remainder > 0 && Game1.player.addItemToInventory(ItemRegistry.Create("(O)178", remainder)) is { } item)
                Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection, Game1.player.currentLocation);
            return false;
        }
    }
}
