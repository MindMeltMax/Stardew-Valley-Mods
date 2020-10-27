using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;

using KuoLua;

using Microsoft.Xna.Framework;

namespace KuoLua.MapStuff
{
    class TileData
    {
        public IMonitor Monitor;
        public IModHelper Helper;
        public TileData()
        {
        }

        public TileData(IModHelper helper, IMonitor monitor)
        {
            Monitor = monitor;
            Helper = helper;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;

            if (!(e.Button == SButton.MouseRight))
                return;

            Vector2 tile = e.Cursor.GrabTile;
            var location = Game1.currentLocation;
            string tileProperty = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Action", "Buildings");

            Warp KuoLuaBeach1 = new Warp(Game1.player.getTileX(), Game1.player.getTileY(), "KuoLua", 19, 37, false);
            Warp TransitionalForest1 = new Warp(Game1.player.getTileX(), Game1.player.getTileY(), "TransistionForest1", 0, 0, false);

            Monitor.Log($"tile: {tile} has property: {tileProperty}", LogLevel.Debug);

            if (tileProperty == null)
                return;
            if(tileProperty == "WarpKuoLua")
            {
                Monitor.Log("Aloha!", LogLevel.Info);
                Game1.player.warpFarmer(KuoLuaBeach1);
            }
        }
    }
}
