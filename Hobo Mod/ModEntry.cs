using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.IO;
using Newtonsoft.Json;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using xTile.Dimensions;
using StardewValley;
using StardewValley.Locations;
using Microsoft.Xna.Framework;

namespace Hobo_Mod
{
    public class ModEntry : Mod
    {
        public Chest cooler;
        public FarmHouse house;
        public Vector2 outOfMap = new Vector2(6000, 6000); //Thanks blueberry
        public override void Entry(IModHelper helper)
        {
            this.addSmapiEvents(helper);
        }

        public void addSmapiEvents(IModHelper helper)
        {
            Interactables interact = new Interactables(helper, Monitor);
            BackPackItem pack = new BackPackItem(helper, Monitor);

            helper.Events.Input.CursorMoved += Input_CursorMoved;

            helper.Events.GameLoop.SaveCreated += GameLoop_SaveCreated;
        }

        private void GameLoop_SaveCreated(object sender, SaveCreatedEventArgs e)
        {
            cooler = new Chest(true, outOfMap);
            cooler.fridge.Value = true;
        }


        private void Input_CursorMoved(object sender, CursorMovedEventArgs e)
        {
            /*if (!Context.CanPlayerMove)
                return;
            Location loc = new Location((int)e.NewPosition.GrabTile.X, (int)e.NewPosition.GrabTile.Y);
            Vector2 vec = e.NewPosition.GrabTile;
            string property = Game1.currentLocation.doesTileHaveProperty((int)vec.X, (int)vec.Y, "Action", "Buildings");
            Monitor.LogOnce($"Tile: X:{loc.X}, Y:{loc.Y}; TileProperty: {property}", LogLevel.Debug);*/
        }
    }
}