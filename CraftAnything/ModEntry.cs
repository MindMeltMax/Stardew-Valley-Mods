using CraftAnything.Integrations.BetterCrafting;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace CraftAnything
{
    internal class ModEntry : Mod
    {
        public static string AssetPath => $"{IHelper.ModRegistry.ModID}/CraftResultTypes";

        internal static IModHelper IHelper;

        internal static IMonitor IMonitor;

        internal static Dictionary<string, string> CraftResultTypeCache = [];

        public override void Entry(IModHelper helper)
        {
            IHelper = Helper;
            IMonitor = Monitor;

            Helper.Events.GameLoop.GameLaunched += onGameLaunched;
            Helper.Events.Content.AssetRequested += onAssetRequested;
            Helper.Events.Content.AssetsInvalidated += onAssetInvalidated;
        }

        private void onGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            Patches.Patch(ModManifest.UniqueID);
            BetterCrafting.Register();
            CraftResultTypeCache = Helper.GameContent.Load<Dictionary<string, string>>(AssetPath);
        }

        private void onAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(AssetPath))
                e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
        }

        private void onAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo(AssetPath)))
            {
                CraftResultTypeCache = [];
                CraftResultTypeCache = Helper.GameContent.Load<Dictionary<string, string>>(AssetPath);
                CraftingRecipe.InitShared();
            }
        }
    }
}
