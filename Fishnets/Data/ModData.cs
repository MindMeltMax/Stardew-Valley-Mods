using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishnets.Data
{
    public record ModData
    {
        public Vector2 Offset { get; set; }

        public string BaitId { get; set; } = "";

        public int BaitQuality { get; set; } = 0;

        public ModData(Vector2 offset) => Offset = offset;
    }
}
