using Microsoft.Xna.Framework;
using StardewValley;

namespace GlowBuff
{
    public record LightSourceData
    {
        public int TextureId { get; set; } = 4;

        public float Radius { get; set; } = 1f;

        public Color Color { get; set; } = new(0, 50, 170);

        public int Duration { get; set; } = Buff.ENDLESS;

        public bool Prismatic { get; set; } = false;

        public string DisplayName { get; set; } = ModEntry.Instance.Helper.Translation.Get("Buff.DefaultName");

        public string Description { get; set; } = ModEntry.Instance.Helper.Translation.Get("Buff.DefaultDescription");
    }
}
