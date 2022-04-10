using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multi_User_Chest
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += OnButtonDown;
        }

        private void OnButtonDown(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsMultiplayer || Game1.getOnlineFarmers().Count <= 1 || Game1.activeClickableMenu != null) return;

            if (SButtonExtensions.IsActionButton(e.Button))
            {
                var tile = e.Cursor.Tile;
                var OatT = Game1.currentLocation.getObjectAtTile((int)tile.X, (int)tile.Y);

                if (OatT is not null and Chest c && c.playerChest.Value)
                    c.ShowMenu();
            }
        }
    }
}
