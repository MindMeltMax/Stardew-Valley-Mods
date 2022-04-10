using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace BetterShipping
{
    internal class ModEntry : Mod
    {
        public static IModHelper IHelper;
        public static IMonitor IMonitor;

        public override void Entry(IModHelper helper)
        {
            IHelper = Helper;
            IMonitor = Monitor;

            Helper.Events.Display.WindowResized += OnResize;
            Helper.Events.Multiplayer.ModMessageReceived += OnModMessageReceived;

            HarmonyPather.Init(Helper);
        }

        private void OnModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.Type == "reloadItemsInBin")
                if (Game1.activeClickableMenu is not null and BinMenuOverride)
                    (Game1.activeClickableMenu as BinMenuOverride).loadItemsInView();
        }

        private void OnResize(object? sender, WindowResizedEventArgs e)
        {
            if (Game1.activeClickableMenu is not null and BinMenuOverride)
            {
                var offset = (Game1.activeClickableMenu as BinMenuOverride).Offset;
                Game1.activeClickableMenu = new BinMenuOverride(Helper, Monitor, offset);
            }
        }
    }
}
