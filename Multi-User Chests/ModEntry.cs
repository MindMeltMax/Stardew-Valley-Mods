using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

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
            if (!Context.IsMultiplayer || !Context.HasRemotePlayers || Game1.activeClickableMenu != null) 
                return;

            if (e.Button.IsActionButton())
            {
                Vector2 tile = e.Cursor.GrabTile;

                if (Helper.ModRegistry.IsLoaded("spacechase0.ExtendedReach")) 
                    tile = e.Cursor.Tile;

                var OatT = Game1.currentLocation.getObjectAtTile((int)tile.X, (int)tile.Y);

                if (OatT is Chest c && c.playerChest.Value)
                    c.ShowMenu();
            }
        }
    }
}
