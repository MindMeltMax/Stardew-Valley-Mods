using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlowBuff
{
    public record LightSourceData
    {
        public int TextureId { get; set; }

        public int Radius { get; set; }

        public Color Color { get; set; }

        public int Duration { get; set; }

        public bool PrismaticColor { get; set; }

        public string Source { get; set; }
    }
}
