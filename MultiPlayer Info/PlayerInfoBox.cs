using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Linq;
using BoxPosition = MPInfo.Position;

namespace MPInfo
{
    public class PlayerInfoBox : IClickableMenu
    {
        public const int PlayerIconBoxSize = 96;
        public const int PlayerInfoStartOffset = 12;
        public const int PlayerInfoAreaSize = 112;
        public const int PlayerInfoEndSize = 28;

        public Farmer Who { get; }

        public static Texture2D? Crown { get; internal set; }

        private Texture2D texture => Game1.mouseCursors;
        private Rectangle sourceRectIconBackground => new(293, 360, 24, 24);
        private Rectangle sourceRectIconBackgroundSelf => new(163, 399, 24, 24);

        private Rectangle[] sourceRectInfoDisplay =>
        [
            new Rectangle(317, 361, 3, 22), //Left
            new Rectangle(320, 361, 2, 22), //Middle (Expands based on given info)
            new Rectangle(322, 361, 7, 22)  //Right
        ];

        private Rectangle[] sourceRectIconPassOut =>
        [
            new Rectangle(195, 408, 4, 8), //Vertical
            new Rectangle(193, 410, 8, 4), //Horizontal
            new Rectangle(194, 409, 1, 1)  //Corner
        ];

        private Rectangle sourceRectIconEnergy => new(0, 428, 10, 10);
        private Rectangle sourceRectIconHealth => new(0, 438, 10, 10);
        private Rectangle sourceRectIconSkull => new(140, 428, 10, 10);
        private Rectangle sourceRectIconCrown => new(0, 0, 9, 7);

        private ModEntry context => ModEntry.Instance;
        private Config config => ModEntry.Config;

        private string hoverText = "";

        public PlayerInfoBox(Farmer who)
        {
            Who = who;
            if (config.ShowPlayerIcon)
            {
                width = PlayerIconBoxSize + PlayerInfoStartOffset + PlayerInfoAreaSize + PlayerInfoEndSize;
                height = PlayerIconBoxSize;
            }
            else
            {
                width = PlayerInfoEndSize + PlayerInfoAreaSize + PlayerInfoEndSize;
                height = PlayerIconBoxSize;
            }
        }

        public static void FixPositions()
        {
            int index = 0;
            var visibleBoxes = Game1.onScreenMenus.Where(x => x is PlayerInfoBox pib && pib.Visible());
            if (!(visibleBoxes?.Any() ?? false))
                return;

            foreach (var box in visibleBoxes)
            {
                var pib = (box as PlayerInfoBox)!;
                var pos = pib.GetPosition(index);
                pib.xPositionOnScreen = (int)pos.X;
                pib.yPositionOnScreen = (int)pos.Y;
                index++;
            }
        }

        public bool Visible() => context.IsEnabled && (Who?.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID || config.ShowSelf);

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            if (oldBounds != newBounds)
                FixPositions();
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            if (new Rectangle(xPositionOnScreen, yPositionOnScreen, PlayerIconBoxSize, PlayerIconBoxSize).Contains(x, y))
                hoverText = $"{Who.Name}{(Game1.player.UniqueMultiplayerID == Who.UniqueMultiplayerID ? $" {context.Helper.Translation.Get("PlayerInfoBox.Me")}" : (Game1.serverHost.Value.UniqueMultiplayerID == Who.UniqueMultiplayerID ? $" {context.Helper.Translation.Get("PlayerInfoBox.Host")}" : ""))}";
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (new Rectangle(xPositionOnScreen, yPositionOnScreen, PlayerIconBoxSize, PlayerIconBoxSize).Contains(x, y))
                context.ForceUpdate();
        }

        public override void draw(SpriteBatch b)
        {
            if (!Visible())
                return;

            base.draw(b);

            var playerInfo = context.PlayerData[Who.UniqueMultiplayerID];
            if (playerInfo is null || Who.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID || context.TraditionalStaminaReporting.Contains(Who.UniqueMultiplayerID))
                playerInfo = new(Who);

            drawInfoBoxBackground(b);
            drawPlayerIcon(b, playerInfo);
            drawInfoText(b, playerInfo);

            if (!string.IsNullOrWhiteSpace(hoverText))
            {
                drawHoverText(b, hoverText, Game1.smallFont);
                hoverText = "";
            }
        }

        private Vector2 GetPosition(int index)
        {
            int x = 32, y = 0;
            int spaceBetween = config.SpaceBetween * index;
            switch (config.Position)
            {
                case BoxPosition.BottomLeft:
                    y = Game1.graphics.GraphicsDevice.Viewport.Height - 32 - PlayerIconBoxSize - spaceBetween;
                    break;
                case BoxPosition.TopLeft:
                    y = 32 + spaceBetween;
                    break;
                case BoxPosition.BottomRight:
                    y = Game1.graphics.GraphicsDevice.Viewport.Height - 32 - PlayerIconBoxSize - spaceBetween;
                    x = Game1.graphics.GraphicsDevice.Viewport.Width - PlayerInfoAreaSize - 16;
                    break;
                case BoxPosition.CenterRight:
                    y = (Game1.graphics.GraphicsDevice.Viewport.Height / 2) + 32 - PlayerIconBoxSize + 8 + spaceBetween;
                    x = Game1.graphics.GraphicsDevice.Viewport.Width - PlayerInfoAreaSize - 16;
                    break;
            }

            return new(x + config.XOffset, y + config.YOffset);
        }

        private bool isPassingOut(PlayerInfo info) => Who.passedOut || Who.FarmerSprite.isPassingOut() || info.Stamina <= -15f;

        private void drawInfoBoxBackground(SpriteBatch b)
        {
            switch (config.Position)
            {
                case BoxPosition.TopLeft:
                case BoxPosition.BottomLeft:
                    if (config.ShowPlayerIcon)
                    {
                        b.Draw(texture, new(xPositionOnScreen, yPositionOnScreen), Who.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID ? sourceRectIconBackgroundSelf : sourceRectIconBackground, Color.White, 0f, Vector2.Zero, 4, SpriteEffects.None, .88f);
                        b.Draw(texture, new(xPositionOnScreen + PlayerIconBoxSize, yPositionOnScreen + 4), sourceRectInfoDisplay[0], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .88f);
                        b.Draw(texture, new(xPositionOnScreen + PlayerIconBoxSize + PlayerInfoStartOffset, yPositionOnScreen + 4), sourceRectInfoDisplay[1], Color.White, 0f, Vector2.Zero, new Vector2(PlayerInfoAreaSize / 2, 4f), SpriteEffects.None, .88f);
                        b.Draw(texture, new(xPositionOnScreen + PlayerIconBoxSize + PlayerInfoStartOffset + PlayerInfoAreaSize, yPositionOnScreen + 4), sourceRectInfoDisplay[2], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .88f);
                        return;
                    }
                    b.Draw(texture, new(xPositionOnScreen, yPositionOnScreen + 4), sourceRectInfoDisplay[2], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, .88f);
                    b.Draw(texture, new(xPositionOnScreen + PlayerInfoEndSize, yPositionOnScreen + 4), sourceRectInfoDisplay[1], Color.White, 0f, Vector2.Zero, new Vector2(PlayerInfoAreaSize / 2, 4f), SpriteEffects.None, .88f);
                    b.Draw(texture, new(xPositionOnScreen + PlayerInfoEndSize + PlayerInfoAreaSize, yPositionOnScreen + 4), sourceRectInfoDisplay[2], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .88f);
                    return;
                case BoxPosition.CenterRight:
                case BoxPosition.BottomRight:
                    if (config.ShowPlayerIcon)
                    {
                        b.Draw(texture, new(xPositionOnScreen + PlayerInfoStartOffset, yPositionOnScreen), Who.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID ? sourceRectIconBackgroundSelf : sourceRectIconBackground, Color.White, 0f, Vector2.Zero, 4, SpriteEffects.None, .88f);
                        b.Draw(texture, new(xPositionOnScreen, yPositionOnScreen + 4), sourceRectInfoDisplay[0], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .88f);
                        b.Draw(texture, new(xPositionOnScreen - PlayerInfoAreaSize, yPositionOnScreen + 4), sourceRectInfoDisplay[1], Color.White, 0f, Vector2.Zero, new Vector2(PlayerInfoAreaSize / 2, 4f), SpriteEffects.None, .88f);
                        b.Draw(texture, new(xPositionOnScreen - PlayerInfoAreaSize - 20, yPositionOnScreen + 4), sourceRectInfoDisplay[2], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, .88f);
                        return;
                    }
                    b.Draw(texture, new(xPositionOnScreen + 64, yPositionOnScreen + 4), sourceRectInfoDisplay[2], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .88f);
                    b.Draw(texture, new(xPositionOnScreen - PlayerInfoAreaSize + 64, yPositionOnScreen + 4), sourceRectInfoDisplay[1], Color.White, 0f, Vector2.Zero, new Vector2(PlayerInfoAreaSize / 2, 4f), SpriteEffects.None, .88f);
                    b.Draw(texture, new(xPositionOnScreen - PlayerInfoAreaSize - 20 + 64, yPositionOnScreen + 4), sourceRectInfoDisplay[2], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, .88f);
                    return;
            }
        }

        private void drawPlayerIcon(SpriteBatch b, PlayerInfo info)
        {
            if (!config.ShowPlayerIcon)
                return;
            FarmerRenderer.isDrawingForUI = true;

            switch (config.Position)
            {
                case BoxPosition.TopLeft:
                case BoxPosition.BottomLeft:
                    Who.FarmerRenderer.drawMiniPortrat(b, new(xPositionOnScreen + 16, yPositionOnScreen + 15), .89f, 4f, 0, Who);
                    if (info.Health <= 0)
                    {
                        int size = PlayerIconBoxSize - (PlayerInfoStartOffset * 2);
                        b.Draw(Game1.fadeToBlackRect, new(xPositionOnScreen + PlayerInfoStartOffset, yPositionOnScreen + PlayerInfoStartOffset, size, size), new(0, 0, 1, 1), Color.Black * .6f, 0f, Vector2.Zero, SpriteEffects.None, .9f);
                        b.Draw(texture, new(xPositionOnScreen + 28, yPositionOnScreen + 36), sourceRectIconSkull, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .91f);
                    }
                    if (isPassingOut(info))
                    {
                        int size = PlayerIconBoxSize - (PlayerInfoStartOffset * 2);
                        b.Draw(Game1.fadeToBlackRect, new(xPositionOnScreen + PlayerInfoStartOffset, yPositionOnScreen + PlayerInfoStartOffset, size, size), new(0, 0, 1, 1), Color.Black * .6f, 0f, Vector2.Zero, SpriteEffects.None, .9f);
                        b.Draw(texture, new(xPositionOnScreen + 40, yPositionOnScreen + 39), sourceRectIconPassOut[0], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .91f);
                        b.Draw(texture, new(xPositionOnScreen + 32, yPositionOnScreen + 47), sourceRectIconPassOut[1], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .91f);
                        for (int x = 0; x < 2; x++)
                            for (int y = 0; y < 2; y++)
                                b.Draw(texture, new(xPositionOnScreen + 36 + (20 * x), yPositionOnScreen + 43 + (20 * y)), sourceRectIconPassOut[2], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .91f);
                    }
                    if (config.ShowHostCrown && Crown is not null && Game1.MasterPlayer.UniqueMultiplayerID == Who.UniqueMultiplayerID)
                        b.Draw(Crown, new(xPositionOnScreen -16, yPositionOnScreen + 16), sourceRectIconCrown, Color.White, -.8f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    break;
                case BoxPosition.CenterRight:
                case BoxPosition.BottomRight:
                    Who.FarmerRenderer.drawMiniPortrat(b, new(xPositionOnScreen + 28, yPositionOnScreen + 15), .89f, 4f, 0, Who);
                    if (info.Health <= 0)
                    {
                        int size = PlayerIconBoxSize - (PlayerInfoStartOffset * 2);
                        b.Draw(Game1.fadeToBlackRect, new(xPositionOnScreen + 24, yPositionOnScreen + PlayerInfoStartOffset, size, size), new(0, 0, 1, 1), Color.Black * .6f, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                        b.Draw(texture, new(xPositionOnScreen + 44, yPositionOnScreen + 39), sourceRectIconSkull, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.91f);
                    }
                    if (isPassingOut(info))
                    {
                        int size = PlayerIconBoxSize - (PlayerInfoStartOffset * 2); 
                        b.Draw(Game1.fadeToBlackRect, new(xPositionOnScreen + 24, yPositionOnScreen + PlayerInfoStartOffset, size, size), new(0, 0, 1, 1), Color.Black * .6f, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                        b.Draw(texture, new(xPositionOnScreen + 52, yPositionOnScreen + 39), sourceRectIconPassOut[0], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .91f);
                        b.Draw(texture, new(xPositionOnScreen + 44, yPositionOnScreen + 47), sourceRectIconPassOut[1], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .91f);
                        for (int x = 0; x < 2; x++)
                            for (int y = 0; y < 2; y++)
                                b.Draw(texture, new(xPositionOnScreen + 48 + (20 * x), yPositionOnScreen + 43 + (20 * y)), sourceRectIconPassOut[2], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, .91f);
                    }
                    if (config.ShowHostCrown && Crown is not null && Game1.MasterPlayer.UniqueMultiplayerID == Who.UniqueMultiplayerID)
                        b.Draw(Crown, new(xPositionOnScreen + 96, yPositionOnScreen - 14), sourceRectIconCrown, Color.White, .7f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
                    break;
            }

            FarmerRenderer.isDrawingForUI = false;
        }

        private void drawInfoText(SpriteBatch b, PlayerInfo info)
        {
            Color healthColor = info.Health <= info.MaxHealth / 10 ? Color.Red : (info.Health <= info.MaxHealth / 5 ? new(218, 128, 12) : Game1.textColor);
            Color staminaColor = info.Stamina <= info.MaxStamina / 10 ? Color.Red : (info.Stamina <= info.MaxStamina / 5 ? new(218, 128, 12) : Game1.textColor);
            bool hasHealthInfo = Who.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID || !context.TraditionalStaminaReporting.Contains(Who.UniqueMultiplayerID);
            switch (config.Position)
            {
                case BoxPosition.TopLeft:
                case BoxPosition.BottomLeft:
                    if (config.ShowPlayerIcon)
                    {
                        b.Draw(texture, new(xPositionOnScreen + PlayerIconBoxSize + 4, yPositionOnScreen + 4 + 26), sourceRectIconHealth, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, .89f);
                        if (!hasHealthInfo)
                            b.DrawString(Game1.smallFont, "???/???", new(xPositionOnScreen + PlayerIconBoxSize + 8 + 24, yPositionOnScreen + 4 + 20), healthColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);
                        else
                            b.DrawString(Game1.smallFont, $"{info.Health}/{info.MaxHealth}", new(xPositionOnScreen + PlayerIconBoxSize + 8 + 24, yPositionOnScreen + 4 + 20), healthColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);

                        b.Draw(texture, new(xPositionOnScreen + PlayerIconBoxSize + 4, yPositionOnScreen + 4 + 50), sourceRectIconEnergy, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, .89f);
                        b.DrawString(Game1.smallFont, $"{Math.Round(info.Stamina)}/{info.MaxStamina}", new(xPositionOnScreen + PlayerIconBoxSize + 8 + 24, yPositionOnScreen + 4 + 44), staminaColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);
                        return;
                    }
                    b.Draw(texture, new(xPositionOnScreen + PlayerInfoStartOffset * 2, yPositionOnScreen + 4 + 26), sourceRectIconHealth, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, .89f);
                    if (!hasHealthInfo)
                        b.DrawString(Game1.smallFont, "???/???", new(xPositionOnScreen + 8 + PlayerInfoStartOffset + 24, yPositionOnScreen + 4 + 20), healthColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);
                    else
                        b.DrawString(Game1.smallFont, $"{info.Health}/{info.MaxHealth}", new(xPositionOnScreen + 8 + PlayerInfoStartOffset + 24, yPositionOnScreen + 4 + 20), healthColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);

                    b.Draw(texture, new(xPositionOnScreen + PlayerInfoStartOffset * 2, yPositionOnScreen + 4 + 50), sourceRectIconEnergy, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, .89f);
                    b.DrawString(Game1.smallFont, $"{Math.Round(info.Stamina)}/{info.MaxStamina}", new(xPositionOnScreen + 8 + PlayerInfoStartOffset + 24, yPositionOnScreen + 4 + 44), staminaColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);
                    return;
                case BoxPosition.CenterRight:
                case BoxPosition.BottomRight:
                    if (config.ShowPlayerIcon)
                    {
                        b.Draw(texture, new(xPositionOnScreen - PlayerIconBoxSize - 10, yPositionOnScreen + 4 + 26), sourceRectIconHealth, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, .89f);
                        if (!hasHealthInfo)
                            b.DrawString(Game1.smallFont, "???/???", new(xPositionOnScreen - PlayerIconBoxSize + 10, yPositionOnScreen + 4 + 20), healthColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);
                        else
                            b.DrawString(Game1.smallFont, $"{info.Health}/{info.MaxHealth}", new(xPositionOnScreen - PlayerIconBoxSize + 10, yPositionOnScreen + 4 + 20), healthColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);

                        b.Draw(texture, new(xPositionOnScreen - PlayerIconBoxSize - 10, yPositionOnScreen + 4 + 50), sourceRectIconEnergy, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, .89f);
                        b.DrawString(Game1.smallFont, $"{info.Stamina}/{info.MaxStamina}", new(xPositionOnScreen - PlayerIconBoxSize + 10, yPositionOnScreen + 4 + 44), staminaColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);
                        return;
                    }
                    b.Draw(texture, new(xPositionOnScreen - PlayerInfoAreaSize + 4 + 64, yPositionOnScreen + 4 + 26), sourceRectIconHealth, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, .89f);
                    if (!hasHealthInfo)
                        b.DrawString(Game1.smallFont, "???/???", new(xPositionOnScreen - PlayerInfoAreaSize + 20 + 4 + 64, yPositionOnScreen + 4 + 20), healthColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);
                    else
                        b.DrawString(Game1.smallFont, $"{info.Health}/{info.MaxHealth}", new(xPositionOnScreen - PlayerInfoAreaSize + 20 + 4 + 64, yPositionOnScreen + 4 + 20), healthColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);

                    b.Draw(texture, new(xPositionOnScreen - PlayerInfoAreaSize + 4 + 64, yPositionOnScreen + 4 + 50), sourceRectIconEnergy, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, .89f);
                    b.DrawString(Game1.smallFont, $"{Math.Round{info.Stamina}}/{info.MaxStamina}", new(xPositionOnScreen - PlayerInfoAreaSize + 20 + 4 + 64, yPositionOnScreen + 4 + 44), staminaColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .89f);
                    return;
            }
        }
    }
}
