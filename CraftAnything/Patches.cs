using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using System.Text;

namespace CraftAnything
{
    internal static class Patches
    {
        public static void Patch(string id)
        {
            Harmony harmony = new(id);

            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingPage), "layoutRecipes"),
                transpiler: new(typeof(Patches), nameof(CraftingPage_LayoutRecipes_Transpiler))
            );

            harmony.Patch(
                original: AccessTools.FirstMethod(typeof(IClickableMenu), x => x.Name == nameof(IClickableMenu.drawHoverText) && x.GetParameters().ElementAt(1).ParameterType == typeof(StringBuilder)), //If you honestly believe I'm typing out all those params...
                prefix: new(typeof(Patches), nameof(IClickableMenu_DrawHoverText_Prefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingPage), nameof(CraftingPage.draw), [typeof(SpriteBatch)]),
                transpiler: new(typeof(Patches), nameof(CraftingPage_Draw_Transpiler))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.drawRecipeDescription)),
                transpiler: new(typeof(Patches), nameof(CraftingRecipe_DrawRecipeDescription_Transpiler))
            );
        }

        internal static IEnumerable<CodeInstruction> CraftingPage_LayoutRecipes_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);

            matcher.Start().MatchEndForward([
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.Method(typeof(CraftingPage), "craftingPageY")),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldc_I4_S),
                new(OpCodes.Mul),
                new(OpCodes.Add),
                new(OpCodes.Ldc_I4_S)
            ]).RemoveInstructions(7).InsertAndAdvance([
                new(OpCodes.Ldloc_S, 9),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(getComponentWidth))),
                new(OpCodes.Ldloc_S, 9),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(getComponentHeight)))
            ]);

            matcher.Start().MatchStartForward([
                new(OpCodes.Ldc_I4, 200),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Add),
                new(OpCodes.Stloc_S)
            ]).InsertAndAdvance([
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldloca_S, 6),
                new(OpCodes.Ldloca_S, 2),
                new(OpCodes.Ldloca_S, 3),
                new(OpCodes.Ldloca_S, 4),
                new(OpCodes.Ldloc_S, 9),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(createNewPageIfNeeded)))
            ]);

            CodeInstruction startInsert = new(OpCodes.Ldloca_S, 6);

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
                new(OpCodes.Ldloc_S, 13),
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

        internal static IEnumerable<CodeInstruction> CraftingPage_Draw_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);

            CodeInstruction startInsert = new(OpCodes.Ldarg_0);

            matcher.Start().MatchStartForward([
                new(OpCodes.Ldloca_S),
                new(OpCodes.Call),
                new(OpCodes.Brtrue),
                new(OpCodes.Leave_S),
            ]).CreateLabel(out var l1);

            matcher.Start().MatchStartForward([
                new(OpCodes.Ldloc_2),
                new(OpCodes.Ldfld),
                new(OpCodes.Ldstr, "ghosted"),
                new(OpCodes.Callvirt),
                new(OpCodes.Brfalse_S),
            ]).CreateLabel(out var l2);
            matcher.InsertAndAdvance([
                startInsert,
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldloc_2),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(drawnOverride))),
                new(OpCodes.Brfalse_S, l2),
                new(OpCodes.Br_S, l1)
            ]);

            return matcher.Instructions();
        }

        internal static IEnumerable<CodeInstruction> CraftingRecipe_DrawRecipeDescription_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);

            List<CodeInstruction> insert = [
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldloc_1),
                new(OpCodes.Ldloc_S, 13),
                new(OpCodes.Ldloc_S, 5),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(drawnOverrideIngredient))),
                new(OpCodes.Brtrue_S)
            ];
            matcher.Start().MatchStartForward([
                new(OpCodes.Callvirt),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(Vector2), nameof(Vector2.X)))
            ]).Advance(1).CreateLabel(out var l).MatchStartBackwards([
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(Vector2), nameof(Vector2.X))),
                new(OpCodes.Ldc_R4),
                new(OpCodes.Add),
                new(OpCodes.Ldarg_2),
                new(OpCodes.Ldfld, AccessTools.Field(typeof(Vector2), nameof(Vector2.Y))),
                new(OpCodes.Ldc_R4),
                new(OpCodes.Add),
            ]).Instruction.MoveLabelsTo(insert[0]);
            insert[^1].operand = l;
            matcher.InsertAndAdvance(insert);

            return matcher.Instructions();
        }

        internal static bool HasValidTypeDef(CraftingRecipe recipe, [NotNullWhen(true)] out string? typeDef)
        {
            return (CraftingRecipe.craftingRecipes.TryGetValue(recipe.name, out string? data) &&
                    ArgUtility.TryGet(data.Split('/'), 6, out typeDef, out _, false) || 
                    (ArgUtility.TryGet(data.Split('/'), 2, out string itemId, out _, false) && 
                    typeDefFromId(itemId, out typeDef))) ||
                    ModEntry.CraftResultTypeCache.TryGetValue(recipe.name, out typeDef);
        }

        internal static bool isValid(CraftingRecipe recipe, [NotNullWhen(true)] out string? typeDef)
        {
            typeDef = null;
            return !recipe.isCookingRecipe &&
                   HasValidTypeDef(recipe, out typeDef) &&
                   typeDef != ItemRegistry.type_object &&
                   typeDef != ItemRegistry.type_bigCraftable;
        }

        internal static bool isValid(CraftingRecipe recipe) => isValid(recipe, out _);

        private static int getComponentWidth(CraftingRecipe recipe)
        {
            if (isValid(recipe, out var typeDef) && (typeDef == ItemRegistry.type_wallpaper || typeDef == ItemRegistry.type_floorpaper))
                return 64;
            return recipe.GetItemData().GetSourceRect().Width * 4;
        }

        private static int getComponentHeight(CraftingRecipe recipe)
        {
            if (isValid(recipe, out var typeDef) && (typeDef == ItemRegistry.type_wallpaper || typeDef == ItemRegistry.type_floorpaper))
                return 64;
            return recipe.GetItemData().GetSourceRect().Height * 4;
        }

        private static void setOccupiedSpace(ref ClickableTextureComponent[,] spaces, int x, int y, ClickableTextureComponent component, CraftingRecipe recipe)
        {
            if (recipe is null || !isValid(recipe, out var typeDef))
                return;
            if (typeDef == ItemRegistry.type_wallpaper || typeDef == ItemRegistry.type_floorpaper)
                return;
            var sourceRect = recipe.GetItemData().GetSourceRect();
            for (int i = 0; i < sourceRect.Width / 16; i++)
                for (int j = 0; j < sourceRect.Height / 16; j++)
                    spaces[x + i, y + j] = component;
        }

        private static bool drawnOverride(CraftingPage menu, SpriteBatch b, ClickableTextureComponent cmp)
        {
            if (menu is null || cmp is null)
                return false;
            var recipe = menu.pagesOfCraftingRecipes[menu.currentCraftingPage][cmp];
            if (!isValid(recipe, out var typeDef) || (typeDef != ItemRegistry.type_wallpaper && typeDef != ItemRegistry.type_floorpaper))
                return false;
            bool hasEnoughItems = recipe.doesFarmerHaveIngredientsInInventory(ModEntry.IHelper.Reflection.GetMethod(menu, "getContainerContents")?.Invoke<IList<Item>>());
            Color color = cmp.hoverText.Equals("ghosted") ? Color.Black * .35f : (!hasEnoughItems ? Color.DimGray * .4f : Color.White);
            recipe.createItem().drawInMenu(b, new(cmp.bounds.X, cmp.bounds.Y), cmp.scale / 4, 1f, 0.89f, StackDrawType.Hide, color, cmp.drawShadow);
            return true;
        }

        private static void createNewPageIfNeeded(CraftingPage menu, ref ClickableTextureComponent[,] newPageLayout, ref Dictionary<ClickableTextureComponent, CraftingRecipe> newPage, ref int x, ref int y, CraftingRecipe recipe)
        {
            if (recipe is null || !isValid(recipe, out var typeDef))
                return;
            if (typeDef == ItemRegistry.type_wallpaper || typeDef == ItemRegistry.type_floorpaper)
                return;
            var sourceRect = recipe.GetItemData().GetSourceRect();
            for (int i = 0; i < sourceRect.Width / 16; i++)
            {
                if (x + i >= 10)
                {
                    x = 0;
                    ++y;
                }
                for (int j = 0; j < sourceRect.Height / 16; j++)
                {
                    if (y + j >= 4)
                    {
                        newPage = ModEntry.IHelper.Reflection.GetMethod(menu, "createNewPage").Invoke<Dictionary<ClickableTextureComponent, CraftingRecipe>>(null);
                        newPageLayout = ModEntry.IHelper.Reflection.GetMethod(menu, "createNewPageLayout").Invoke<ClickableTextureComponent[,]>(null);
                        x = 0;
                        y = 0;
                        return;
                    }

                }
            }
        }

        private static bool drawnOverrideIngredient(SpriteBatch b, Vector2 position, int ingredientIndex, float scale, string itemId)
        {
            if (!ItemRegistry.IsQualifiedItemId(itemId) || (!itemId.StartsWith(ItemRegistry.type_wallpaper) && !itemId.StartsWith(ItemRegistry.type_floorpaper)))
                return false;

            ItemRegistry.Create(itemId).drawInMenu(b, new Vector2(position.X - 12f, position.Y + 64f + (ingredientIndex * 64 / 2) + (ingredientIndex + 4) - 16f), scale * .75f, 1f, 0.86f, StackDrawType.Hide, Color.White, false);
            return true;
        }

        private static bool typeDefFromId(string itemId, out string? typeDef)
        {
            typeDef = "";
            if (!ItemRegistry.IsQualifiedItemId(itemId))
                return false;
            typeDef = itemId.Split(')')[0] + ')';
            return true;
        }
    }
}
