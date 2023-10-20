using Leclair.Stardew.Common.Crafting;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishnets.BetterCrafting
{
    public class RecipeProvider : IRecipeProvider
    {
        public int RecipePriority => -1;

        public bool CacheAdditionalRecipes => false;

        public IEnumerable<IRecipe> GetAdditionalRecipes(bool cooking) => null;

        public IRecipe GetRecipe(CraftingRecipe recipe)
        {
            if (recipe.name != "Fish Net")
                return null;
            return ModEntry.BetterCraftingApi.RecipeBuilder(recipe)
                                             .Texture(() => ModEntry.IHelper.GameContent.Load<Texture2D>("Fishnets/Fishnet"))
                                             .Source(() => new(0, 0, 16, 16))
                                             .Build();
        }
    }
}
