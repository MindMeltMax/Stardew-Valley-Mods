using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QualityBait
{
    public interface IApi
    {
        int GetQuality(int currentQuality, int baitQuality);

        int GetQuality(string itemId, int currentQuality, int baitQuality);
    }

    public class Api : IApi
    {
        public int GetQuality(int currentQuality, int baitQuality) => ModEntry.GetQualityForBait(currentQuality, baitQuality); //Use sunfish id for simplicity

        public int GetQuality(string itemId, int currentQuality, int baitQuality) => ModEntry.GetQualityForCatch(itemId, currentQuality, baitQuality);
    }
}
