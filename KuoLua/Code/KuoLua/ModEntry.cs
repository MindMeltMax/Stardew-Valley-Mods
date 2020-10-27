using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

using KuoLua.MapStuff;
using xTile.Tiles;

namespace KuoLua
{
    class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            TileData data = new TileData(helper, Monitor);
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
        }
    }
}
