using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualityBait
{
    internal class Config
    {
        public int ChancePercentage { get; set; } = 75;

        [JsonIgnore]
        public double Chance => (double)ChancePercentage / 100.0;
    }
}
