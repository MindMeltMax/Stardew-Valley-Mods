using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishnets.Data
{
    internal class ObjectInformation
    {
        public string Id { get; set; }

        public ObjectData Object { get; set; }

        public string Recipe { get; set; }
    }
}
