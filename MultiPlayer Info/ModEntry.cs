using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Linq;

namespace MPInfo 
{
    internal class ModEntry : Mod 
    {
        internal static Config Config = null!;

        private int lastMaxHealth;
        private int lastHealth;

        public override void Entry(IModHelper helper) 
        {
            PlayerInfoBox.Crown = helper.ModContent.Load<Texture2D>("Assets/Crown.png");
            Config = helper.ReadConfig<Config>();

            helper.Events.GameLoop.GameLaunched += OnGameLaunched;

            helper.Events.Display.WindowResized += Display_WindowResized;

            helper.Events.Multiplayer.PeerConnected += OnPlayerJoin;
            helper.Events.Multiplayer.PeerDisconnected += OnPlayerLeave;
            helper.Events.Multiplayer.ModMessageReceived += OnMultiplayerDataReceived;

            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;

            Patches.Apply(ModManifest.UniqueID);
        }

        private void Display_WindowResized(object? sende, WindowResizedEventArgs e)
        {
            if (e.OldSize != e.NewSize)
                RepositionAll();
        }

        public static void RepositionAll()
        {
            var index = 0;
            foreach (var pib in Game1.onScreenMenus.Where(x => (x as PlayerInfoBox)?.Visible() ?? false).OfType<PlayerInfoBox>())
            {
                pib.xPositionOnScreen = 32;
                pib.yPositionOnScreen = (Game1.graphics.GraphicsDevice.Viewport.Height - 32 - 96) - (112 * index);
                index++;
            }
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) 
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new Config(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Enabled",
                tooltip: () => "",
                getValue: () => Config.Enabled,
                setValue: value => 
                {
                    Config.Enabled = value;
                    RepositionAll();
                }
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Self",
                tooltip: () => "",
                getValue: () => Config.ShowSelf,
                setValue: value => 
                {
                    Config.ShowSelf = value;
                    RepositionAll();
                }
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Host Crown",
                tooltip: () => "",
                getValue: () => Config.ShowHostCrown,
                setValue: value => Config.ShowHostCrown = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Hide Health and Stamina Bars",
                tooltip: () => "",
                getValue: () => Config.HideHealthBars,
                setValue: value => Config.HideHealthBars = value
            );
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e) 
        {
            lastHealth = Game1.player.health;
            lastMaxHealth = Game1.player.maxHealth;
            foreach (var player in Game1.getOnlineFarmers())
                Game1.onScreenMenus.Add(new PlayerInfoBox(player));
            RepositionAll();
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e) 
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.player.health != lastHealth) {
                lastHealth = Game1.player.health;
                Helper.Multiplayer.SendMessage(Game1.player.health, "MPInfo.Health", new[] { ModManifest.UniqueID });
            }
            if (Game1.player.maxHealth != lastMaxHealth) {
                lastMaxHealth = Game1.player.maxHealth;
                Helper.Multiplayer.SendMessage(Game1.player.maxHealth, "MPInfo.MaxHealth", new[] { ModManifest.UniqueID });
            }
        }

        private static void ModifyPlayerBox(long id, bool addPlayer)
        {
            var who = Game1.getFarmer(id);
            Game1.onScreenMenus
                .Where(x => x is PlayerInfoBox).OfType<PlayerInfoBox>()
                .Where(x => x.Who.UniqueMultiplayerID == who.UniqueMultiplayerID)
                .ToList()
                .ForEach(x => Game1.onScreenMenus.Remove(x));
            if (addPlayer)
                Game1.onScreenMenus.Add(new PlayerInfoBox(who));
            RepositionAll();
        }

        private void OnPlayerJoin(object? sender, PeerConnectedEventArgs e)
        {
            if (Game1.IsMasterGame)
            {
                ModifyPlayerBox(e.Peer.PlayerID, true);
                Helper.Multiplayer.SendMessage(e.Peer.PlayerID, "MPInfo.AddPlayer", new[] { ModManifest.UniqueID });
            }
        }

        private void OnPlayerLeave(object? sender, PeerDisconnectedEventArgs e)
        {
            if (Game1.IsMasterGame)
            {
                ModifyPlayerBox(e.Peer.PlayerID, false);
                Helper.Multiplayer.SendMessage(e.Peer.PlayerID, "MPInfo.RemovePlayer", new[] { ModManifest.UniqueID });
            }
        }

        private void OnMultiplayerDataReceived(object? sender, ModMessageReceivedEventArgs e) 
        {
            if (e.FromModID == Helper.ModRegistry.ModID) 
            {
                var who = Game1.getFarmer(e.FromPlayerID);
                if (who is not null)
                {
                    if (e.Type == "MPInfo.Health")
                    {
                        who.health = e.ReadAs<int>();
                    }
                    else if (e.Type == "MPInfo.MaxHealth")
                    {
                        who.maxHealth = e.ReadAs<int>();
                    }
                    else if (e.FromPlayerID == Game1.MasterPlayer.UniqueMultiplayerID)
                    {
                        who = Game1.getFarmer(e.ReadAs<long>());
                        if (who.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
                        {
                            if (e.Type == "MPInfo.AddPlayer")
                            {
                                ModifyPlayerBox(e.ReadAs<long>(), true);
                            }
                            else if (e.Type == "MPInfo.RemovePlayer")
                            {
                                ModifyPlayerBox(e.ReadAs<long>(), false);
                            }
                        }
                    }
                }
            }
        }
    }
}
