using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using System.Text;

namespace CraftAnything
{
    internal static class Patches
    {
        public static void Patch(string id) //Fix hover box (why is it even fucked up?!?!?) \\Fixed
        {
            Harmony harmony = new(id);

            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.GetItemData)),
                prefix: new(typeof(Patches), nameof(CraftingRecipe_GetItemData_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.drawMenuView)),
                transpiler: new(typeof(Patches), nameof(CraftingRecipe_DrawMenuView_Transpiler))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingPage), "layoutRecipes"),
                transpiler: new(typeof(Patches), nameof(CraftingPage_LayoutRecipes_Transpiler))
            );

            harmony.Patch(
                original: AccessTools.FirstMethod(typeof(IClickableMenu), x => x.Name == nameof(IClickableMenu.drawHoverText) && x.GetParameters().ElementAt(1).ParameterType == typeof(StringBuilder)), //If you honestly believe I'm typing out all those params...
                prefix: new(typeof(Patches), nameof(IClickableMenu_DrawHoverText_Prefix))
            );
        }

        internal static bool CraftingRecipe_GetItemData_Prefix(CraftingRecipe __instance, bool useFirst, ref ParsedItemData __result)
        {
            try
            {
                if (!isValid(__instance, out var typeDef))
                    return true;
                string? str = useFirst ? __instance.itemToProduce.FirstOrDefault() : Game1.random.ChooseFrom(__instance.itemToProduce);
                __result = ItemRegistry.GetDataOrErrorItem(typeDef.Trim() + str);
                return false;
            }
            catch (Exception ex)
            {
                ModEntry.IMonitor.Log($"Failed Patching {nameof(CraftingRecipe.GetItemData)}", LogLevel.Error);
                ModEntry.IMonitor.Log($"[{nameof(CraftingRecipe_GetItemData_Prefix)}] {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
            }
            return true;
        }

        internal static IEnumerable<CodeInstruction> CraftingRecipe_DrawMenuView_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);

            matcher.Start().MatchStartForward([
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld),
                new(OpCodes.Brtrue_S),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Callvirt),
                new(OpCodes.Br_S),
                new(OpCodes.Ldstr, "(BC)"),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Callvirt),
                new(OpCodes.Call),
                new(OpCodes.Call),
                new(OpCodes.Dup)
            ]).Advance(1).RemoveInstructions(9).InsertAndAdvance([
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(getDataForDraw)))
            ]).Labels.Clear();

            return matcher.Instructions();
        }

        internal static IEnumerable<CodeInstruction> CraftingPage_LayoutRecipes_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);

            matcher.Start().MatchStartForward([
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldfld),
                new(OpCodes.Brtrue_S),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Br_S),
                new(OpCodes.Ldstr, "(BC)"),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Call),
                new(OpCodes.Call),
                new(OpCodes.Dup),
            ]).Advance(1).RemoveInstructions(7).InsertAndAdvance([
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(getDataForDraw)))
            ]).Labels.Clear();

            matcher.Start().MatchEndForward([
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.Method(typeof(CraftingPage), "craftingPageY")),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldc_I4_S),
                new(OpCodes.Mul),
                new(OpCodes.Add),
                new(OpCodes.Ldc_I4_S)
            ]);
            matcher.RemoveInstructions(7).InsertAndAdvance([
                new(OpCodes.Ldloc_S, 9),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(getComponentWidth))),
                new(OpCodes.Ldloc_S, 9),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(getComponentHeight))),
            ]).Labels.Clear();

            CodeInstruction startInsert = new(OpCodes.Ldloc_S, 6);

            matcher.Start().MatchEndForward([
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldloc_3),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldc_I4_1),
                new(OpCodes.Add),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Call),
                new(OpCodes.Ldloca_S)
            ]).Instruction.MoveLabelsTo(startInsert);
            matcher.InsertAndAdvance([
                startInsert,
                new(OpCodes.Ldloc_3),
                new(OpCodes.Ldloc_S, 4),
                new(OpCodes.Ldloc_S, 14),
                new(OpCodes.Ldloc_S, 9),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(setOccupiedSpace)))
            ]);

            return matcher.Instructions();
        }

        internal static void IClickableMenu_DrawHoverText_Prefix(ref Item hoveredItem, CraftingRecipe craftingIngredients)
        {
            if (craftingIngredients is null || !isValid(craftingIngredients))
                return;
            hoveredItem = null; //Because of weapon and boot icons, their hover boxes are too high, fix by setting the hoveredItem to null for custom crafting recipes
        }

        private static ParsedItemData getDataFor(string typeDef, string itemId) => ItemRegistry.GetDataOrErrorItem(typeDef.Trim() + itemId);

        private static string getDataForDraw(CraftingRecipe recipe)
        {
            string indexOfMenuView = recipe.getIndexOfMenuView();
            if (!isValid(recipe, out var typeDef))
                return recipe.bigCraftable ? "(BC)" + indexOfMenuView : indexOfMenuView;
            return typeDef + indexOfMenuView;
        }

        private static bool isValid(CraftingRecipe recipe, [NotNullWhen(true)] out string? typeDef)
        {
            typeDef = null;
            return !recipe.isCookingRecipe &&
                   CraftingRecipe.craftingRecipes.TryGetValue(recipe.name, out string? data) &&
                   ArgUtility.TryGet(data.Split('/'), 6, out typeDef, out _, false) &&
                   typeDef != ItemRegistry.type_object &&
                   typeDef != ItemRegistry.type_bigCraftable;
        }

        private static bool isValid(CraftingRecipe recipe) => isValid(recipe, out _);

        private static int getComponentWidth(CraftingRecipe recipe) => recipe.GetItemData().GetSourceRect().Width * 4;

        private static int getComponentHeight(CraftingRecipe recipe) => recipe.GetItemData().GetSourceRect().Height * 4;

        private static void setOccupiedSpace(ClickableTextureComponent[,] spaces, int x, int y, ClickableTextureComponent component, CraftingRecipe recipe)
        {
            if (recipe is null || !isValid(recipe))
                return;
            var sourceRect = recipe.GetItemData().GetSourceRect();
            for (int i = 0; i < sourceRect.Width / 16; i++)
                for (int j = 0; j < sourceRect.Height / 16; j++)
                    spaces[x + i, y + j] = component;
        }
    }
}
