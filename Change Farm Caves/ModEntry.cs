using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;

namespace ChangeFarmCaves
{
    internal class ModEntry : Mod
    {
        internal static IModHelper IHelper;
        internal static IMonitor IMonitor;
        internal static ITranslationHelper ITranslations;
        internal static Config IConfig;

        internal static FarmCave FarmCave => Game1.RequireLocation<FarmCave>("FarmCave");

        public override void Entry(IModHelper helper)
        {
            IHelper = Helper;
            IMonitor = Monitor;
            ITranslations = Helper.Translation;
            IConfig = Helper.ReadConfig<Config>();

            Helper.Events.GameLoop.GameLaunched += (s, e) => Patches.Patch(helper);
            Helper.Events.GameLoop.DayStarted += (s, e) =>
            {
                if (!Game1.IsMasterGame) 
                    return;
                if (FarmCave.modData.TryGetValue("ChangeFarmCaves.ShouldChange", out string data))
                {
                    new Event().answerDialogue(data.Split(',')[0], Convert.ToInt32(data.Split(',')[1]));
                    FarmCave.modData.Remove("ChangeFarmCaves.ShouldChange");
                }
            };
        }
    }
}
