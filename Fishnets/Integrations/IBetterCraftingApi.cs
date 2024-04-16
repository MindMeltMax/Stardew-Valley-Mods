using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishnets.Integrations
{
    public interface IBetterCraftingApi
    {
        void AddRecipesToDefaultCategory(bool cooking, string categoryId, IEnumerable<string> recipeNames);
    }
}
