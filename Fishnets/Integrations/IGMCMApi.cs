using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fishnets.Integrations
{
    public interface IGMCMApi
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);

        void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string> formatValue = null, string fieldId = null);
    }
}
