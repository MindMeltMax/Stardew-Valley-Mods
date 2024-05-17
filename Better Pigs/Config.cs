using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinterPigs
{
    public class Config
    {
        public List<string> AnimalsGoOutsideHorribleWeather { get; set; } = [];

        public List<string> AnimalsGoOutsideBadWeather { get; set; } = ["Pig"];

        public List<string> AnimalsStayInside { get; set; } = [];

        public bool DelayedPet { get; set; } = true;

        public double PigAnimalCrackerMultiplier { get; set; } = 2.0;

        public bool NoTruffleLimit { get; set; } = true;
    }
}
