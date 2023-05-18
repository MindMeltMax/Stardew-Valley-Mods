using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CropWalker
{
    internal class Config
    {
        public bool FastGrass { get; set; } = true;

        public bool FastCrops { get; set; } = true;

        public bool PassableTrellis { get; set; } = true;
    }
}
