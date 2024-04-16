using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishnets
{
    internal class Config
    {
        public int TextureVariant { get; set; } = 0;

        public bool LessTrash { get; set; } = false;

        public bool LessWeeds { get; set; } = false;

        public bool LessJelly { get; set; } = false;
    }
}
