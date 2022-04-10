using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unbreakable_Tackles.Harmony;

namespace Unbreakable_Tackles
{
    public class ModEntry : Mod
    {
        public static IModHelper IHelper;
        public static IMonitor IMonitor;
        public static Config IConfig;

        public override void Entry(IModHelper helper)
        {
            IHelper = Helper;
            IMonitor = Monitor;
            IConfig = helper.ReadConfig<Config>();

            helper.Events.GameLoop.GameLaunched += (s, e) => Patcher.Init();
        }
    }
}
