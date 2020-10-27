using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Objects;
using StardewValley.Menus;

using Microsoft.Xna.Framework;

using System.Collections.Generic;

using Netcode;

namespace Hobo_Mod
{
    public class Interactables
    {
        private IModHelper modHelper;
        private IMonitor modMonitor;
        public GameLocation location;
        public Chest cooler;

        public Vector2 outOfMap = new Vector2(6000, 6000);

        public Interactables(IModHelper helper, IMonitor monitor)
        {
            this.modHelper = helper;
            this.modMonitor = monitor;

            modHelper.Events.Input.ButtonPressed += Input_ButtonPressed;
            modHelper.Events.Input.CursorMoved += Input_CursorMoved;

            modHelper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
        }

        private void Input_CursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;
            if (e.Button != SButton.MouseRight || e.Cursor.GrabTile != e.Cursor.Tile)
                return;
            location = Game1.currentLocation;
            Vector2 vec = e.Cursor.GrabTile;
            string property = Game1.currentLocation.doesTileHaveProperty((int)vec.X, (int)vec.Y, "Action", "Buildings");
            Vector2 centerScreenPos = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
            if (location.objects.ContainsKey(outOfMap))
                cooler = location.objects[outOfMap] as Chest ?? new Chest(true, outOfMap);
            else
                location.objects.Add(outOfMap, new Chest(true, outOfMap));

            if (property == null)
                return;

            if(property == "Cooler")
            {
                if (cooler != null)
                {
                    this.modMonitor.Log("Opening Cooler Menu...", LogLevel.Debug);
                    Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu((IList<Item>)cooler.items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems),
                                                new ItemGrabMenu.behaviorOnItemSelect(cooler.grabItemFromInventory), (string)null, new ItemGrabMenu.behaviorOnItemSelect(cooler.grabItemFromChest),
                                                false, true, true, true, true, 1, sourceItem: ((bool)(NetFieldBase<bool, NetBool>)cooler.fridge ? (Item)null : (Item)cooler), context: ((object)cooler));
                }
                else
                {
                    this.modMonitor.Log("Failed creating object cooler", LogLevel.Debug);
                }
            }
            else if(property == "kitchen")
            {
                this.modMonitor.Log("Opening Kitchen Menu...", LogLevel.Debug);
                Game1.activeClickableMenu = (IClickableMenu)new CraftingPage((int)centerScreenPos.X, (int)centerScreenPos.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, true, true);
            }
        }
    }
}
