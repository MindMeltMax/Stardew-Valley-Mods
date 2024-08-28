using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapBuildings
{
    public class MapBuilding
    {
        public string Location { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public string Building { get; set; }

        public List<string> Animals { get; set; } = [];
    }
}
