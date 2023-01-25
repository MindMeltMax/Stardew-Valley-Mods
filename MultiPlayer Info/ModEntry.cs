using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MPInfo
{
    internal class ModEntry : Mod
    {
        internal static IModHelper IHelper;
        internal static IMonitor IMonitor;
        internal static Config IConfig;

        private int lastMaxHealth;
        private int lastHealth;

        public override void Entry(IModHelper helper)
        {
            IHelper = Helper;
            IMonitor = Monitor;
            IConfig = Helper.ReadConfig<Config>();

            Helper.Events.Multiplayer.PeerConnected += onPlayerJoin;
            Helper.Events.Multiplayer.PeerDisconnected += onPlayerLeave;
            Helper.Events.Multiplayer.ModMessageReceived += onMultiplayerDataReceived;

            Helper.Events.GameLoop.SaveLoaded += (s, e) => { lastHealth = Game1.player.health; lastMaxHealth = Game1.player.maxHealth; ResetDisplays(); };
            Helper.Events.GameLoop.UpdateTicked += (s, e) => checkMyHealth();
        }

        internal static void UpdatePositions()
        {
            int index = 0;
            foreach (var pib in Game1.onScreenMenus.Where(x => x is PlayerInfoBox).OfType<PlayerInfoBox>())
            {
                pib.X = 32;
                pib.Y = Game1.uiViewport.Height - 32 - 96;
                pib.UpdatePosition(index);
                index++;
            }
        }

        internal static void ResetDisplays(int offsetIndex = 0)
        {
            var displays = Game1.onScreenMenus.Where(x => x is PlayerInfoBox).OfType<PlayerInfoBox>().ToArray();
            var reportedHealthList = new List<int>(displays.Select(x => x.Health));
            for (int i = 0; i < displays.Length; i++)
                Game1.onScreenMenus.Remove(displays[i]);

            int playerIndex = 0;
            foreach (var player in Game1.getOnlineFarmers())
            {
                if (player == Game1.player && !IConfig.ShowSelf)
                    continue;

                int index = offsetIndex;
                PlayerInfoBox display = new(32, Game1.uiViewport.Height - 32 - 96, player.UniqueMultiplayerID) { Health = playerIndex < reportedHealthList.Count ? reportedHealthList[playerIndex] : player.health };
                foreach (var pib in Game1.onScreenMenus.Where(x => x is PlayerInfoBox).OfType<PlayerInfoBox>())
                {
                    pib.UpdatePosition(index);
                    index++;
                }
                Game1.onScreenMenus.Add(display);
                playerIndex++;
                
            }
        }

        private void checkMyHealth()
        {
            if (!Context.IsWorldReady)
                return;

            if (Game1.player.health != lastHealth)
                Helper.Multiplayer.SendMessage(Game1.player.health, "MPInfo.Health", new[] { Helper.ModRegistry.ModID });
            if (Game1.player.maxHealth != lastMaxHealth)
                Helper.Multiplayer.SendMessage(Game1.player.maxHealth, "MPInfo.MaxHealth", new[] { Helper.ModRegistry.ModID });
        }

        private void onPlayerJoin(object? sender, PeerConnectedEventArgs e)
        {
            int index = 0;
            PlayerInfoBox display = new(32, Game1.uiViewport.Height - 32 - 96, e.Peer.PlayerID);
            foreach (var pib in Game1.onScreenMenus.Where(x => x is PlayerInfoBox).OfType<PlayerInfoBox>())
            {
                index++;
                pib.UpdatePosition(index);
            }
            Game1.onScreenMenus.Add(display);
        }
        private void onPlayerLeave(object? sender, PeerDisconnectedEventArgs e)
        {
            var display = Game1.onScreenMenus.FirstOrDefault(x => x is PlayerInfoBox pib && pib.Who.UniqueMultiplayerID == e.Peer.PlayerID);
            if (display is not null)
                Game1.onScreenMenus.Remove(display);
        }
        private void onMultiplayerDataReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID == Helper.ModRegistry.ModID)
            {
                if (e.Type == "MPInfo.Health")
                {
                    var display = (PlayerInfoBox?)Game1.onScreenMenus.FirstOrDefault(x => x is PlayerInfoBox pib && pib.Who.UniqueMultiplayerID == e.FromPlayerID);
                    if (display is not null)
                        display.Health = e.ReadAs<int>();
                }
                else if (e.Type == "MPInfo.MaxHealth")
                {
                    var display = (PlayerInfoBox?)Game1.onScreenMenus.FirstOrDefault(x => x is PlayerInfoBox pib && pib.Who.UniqueMultiplayerID == e.FromPlayerID);
                    if (display is not null)
                        display.MaxHealth = e.ReadAs<int>();
                }
            }
        }
    }
}
