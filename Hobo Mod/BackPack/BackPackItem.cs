using Hobo_Mod.BackPack;
using Hobo_Mod.Data;

using Microsoft.Xna.Framework;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using StardewValley;
using StardewValley.Objects;
using StardewValley.Menus;

using Netcode;

using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Hobo_Mod
{
    class BackPackItem
    {
        public IModHelper modHelper;
        public IMonitor modMonitor;
        public HMConfig Config;

        public Chest backPack;
        public Vector2 outOfMapBounds2 = new Vector2(6001, 6001);
        private static Texture2D BackPackSprite;

        private static bool _PickupKeyPressed;

        private static IJsonAssetsApi JA;
        public BackPackItem(IModHelper helper, IMonitor monitor)
        {
            modHelper = helper;
            modMonitor = monitor;

            modHelper.Events.GameLoop.SaveCreated += GameLoop_SaveCreated;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            modHelper.Events.Input.ButtonPressed += Input_ButtonPressed;
            modHelper.Events.Input.ButtonReleased += Input_ButtonReleased;

            this.readWriteConfig();
        }

        public void readWriteConfig()
        {
            Config = (HMConfig)modHelper.ReadConfig<HMConfig>();
            if (!(Config.PickupKey == "control"))
                return;
            Config.PickupKey = "leftcontrol,rightcontrol";
            modHelper.WriteConfig<HMConfig>(Config);
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            JA = modHelper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            JA.LoadAssets(Path.Combine(modHelper.DirectoryPath, "assets"));
        }

        private void GameLoop_SaveCreated(object sender, SaveCreatedEventArgs e)
        {
            backPack = new Chest(true, outOfMapBounds2);
            backPack.Name = "backPack";
            //Some Random comment
        }

        private void Input_ButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (!this.IsButtonPickupKey(e.Button))
                return;
            _PickupKeyPressed = false;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;
            if (this.IsButtonPickupKey(e.Button))
                _PickupKeyPressed = true;

            Vector2 tile = e.Cursor.Tile;
            GameLocation location = Game1.currentLocation;

            Vector2 vec = e.Cursor.GrabTile;
            string property = Game1.currentLocation.doesTileHaveProperty((int)vec.X, (int)vec.Y, "Action", "Buildings");
            string Property = Game1.currentLocation.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Action", "Buildings");

            var clickedObject = location.getObjectAtTile((int)tile.X, (int)tile.Y);
            if (clickedObject != null)
                modMonitor.Log($"Object : {clickedObject.Name} - Property : {Property}", LogLevel.Debug);
            else
                return;

            if (location.objects.ContainsKey(outOfMapBounds2))
                backPack = location.objects[outOfMapBounds2] as Chest ?? new Chest(true, outOfMapBounds2);
            else
                location.objects.Add(outOfMapBounds2, backPack);

            if (property == null)
                return;
            if(property == "backPack" && !_PickupKeyPressed)
            {
                modMonitor.Log("BackPack found, attempting to open...", LogLevel.Trace);
                Game1.activeClickableMenu = (IClickableMenu)new ItemGrabMenu((IList<Item>)backPack.items, false, true, new InventoryMenu.highlightThisItem(InventoryMenu.highlightAllItems),
                                                new ItemGrabMenu.behaviorOnItemSelect(backPack.grabItemFromInventory), (string)null, new ItemGrabMenu.behaviorOnItemSelect(backPack.grabItemFromChest),
                                                false, true, true, true, true, 1, sourceItem: ((bool)(NetFieldBase<bool, NetBool>)backPack.fridge ? (Item)null : (Item)backPack), context: ((object)backPack));
            }
            else if(property == "backPack" && _PickupKeyPressed)
            {
                modMonitor.Log("BackPack found, attempting to pick up...", LogLevel.Trace);
                if(e.Button == SButton.MouseLeft && Game1.player.CurrentItem == null)
                {
                    if(location.objects.TryGetValue(tile, out StardewValley.Object obj) && obj is Chest chest && chest.Name == nameof(backPack))
                    {
                        location.objects.Remove(tile);
                        location.removeTileProperty((int)vec.X, (int)vec.Y, "Buildings", "Action");
                        Game1.player.addItemToInventory(backPack);
                    }
                }
            }
            else if(property == null && Game1.player.CurrentItem.DisplayName == "backPack")
            {
                modMonitor.Log("Attempting to place backpack...", LogLevel.Trace);
                if(e.Button == SButton.MouseLeft && Game1.player.CurrentItem.DisplayName == "backPack")
                {
                    location.objects.Add(tile, backPack);
                    location.setTileProperty((int)vec.X, (int)vec.Y, "Buildings", "Action", "backPack");
                    Game1.player.removeItemFromInventory(backPack);
                }
            }
        }

        private bool IsButtonPickupKey(SButton button)
        {
            string buttonAsString = button.ToString().ToLower();
            return ((IEnumerable<string>)Config.PickupKey.ToLower().Split(new char[1]
            {
                ','
            }, StringSplitOptions.RemoveEmptyEntries)).Any<string>((Func<string, bool>)(Item => buttonAsString.Equals(Item.Trim())));
        }
    }
    public interface IJsonAssetsApi
    {
        int GetObjectId(string name);
        void LoadAssets(string path);
    }
}
