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
    }

    public class Api : IApi
    {
        public int GetQuality(int currentQuality, int baitQuality) => ModEntry.GetQualityForCatch(currentQuality, baitQuality);
    }
}
