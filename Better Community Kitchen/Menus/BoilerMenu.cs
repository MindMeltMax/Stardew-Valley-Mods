using BCC.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.Objects;
using System.Collections.Generic;

namespace BCC.Menus
{
    public class BoilerMenu : IClickableMenu
    {
        public IMonitor Monitor;
        public bool isSmelting;
        public string Title;
        public int BoxCount;
        public List<ClickableTextureComponent> inputComponents = new List<ClickableTextureComponent>();
        public List<ClickableTextureComponent> outputComponents = new List<ClickableTextureComponent>();
        public ClickableTextureComponent Component;

        public BoilerMenu(IMonitor monitor, Chest coalStorageObject, int boxCount = 5) : base(0, 0, 0, 0, true)
        {
            Monitor = monitor;
            Title = Util.i18n.Get("BoilerTitle");
            BoxCount = boxCount;

            width = 800 + borderWidth * 2;
            height = 600 + borderWidth * 2;
            xPositionOnScreen = Game1.viewport.Width / 2 - (800 + borderWidth * 2) / 2;
            yPositionOnScreen = Game1.viewport.Height / 2 - (600 + borderWidth * 2) / 2;
            upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 30, Game1.viewport.Height / 2 - 296, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
            Component = new ClickableTextureComponent("Component", new Rectangle(xPositionOnScreen + 192, yPositionOnScreen + 128, 64, 64), null, "", Game1.menuTexture, new Rectangle(128, 128, 64, 64), 1f)
            {
                myID = 7604,
                downNeighborID = -7777
            };
            int indexer1 = 48;
            int indexer2 = 48;
            for(int i =0; i<boxCount; ++i)
            {
                ClickableTextureComponent component = new ClickableTextureComponent("Component", new Rectangle(xPositionOnScreen + 128, yPositionOnScreen + 96 + indexer1, 64, 64), null, "", Game1.menuTexture, new Rectangle(128, 128, 64, 64), 1f)
                {
                    myID = i,
                    downNeighborID = i == 4 ? -7777 : i - 1,
                    upNeighborID = i == 0 ? -7777 : i + 1
                };
                inputComponents.Add(component);
                indexer1 += 96;
            }
            for (int i = 0; i < boxCount; ++i)
            {
                ClickableTextureComponent component = new ClickableTextureComponent("Component", new Rectangle(xPositionOnScreen + width - 160, yPositionOnScreen + 96 + indexer2, 64, 64), null, "", Game1.menuTexture, new Rectangle(128, 128, 64, 64), 1f)
                {
                    myID = i,
                    downNeighborID = i == 4 ? -7777 : i - 1,
                    upNeighborID = i == 0 ? -7777 : i + 1
                };
                outputComponents.Add(component);
                indexer2 += 96;
            }
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground)
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            else
                drawBackground(b);
            base.draw(b);
            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);
            SpriteText.drawStringWithScrollCenteredAt(b, Title, Game1.viewport.Width / 2 + 50, Game1.viewport.Height / 2 - 310, Title, 1f, -1, 0, 0.88f, false);
            foreach (ClickableTextureComponent texture in inputComponents)
                texture.draw(b);
            foreach (ClickableTextureComponent texture in outputComponents)
                texture.draw(b);
            drawMouse(b);
        }
    }
}
