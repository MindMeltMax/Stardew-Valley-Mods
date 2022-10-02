using StardewModdingAPI;
using StardewValley;

namespace ChangeFarmCaves
{
    internal class ModEntry : Mod
    {
        internal static IModHelper IHelper;
        internal static IMonitor IMonitor;
        internal static ITranslationHelper ITranslations;

        public override void Entry(IModHelper helper)
        {
            IHelper = Helper;
            IMonitor = Monitor;
            ITranslations = Helper.Translation;

            Helper.Events.GameLoop.GameLaunched += (s, e) => Patcher.Patch(helper);
        }
    }
}
