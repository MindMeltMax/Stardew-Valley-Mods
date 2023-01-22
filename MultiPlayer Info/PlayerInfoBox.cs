using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MPInfo
{
    public class PlayerInfoBox : IClickableMenu
    {
        public int X
        {
            get => xPositionOnScreen;
            set => xPositionOnScreen = value;
        }
        public int Y
        {
            get => yPositionOnScreen;
            set => yPositionOnScreen = value;
        }
        public int Width
        {
            get => width;
            set => width = value;
        }
        public int Height
        {
            get => height;
            set => height = value;
        }

        public int Health { get; set; } //Stamina is reported accross multiplayer but health isn't...

        public int MaxHealth { get; set; } //Neither is max health...

        public Farmer Who => Game1.getFarmer(id);

        public Texture2D Texture => Game1.mouseCursors;

        public Texture2D Crown { get; init; } //I know it's not great, but I'm a developer not an artist

        public Rectangle SourceRectIconBackground => new(293, 360, 24, 24);

        public Rectangle[] SourceRectInfoDisplay => new[]
        {
            new Rectangle(317, 361, 3, 22), //Left
            new Rectangle(320, 361, 2, 22), //Middle (Expands based on given info)
            new Rectangle(322, 361, 7, 22)  //Right
        };

        public Rectangle[] SourceRectIconPassOut => new[]
        {
            new Rectangle(195, 408, 4, 8), //Vertical
            new Rectangle(193, 410, 8, 4), //Horizontal
            new Rectangle(194, 409, 1, 1)  //Corner
        };

        public Rectangle SourceRectIconEnergy => new(0, 428, 10, 10);

        public Rectangle SourceRectIconHealth => new(0, 438, 10, 10);

        public Rectangle SourceRectIconSkull => new(140, 428, 10, 10);

        public Rectangle SourceRectIconCrown => new(0, 0, 9, 7);

        private long id;
        private string hoverText;

        public PlayerInfoBox(int x, int y, long who)
        {
            Crown = ModEntry.IHelper.ModContent.Load<Texture2D>("Assets/Crown.png");
            id = who;
            X = x;
            Y = y;
            Width = 96 + 12 + 112 + 28;
            Height = 96;
            Health = Who.health;
            MaxHealth = Who.maxHealth;
        }

        public void UpdatePosition(int index) => Y -= (112 * index);

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            if (new Rectangle(X, Y, 96, 96).Contains(x, y))
                hoverText = $"{Who.Name}{(Game1.serverHost.Value.UniqueMultiplayerID == id ? " (Host)" : "")}";
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (new Rectangle(X, Y, 96, 96).Contains(x, y))
                Who.takeDamage(5, false, new Ghost());
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            if (oldBounds != newBounds)
                ModEntry.UpdatePositions();
        }

        public override void draw(SpriteBatch b)
        {
            base.draw(b);
            b.Draw(Texture, new(X, Y), SourceRectIconBackground, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            b.Draw(Texture, new(X + 96, Y + 4), SourceRectInfoDisplay[0], Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
            b.Draw(Texture, new(X + 96 + 12, Y + 4), SourceRectInfoDisplay[1], Color.White, 0.0f, Vector2.Zero, new Vector2(56f, 4f), SpriteEffects.None, 0.88f);
            b.Draw(Texture, new(X + 96 + 12 + 112, Y + 4), SourceRectInfoDisplay[2], Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);

            FarmerRenderer.isDrawingForUI = true;
            Who.FarmerRenderer.drawMiniPortrat(b, new(X + 16, Y + 15), 0.89f, 4f, 0, Who);
            if (Health <= 0) //Icon to display being knocked out
            {
                b.Draw(Game1.fadeToBlackRect, new(X + 12, Y + 12, 72, 72), new(0, 0, 1, 1), Color.Black * 0.6f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                b.Draw(Texture, new(X + 32, Y + 39), SourceRectIconSkull, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
            }
            if (Who.passedOut || Who.FarmerSprite.isPassingOut()) //Icon to display passing out
            {
                b.Draw(Game1.fadeToBlackRect, new(X + 12, Y + 12, 72, 72), new(0, 0, 1, 1), Color.Black * 0.6f, 0.0f, Vector2.Zero, SpriteEffects.None, 0.9f);
                b.Draw(Texture, new(X + 40, Y + 39), SourceRectIconPassOut[0], Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
                b.Draw(Texture, new(X + 32, Y + 47), SourceRectIconPassOut[1], Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
                for (int i = 0; i < 2; i++) //Yes the corners are necessary
                    for (int j = 0; j < 2; j++)
                        b.Draw(Texture, new(X + 36 + (20 * j), Y + 43 + (20 * i)), SourceRectIconPassOut[2], Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
            }
            if (Game1.serverHost.Value.UniqueMultiplayerID == id)
                b.Draw(Crown, new(X - 16, Y + 16), SourceRectIconCrown, Color.White, -.8f, Vector2.Zero, 4f, SpriteEffects.None, 0.91f);
            FarmerRenderer.isDrawingForUI = false;

            b.Draw(Texture, new(X + 96 + 4, Y + 4 + 26), SourceRectIconHealth, Color.White, 0.0f, Vector2.Zero, 2f, SpriteEffects.None, 0.89f);
            b.DrawString(Game1.smallFont, $"{Health}/{MaxHealth}", new(X + 96 + 8 + 24, Y + 4 + 20), (Health <= (float)(MaxHealth / 10)) ? Color.Red : (Health <= (float)(MaxHealth / 5) ? Color.Yellow : Game1.textColor), 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.89f);
            b.Draw(Texture, new(X + 96 + 4, Y + 4 + 50), SourceRectIconEnergy, Color.White, 0.0f, Vector2.Zero, 2f, SpriteEffects.None, 0.89f);
            b.DrawString(Game1.smallFont, $"{Who.stamina}/{Who.maxStamina}", new(X + 96 + 8 + 24, Y + 4 + 44), (Who.stamina <= (float)(Who.maxStamina.Value / 10)) ? Color.Red : (Who.stamina <= (float)(Who.maxStamina.Value / 5) ? Color.Yellow : Game1.textColor), 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.89f);
            if (!string.IsNullOrWhiteSpace(hoverText))
            {
                drawHoverText(b, hoverText, Game1.smallFont);
                hoverText = "";
            }
        }
    }
}
