using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChestDisplays.Patches
{
    public class Patcher
    {
        private static Harmony harmony;

        public static void Init(IModHelper helper)
        {
            harmony = new Harmony(helper.ModRegistry.ModID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
                //prefix: new HarmonyMethod(typeof(ChestPatches), nameof(ChestPatches.draw_prefix))
                postfix: new HarmonyMethod(typeof(ChestPatches), nameof(ChestPatches.draw_postfix))
            );
        }
    }
}
