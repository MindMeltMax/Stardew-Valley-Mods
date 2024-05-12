using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace QualityBait
{
    internal static class Patches
    {
        private static Harmony harmony;

        internal static void Patch(string id)
        {
            harmony ??= new(id);

            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingPage), "clickCraftingRecipe", [typeof(ClickableTextureComponent), typeof(bool)]),
                transpiler: new(typeof(Patches), nameof(CraftingPage_ClickCraftingRecipe_Transpiler))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingPage), nameof(CraftingPage.draw), [typeof(SpriteBatch)]),
                transpiler: new(typeof(Patches), nameof(CraftingPage_Draw_Transpiler))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(CrabPot), nameof(CrabPot.DayUpdate)),
                prefix: new(typeof(Patches), nameof(CrabPot_DayUpdate_Prefix)),
                postfix: new(typeof(Patches), nameof(CrabPot_DayUpdate_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(FishingRod), nameof(FishingRod.pullFishFromWater)),
                prefix: new(typeof(Patches), nameof(FishingRod_PullFishFromWater_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.consumeIngredients)),
                prefix: new(typeof(Patches), nameof(CraftingRecipe_ConsumeIngredients_Prefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.doesFarmerHaveIngredientsInInventory)),
                prefix: new(typeof(Patches), nameof(CraftingRecipe_DoesFarmerHaveIngredientsInInventory_Prefix)) //Holy shit, that's a long name
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(MachineDataUtility), nameof(MachineDataUtility.GetOutputItem)),
                postfix: new(typeof(Patches), nameof(MachineDataUtility_GetOutputItem_Postfix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.drawMenuView)),
                postfix: new(typeof(Patches), nameof(CraftingRecipe_DrawMenuView_Postfix))
            );

            // Khloe if you see this, I'm sorry, believe me I tried to use the api, but at around 4 am I just gave up all hope.
            // I can't use the OnPerformCraft or OnPostCraft event because the first won't run the original code, and the second won't craft more than 1 item because the next one is not stackable with the quality variant
            // If / When I find a way to use the api for all of this, I'll switch to that
            if (ModEntry.IHelper.ModRegistry.IsLoaded("leclair.bettercrafting") && ModEntry.IConfig.EnableBetterCraftingIntegration)
            {
                try
                {
                    harmony.Patch(
                        original: AccessTools.Method("Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage:PerformCraftRecursive"),
                        transpiler: new(typeof(Patches), nameof(BetterCraftingPage_PerformCraftRecursive_Transpiler))
                    );
                }
                catch(Exception ex)
                {
                    ModEntry.IMonitor.Log($"An error occured while trying to apply a patch to a better crafting method, most likely better crafting updated and the relevant code has changed. Please submit a bug report with a log of this error to Quality Bait", StardewModdingAPI.LogLevel.Error);
                    ModEntry.IMonitor.Log($"[{nameof(BetterCraftingPage_PerformCraftRecursive_Transpiler)}] {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                }
                try
                {
                    harmony.Patch(
                        original: AccessTools.Method("Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage:draw", [typeof(SpriteBatch)]),
                        transpiler: new(typeof(Patches), nameof(BetterCraftingPage_Draw_Transpiler))
                    );
                }
                catch (Exception ex)
                {
                    ModEntry.IMonitor.Log($"An error occured while trying to apply a patch to a better crafting method, most likely better crafting updated and the relevant code has changed. Please submit a bug report with a log of this error to Quality Bait", StardewModdingAPI.LogLevel.Error);
                    ModEntry.IMonitor.Log($"[{nameof(BetterCraftingPage_Draw_Transpiler)}] {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                }
                try
                {
                    harmony.Patch(
                        original: AccessTools.Method("Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage:CanCraft"),
                        transpiler: new(typeof(Patches), nameof(BetterCraftingPage_CanCraft_Transpiler))
                    );
                }
                catch (Exception ex)
                {
                    ModEntry.IMonitor.Log($"An error occured while trying to apply a patch to a better crafting method, most likely better crafting updated and the relevant code has changed. Please submit a bug report with a log of this error to Quality Bait", StardewModdingAPI.LogLevel.Error);
                    ModEntry.IMonitor.Log($"[{nameof(BetterCraftingPage_CanCraft_Transpiler)}] {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        internal static void UnPatch(string id) => harmony.UnpatchAll(id);

        #region Transpilers

        internal static IEnumerable<CodeInstruction> CraftingPage_ClickCraftingRecipe_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            generator.DeclareLocal(typeof(int));
            CodeMatcher matcher = new(instructions, generator);

            matcher.Start().MatchEndForward([
                new(OpCodes.Ldloc_0), //recipe
                new(OpCodes.Callvirt, AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem))),
                new(OpCodes.Stloc_1) //jump past item creation
            ]).Advance(1).InsertAndAdvance([
                new(OpCodes.Ldloc_0), //recipe
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(isKnownRecipe))),
                new(OpCodes.Brfalse_S), //skip next code if not known recipe
                new(OpCodes.Ldloc_0), //recipe
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(getQualityForRecipe))),
                new(OpCodes.Stloc_S, 6), //assign quality to variable at index 6
                new(OpCodes.Ldloc_1), //obj
                new(OpCodes.Ldloc_S, 6), //quality
                new(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(Object), nameof(Object.Quality))) //set quality
            ]).CreateLabel(out var l).MatchEndBackwards([
                new(OpCodes.Ldloc_0), //recipe
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(isKnownRecipe))),
                new(OpCodes.Brfalse_S)
            ]).Instruction.operand = l; //set jump label at skip instruction

            return matcher.Instructions();
        }

        internal static IEnumerable<CodeInstruction> CraftingPage_Draw_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);

            CodeInstruction startInsert = new(OpCodes.Ldarg_0);

            matcher.Start().MatchStartForward([ //move next iterator of foreach loop
                new(OpCodes.Ldloca_S),
                new(OpCodes.Call),
                new(OpCodes.Brtrue),
                new(OpCodes.Leave_S),
            ]).Instruction.MoveLabelsTo(startInsert); //move labels from exit loop index to custom draw stack

            matcher.InsertAndAdvance([
                startInsert, //this
                new(OpCodes.Ldarg_1), //b
                new(OpCodes.Ldloc_2), //key
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(drawQualityIfNeeded))),
            ]);

            return matcher.Instructions();
        }

        #region Better Crafting

        internal static IEnumerable<CodeInstruction> BetterCraftingPage_PerformCraftRecursive_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);

            //Has Ingredients Fix
            matcher.Start().MatchEndForward([
                new(OpCodes.Ldloc_0),
                new(OpCodes.Ldfld),
                new(OpCodes.Call),
                new(OpCodes.Callvirt),
                new(OpCodes.Brfalse_S),
            ]).Advance(1).InsertAndAdvance([
                new(OpCodes.Ldarg_1),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter("Leclair.Stardew.Common.Crafting.IRecipe:CraftingRecipe")),
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
                new(OpCodes.Ldarg_S, 6),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(hasIngredientsForRecipe))),
                new(OpCodes.Brfalse_S),
            ]).MatchStartForward([
                new(OpCodes.Ldloc_0),
                new(OpCodes.Ldfld),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Ldfld),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Ldfld),
                new(OpCodes.Callvirt),
                new(OpCodes.Ret),
            ]).CreateLabel(out var l).MatchEndBackwards([
                new(OpCodes.Ldarg_1),
                new(OpCodes.Callvirt),
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
                new(OpCodes.Ldarg_S),
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(hasIngredientsForRecipe))),
                new(OpCodes.Brfalse_S),
            ]).Instruction.operand = l;

            //Set Quality
            matcher.Start().MatchEndForward([
                new(OpCodes.Ldloc_1),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Ldfld),
                new(OpCodes.Callvirt),
                new(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(Item), nameof(Item.Stack))),
            ]).Advance(1).InsertAndAdvance([
                new(OpCodes.Ldloc_1),
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.PropertyGetter("Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage:ActiveRecipe")),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter("Leclair.Stardew.Common.Crafting.IRecipe:CraftingRecipe")),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(getQualityForRecipe))),
                new(OpCodes.Callvirt, AccessTools.PropertySetter(typeof(Item), nameof(Item.Quality)))
            ]);

            return matcher.Instructions();
        }

        internal static IEnumerable<CodeInstruction> BetterCraftingPage_Draw_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);

            CodeInstruction startInsert = new(OpCodes.Ldarg_1);

            matcher.Start().MatchEndForward([
                new(OpCodes.Ldloc_S),
                new(OpCodes.Ldfld),
                new(OpCodes.Ldstr, "ghosted"),
                new(OpCodes.Callvirt),
                new(OpCodes.Stloc_S),
            ]);
            matcher.MatchStartForward([
                new(OpCodes.Ldarg_0),
                new(OpCodes.Call, AccessTools.PropertyGetter("Leclair.Stardew.BetterCrafting.Menus.BetterCraftingPage:Editing")),
                new(OpCodes.Brtrue_S),
                new(OpCodes.Ldloc_S), 
                new(OpCodes.Ldc_I4_0),
                new(OpCodes.Ceq),
                new(OpCodes.Br_S),
            ]);
            matcher.Instruction.MoveLabelsTo(startInsert);

            matcher.InsertAndAdvance([
                startInsert,
                new(OpCodes.Ldloc_S, 25),
                new(OpCodes.Ldloc_S, 26),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter("Leclair.Stardew.Common.Crafting.IRecipe:CraftingRecipe")),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(drawQualityIfNeededBC)))
            ]);

            return matcher.Instructions();
        }

        internal static IEnumerable<CodeInstruction> BetterCraftingPage_CanCraft_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher matcher = new(instructions, generator);

            CodeInstruction startInsert = new(OpCodes.Ldarg_1);

            matcher.Start().MatchStartForward([
                new(OpCodes.Ldarg_0),
                new(OpCodes.Ldfld),
                new(OpCodes.Ldarg_1),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Callvirt),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Ret)
            ]).Instruction.MoveLabelsTo(startInsert);

            matcher.InsertAndAdvance([
                startInsert,
                new(OpCodes.Callvirt, AccessTools.PropertyGetter("Leclair.Stardew.Common.Crafting.IRecipe:CraftingRecipe")),
                new(OpCodes.Call, AccessTools.PropertyGetter(typeof(Game1), nameof(Game1.player))),
                new(OpCodes.Ldloc_2),
                new(OpCodes.Ldloc_0),
                new(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(hasIngredientsForRecipe))),
                new(OpCodes.Stloc_0)
            ]);

            return matcher.Instructions();
        }

        #endregion

        #endregion

        #region Patches

        internal static void CrabPot_DayUpdate_Prefix(CrabPot __instance, ref (bool, int) __state)
        {
            __state = (false, 0);
            if (__instance.heldObject.Value is null)
                __state = (true, __instance.bait.Value?.Quality ?? 0);
        }

        internal static void CrabPot_DayUpdate_Postfix(CrabPot __instance, (bool, int) __state)
        {
            if (__instance.heldObject.Value is Object obj && __state.Item1)
                __instance.heldObject.Value.Quality = getQualityForCatch(obj.ItemId, obj.Quality, __state.Item2);
        }

        internal static void FishingRod_PullFishFromWater_Prefix(FishingRod __instance, string fishId, ref int fishQuality)
        {
            if (__instance.attachments.Count > 0 && __instance.attachments[0] is not null and Object obj)
                fishQuality = getQualityForCatch(fishId, fishQuality, obj.Quality);
        }

        // Choosing to still prefix this instead of transpiling, because it's easier that way
        internal static bool CraftingRecipe_ConsumeIngredients_Prefix(CraftingRecipe __instance, List<IInventory> additionalMaterials)
        {
            if (!isKnownRecipe(__instance))
                return true;
            var recipe = __instance;
            var quality = getQualityForRecipe(recipe);
            foreach (var ingredient in recipe.recipeList)
            {
                int ingredientCount = ingredient.Value;
                var items = getItemsWithId(ingredient.Key, Game1.player.Items, quality);
                for (int i = 0; i < items.Count; i++)
                {
                    int index = Game1.player.Items.IndexOf(items[i]);
                    int num = ingredientCount;
                    ingredientCount -= items[i].Stack;
                    if ((Game1.player.Items[index].Stack -= num) <= 0)
                        Game1.player.Items[index] = null;
                    if (ingredientCount <= 0)
                        break;
                }

                if (ingredientCount <= 0)
                    continue;

                for (int i = 0; i < additionalMaterials.Count; i++)
                {
                    IInventory inventory = additionalMaterials[i];
                    if (inventory is null)
                        continue;
                    items = getItemsWithId(ingredient.Key, Game1.player.Items, quality);
                    for (int j = 0; j < items.Count; j++)
                    {
                        int index = inventory.IndexOf(items[j]);
                        int num = ingredientCount;
                        ingredientCount -= items[j].Stack;
                        if ((inventory[index].Stack -= num) <= 0)
                        {
                            inventory[index] = null;
                            inventory.RemoveEmptySlots();
                        }
                        if (ingredientCount <= 0)
                            break;
                    }
                    if (ingredientCount <= 0)
                        break;
                }
            }
            return false;
        }

        //Same as above, just can't be arsed to go through a more difficult transpiler
        internal static bool CraftingRecipe_DoesFarmerHaveIngredientsInInventory_Prefix(CraftingRecipe __instance, IList<Item> extraToCheck, ref bool __result)
        {
            if (!isKnownRecipe(__instance))
                return true;
            var recipe = __instance;
            __result = hasIngredientsForRecipe(recipe, Game1.player, extraToCheck);
            return false;
        }

        internal static void MachineDataUtility_GetOutputItem_Postfix(Object machine, Item inputItem, ref Item __result)
        {
            if (__result is null || machine.QualifiedItemId != "(BC)BaitMaker" || !ModEntry.IConfig.BaitMakerQuality)
                return;
            __result.Quality = ModEntry.GetQualityForBait(__result.Quality, inputItem.Quality);
        }

        internal static void CraftingRecipe_DrawMenuView_Postfix(CraftingRecipe __instance, SpriteBatch b, int x, int y)
        {
            if (!isKnownRecipe(__instance))
                return;
            int quality = getQualityForRecipe(__instance);
            Rectangle sourceRect = getSourceRectForQuality(quality);
            float num = quality < 4 ? 0.0f : (float)((Math.Cos(Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.0500000007450581);
            b.Draw(Game1.mouseCursors, new(x + 12, y + 52), sourceRect, Color.White, 0.0f, new(4f), (float)(3.0 * 1.0 * (1.0 + num)), SpriteEffects.None, .97f);
        }

        #endregion

        #region Utility

        private static bool isKnownRecipe(CraftingRecipe recipe) => ModEntry.Recipes.ContainsKey(recipe.name);

        private static int getQualityForRecipe(CraftingRecipe recipe)
        {
            if (recipe.name.Contains("(Silver)"))
                return Object.medQuality;
            if (recipe.name.Contains("(Gold)"))
                return Object.highQuality;
            if (recipe.name.Contains("(Iridium)"))
                return Object.bestQuality;
            return Object.lowQuality;
        }

        private static Rectangle getSourceRectForQuality(int quality)
        {
            return quality switch
            {
                Object.medQuality => new(338, 400, 8, 8),
                Object.highQuality => new(346, 400, 8, 8),
                Object.bestQuality => new(346, 392, 8, 8),
                _ => new(338, 392, 8, 8)
            };
        }

        private static void drawQualityIfNeeded(CraftingPage menu, SpriteBatch b, ClickableTextureComponent component)
        {
            if (component is null)
                return;
            var recipe = menu.pagesOfCraftingRecipes[menu.currentCraftingPage][component];
            if (!isKnownRecipe(recipe))
                return;
            int quality = getQualityForRecipe(recipe);
            Rectangle sourceRect = getSourceRectForQuality(quality);
            float num = quality < 4 ? 0.0f : (float)((Math.Cos(Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.0500000007450581);
            b.Draw(Game1.mouseCursors, new(component.bounds.X + 12, component.bounds.Y + 52), sourceRect, Color.White, 0.0f, new(4f), (float)(3.0 * 1.0 * (1.0 + num)), SpriteEffects.None, .97f);
        }

        private static int getQualityForCatch(string itemId, int originalQuality, int baitQuality) => ModEntry.GetQualityForCatch(itemId, originalQuality, baitQuality);

        private static List<Item> getItemsWithId(string id, IEnumerable<Item> source, int baseQuality)
        {
            List<Item> matchingItems = [];
            foreach (var item in source)
                if (item?.ItemId == id && (!ModEntry.IConfig.ForceLowerQuality || item.Quality < baseQuality))
                    matchingItems.Add(item);
            matchingItems.Sort((a, b) => a.Quality.CompareTo(b.Quality));
            return matchingItems;
        }

        private static int getCountOfItemsWithId(string id, IEnumerable<Item> source, int baseQuality)
        {
            int num = 0;
            foreach (var item in source)
                if (item?.ItemId == id && (!ModEntry.IConfig.ForceLowerQuality || item.Quality < baseQuality))
                    num += item.Stack;
            return num;
        }

        private static void drawQualityIfNeededBC(SpriteBatch b, ClickableTextureComponent component, CraftingRecipe recipe)
        {
            if (component is null || !isKnownRecipe(recipe))
                return;
            int quality = getQualityForRecipe(recipe);
            Rectangle sourceRect = getSourceRectForQuality(quality);
            float num = quality < 4 ? 0.0f : (float)((Math.Cos(Game1.currentGameTime.TotalGameTime.Milliseconds * Math.PI / 512.0) + 1.0) * 0.0500000007450581);
            b.Draw(Game1.mouseCursors, new(component.bounds.X + 12, component.bounds.Y + 52), sourceRect, Color.White, 0.0f, new(4f), (float)(3.0 * 1.0 * (1.0 + num)), SpriteEffects.None, .97f);
        }

        private static bool hasIngredientsForRecipe(CraftingRecipe recipe, Farmer who, IList<Item> extraItems, bool original = false)
        {
            if (!isKnownRecipe(recipe)) 
                return original;
            int quality = getQualityForRecipe(recipe);
            foreach (var ingredient in recipe.recipeList)
            {
                int num = ingredient.Value - getCountOfItemsWithId(ingredient.Key, who.Items, quality);
                if (extraItems is not null)
                    num -= getCountOfItemsWithId(ingredient.Key, extraItems, quality);
                if (num > 0)
                    return false;
            }
            return true;
        }

        #endregion
    }
}
