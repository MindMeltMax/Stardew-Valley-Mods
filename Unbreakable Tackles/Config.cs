using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unbreakable_Tackles
{
    public class Config
    {
        public bool consumeBait { get; set; } = false;

        public Config() { }

        public Config(bool bait)
        {
            consumeBait = bait;
        }
    }
}
