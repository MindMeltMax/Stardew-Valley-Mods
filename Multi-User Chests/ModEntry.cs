using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Multi_User_Chest
{
    public class ModEntry : Mod
    {
        private readonly PerScreen<Vector2?> Tile = new();

        public override void Entry(IModHelper helper) //I'm commited to doing this without harmony at this point
        {
            helper.Events.Input.ButtonPressed += OnButtonDown;

            helper.Events.World.ObjectListChanged += OnObjectListChanged;
            helper.Events.Display.MenuChanged += OnMenuChanged;
        }

        private void OnButtonDown(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsMultiplayer || Game1.getOnlineFarmers().Count <= 1 || Game1.activeClickableMenu != null) 
                return;

            if (e.Button.IsActionButton())
            {
                Vector2 tile = e.Cursor.GrabTile;

                if (Helper.ModRegistry.IsLoaded("spacechase0.ExtendedReach")) 
                    tile = e.Cursor.Tile;

                var OatTOrig = Game1.currentLocation.getObjectAtTile((int)tile.X, (int)tile.Y);

                DelayedAction.functionAfterDelay(() =>
                {
                    var OatT = Game1.currentLocation.getObjectAtTile((int)tile.X, (int)tile.Y); //Check after delay for the chest object
                    if (OatT.QualifiedItemId == OatTOrig.QualifiedItemId && OatT is Chest c && c.playerChest.Value && c.GetMutex().IsLocked())
                    {
                        Game1.playSound("openChest");
                        c.ShowMenu();
                        Tile.Value = c.TileLocation;
                    }
                }, 250);
            }
        }

        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!e.IsCurrentLocation)
                return;
            if (Tile.Value is not null && e.Removed.Any(x => x.Key == Tile.Value))
                Game1.activeClickableMenu = null;
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is null && Tile.Value is not null)
                Tile.Value = null;
        }

        /*private void OnUpdateTicked(object sender, UpdateTickedEventArgs e) //Close the active chest menu if the chest is replaced \\This was the dumbest fucking thing I wrote in a minute
        {
        }*/
    }
}
