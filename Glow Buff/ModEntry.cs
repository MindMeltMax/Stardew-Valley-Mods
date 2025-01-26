using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Buffs;
using StardewValley.GameData.Objects;

namespace GlowBuff
{
    internal class ModEntry : Mod
    {
        internal static ModEntry Instance;
        internal LightSourceCache Cache;
        internal Texture2D HoverIcon;

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Cache = new();
            Helper.Events.GameLoop.GameLaunched += onGameLaunched;
            Helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            Helper.Events.Content.AssetsInvalidated += onAssetInvalidated;
            Helper.Events.Content.AssetRequested += onAssetRequested;
        }

        private void onGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            HoverIcon = Game1.content.Load<Texture2D>(ModManifest.UniqueID + "/HoverIcon");
            Patches.Patch(ModManifest.UniqueID);
        }

        private void onSaveLoaded(object? sender, SaveLoadedEventArgs e) => Cache.Clear();

        private void onAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo(ModManifest.UniqueID + "/HoverIcon")))
                HoverIcon = Game1.content.Load<Texture2D>(ModManifest.UniqueID + "/HoverIcon");
        }

        private void onAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data\\Buffs"))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, BuffData>().Data;
                    dict[ModManifest.UniqueID + "/Glow"] = new()
                    {
                        DisplayName = Helper.Translation.Get("Buff.DefaultName"),
                        Description = Helper.Translation.Get("Buff.DefaultDescription"),
                        Duration = -2,
                        IconTexture = ModManifest.UniqueID + "/BuffIcon",
                        IconSpriteIndex = 0,
                    };
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo(ModManifest.UniqueID + "/BuffIcon"))
                e.LoadFromModFile<Texture2D>("Assets/BuffIcon.png", AssetLoadPriority.Exclusive);
            if (e.NameWithoutLocale.IsEquivalentTo(ModManifest.UniqueID + "/HoverIcon"))
                e.LoadFromModFile<Texture2D>("Assets/HoverIcon.png", AssetLoadPriority.Exclusive);
        }
    }
}
