using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishnets.Integrations
{
    public interface IQualityBaitApi
    {
        int GetQuality(int currentQuality, int baitQuality);
    }
}
