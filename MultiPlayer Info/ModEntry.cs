using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MPInfo
{
    public enum Position
    {
        TopLeft,
        BottomLeft,
        BottomRight,
        CenterRight,
    }

    internal class ModEntry : Mod
    {
        internal static ModEntry Instance;
        internal static Config Config;

        public bool IsEnabled => Config.Enabled;
        public Dictionary<long, int> PingsSinceLastPong { get; } = [];
        public Dictionary<long, PlayerInfo?> PlayerData { get; } = [];

        public List<long> TraditionalStaminaReporting { get; } = [];

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            Config = Helper.ReadConfig<Config>();

            Helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            Helper.Events.GameLoop.GameLaunched += onGameLaunch;
            Helper.Events.GameLoop.ReturnedToTitle += onReturnToTitle;
            Helper.Events.GameLoop.DayStarted += onDayStarted;
            Helper.Events.GameLoop.OneSecondUpdateTicked += onOneSecondTicked;

            Helper.Events.Input.ButtonPressed += onButtonPressed;

            Helper.Events.Multiplayer.PeerConnected += onPlayerJoin;
            Helper.Events.Multiplayer.PeerDisconnected += onPlayerLeave;
            Helper.Events.Multiplayer.ModMessageReceived += onModMessageReceived;

            Helper.Events.Content.AssetRequested += onAssetRequested;
            Helper.Events.Content.AssetsInvalidated += onAssetInvalidated;
        }

        public void Toggle(bool value)
        {
            Config.Enabled = value;
            ResetDisplays();
        }

        public void Ping()
        {
            if (!Context.IsWorldReady)
                return;
            List<long> playersToPing = [];
            foreach (var p in PingsSinceLastPong.Keys)
            {
                if (PingsSinceLastPong[p] > 30 || p == Game1.player.UniqueMultiplayerID)
                    continue;
                if (PingsSinceLastPong[p] == 30)
                {
                    Monitor.Log($"Send 30 packets to {Game1.GetPlayer(p)?.Name ?? "Player " + p.ToString()} but they still haven't responded. Messaging suspended");
                    PingsSinceLastPong[p] = 31;
                    TraditionalStaminaReporting.Add(p);
                    continue;
                }
                PingsSinceLastPong[p]++;
                playersToPing.Add(p);
            }
            PlayerInfo packet = new(Game1.player);
            Helper.Multiplayer.SendMessage(packet.Serialize(), "MPInfo.Ping", [ModManifest.UniqueID], [.. playersToPing]);
        }

        public void Pong(long sender)
        {
            if (!Context.IsWorldReady)
                return;
            PlayerInfo packet = new(Game1.player);
            Helper.Multiplayer.SendMessage(packet.Serialize(), "MPInfo.Pong", [ModManifest.UniqueID], [sender]);
        }

        public void ForceUpdate()
        {
            Monitor.Log("Forcing an update, Re-enabling pings for all players");
            foreach (var p in PingsSinceLastPong.Keys)
            {
                PingsSinceLastPong[p] = 0;
                PlayerData[p] = new(Game1.GetPlayer(p));
            }
            TraditionalStaminaReporting.Clear();
            Ping();

        }

        public void ResetDisplays()
        {
            Game1.onScreenMenus = new List<IClickableMenu>(Game1.onScreenMenus.Where(x => x is not PlayerInfoBox));
            int beforeIndex = Game1.onScreenMenus.IndexOf(Game1.onScreenMenus.FirstOrDefault(x => x is Toolbar));
            foreach (var p in PlayerData.Keys)
            {
                if (beforeIndex != -1)
                    Game1.onScreenMenus.Insert(beforeIndex, new PlayerInfoBox(Game1.GetPlayer(p)));
                else
                    Game1.onScreenMenus.Add(new PlayerInfoBox(Game1.GetPlayer(p)));
            }
            PlayerInfoBox.FixPositions();
        }

        private void onSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            PingsSinceLastPong[Game1.player.UniqueMultiplayerID] = 0;
            PlayerData[Game1.player.UniqueMultiplayerID] = new(Game1.player);
        }

        private void onGameLaunch(object? sender, GameLaunchedEventArgs e)
        {
            Patches.Apply(ModManifest.UniqueID);
            PlayerInfoBox.Crown = Game1.content.Load<Texture2D>("MPInfo/Crown");

            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.Enabled.Name"),
                tooltip: () => "",
                getValue: () => Config.Enabled,
                setValue: Toggle
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.ToggleButton.Name"),
                tooltip: () => Helper.Translation.Get("Config.ToggleButton.Description"),
                getValue: () => Config.ToggleButton,
                setValue: value => Config.ToggleButton = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.ShowSelf.Name"),
                tooltip: () => Helper.Translation.Get("Config.ShowSelf.Description"),
                getValue: () => Config.ShowSelf,
                setValue: value =>
                {
                    Config.ShowSelf = value;
                    ResetDisplays();
                }
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.ShowHostCrown.Name"),
                tooltip: () => Helper.Translation.Get("Config.ShowHostCrown.Description"),
                getValue: () => Config.ShowHostCrown,
                setValue: value => Config.ShowHostCrown = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.ShowPlayerIcon.Name"),
                tooltip: () => Helper.Translation.Get("Config.ShowPlayerIcon.Description"),
                getValue: () => Config.ShowPlayerIcon,
                setValue: value =>
                {
                    Config.ShowPlayerIcon = value;
                    ResetDisplays();
                }
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.HideBars.Name"),
                tooltip: () => "",
                getValue: () => Config.HideHealthBars,
                setValue: value => Config.HideHealthBars = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.PositionBoxes.Name"),
                tooltip: () => Helper.Translation.Get("Config.PositionBoxes.Description"),
                getValue: () => Enum.GetName(Config.Position)!,
                setValue: value =>
                {
                    Config.Position = Enum.Parse<Position>(value);
                    PlayerInfoBox.FixPositions();
                },
                allowedValues: Enum.GetNames<Position>()
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.XOffset.Name"),
                tooltip: () => Helper.Translation.Get("Config.XOffset.Description"),
                getValue: () => Config.XOffset,
                setValue: value =>
                {
                    Config.XOffset = value;
                    PlayerInfoBox.FixPositions();
                }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.YOffset.Name"),
                tooltip: () => Helper.Translation.Get("Config.YOffset.Description"),
                getValue: () => Config.YOffset,
                setValue: value =>
                {
                    Config.YOffset = value;
                    PlayerInfoBox.FixPositions();
                }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => Helper.Translation.Get("Config.SpaceBetween.Name"),
                tooltip: () => Helper.Translation.Get("Config.SpaceBetween.Description"),
                getValue: () => Config.SpaceBetween,
                setValue: value =>
                {
                    Config.SpaceBetween = value;
                    PlayerInfoBox.FixPositions();
                }
            );
        }

        private void onReturnToTitle(object? sender, ReturnedToTitleEventArgs e)
        {
            PingsSinceLastPong.Clear();
            PlayerData.Clear();
            TraditionalStaminaReporting.Clear();
        }

        private void onDayStarted(object? sender, DayStartedEventArgs e)
        {
            foreach (var p in PlayerData.Keys)
                PlayerData[p] = new(Game1.GetPlayer(p));
            Ping();
            ResetDisplays();
        }

        private void onOneSecondTicked(object? sender, OneSecondUpdateTickedEventArgs e) => Ping();

        private void onButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (Config.ToggleButton.JustPressed())
                Toggle(!IsEnabled);
        }

        private void onPlayerJoin(object? sender, PeerConnectedEventArgs e)
        {
            if (Game1.MasterPlayer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
                return;
            var peers = Helper.Multiplayer.GetConnectedPlayers().Select(x => x.PlayerID).Where(x => x != e.Peer.PlayerID && x != Game1.player.UniqueMultiplayerID).ToArray();
            Helper.Multiplayer.SendMessage(e.Peer.PlayerID, "MPInfo.Joined", [ModManifest.UniqueID], peers);
            PingsSinceLastPong[e.Peer.PlayerID] = 0;
            PlayerData[e.Peer.PlayerID] = null;
            Ping();
            ResetDisplays();
        }

        private void onPlayerLeave(object? sender, PeerDisconnectedEventArgs e)
        {
            if (Game1.MasterPlayer.UniqueMultiplayerID != Game1.player.UniqueMultiplayerID)
                return;
            var peers = Helper.Multiplayer.GetConnectedPlayers().Select(x => x.PlayerID).Where(x => x != e.Peer.PlayerID && x != Game1.player.UniqueMultiplayerID).ToArray();
            Helper.Multiplayer.SendMessage(e.Peer.PlayerID, "MPInfo.Left", [ModManifest.UniqueID], peers);
            PingsSinceLastPong.Remove(e.Peer.PlayerID);
            PlayerData.Remove(e.Peer.PlayerID);
            TraditionalStaminaReporting.Remove(e.Peer.PlayerID);
            ResetDisplays();
        }

        private void onModMessageReceived(object? sender, ModMessageReceivedEventArgs e)
        {
            if (e.FromModID != ModManifest.UniqueID || e.FromPlayerID == Game1.player.UniqueMultiplayerID)
                return;
            string packet = "";
            long player = -1;
            switch (e.Type)
            {
                case "MPInfo.Ping":
                    packet = e.ReadAs<string>();
                    PlayerData[e.FromPlayerID] = PlayerInfo.Deserialize(packet);
                    Pong(e.FromPlayerID);
                    break;
                case "MPInfo.Pong":
                    packet = e.ReadAs<string>();
                    PlayerData[e.FromPlayerID] = PlayerInfo.Deserialize(packet);
                    PingsSinceLastPong[e.FromPlayerID] = 0;
                    break;
                case "MPInfo.Joined":
                    player = e.ReadAs<long>();
                    PingsSinceLastPong[player] = 0;
                    PlayerData[player] = null;
                    Ping();
                    ResetDisplays();
                    break;
                case "MPInfo.Left":
                    player = e.ReadAs<long>();
                    PingsSinceLastPong.Remove(player);
                    PlayerData.Remove(player);
                    TraditionalStaminaReporting.Remove(player);
                    ResetDisplays();
                    break;
            }
        }

        private void onAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("MPInfo/Crown"))
                e.LoadFromModFile<Texture2D>("Assets/Crown.png", AssetLoadPriority.Exclusive);
        }

        private void onAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo("MPInfo/Crown")))
                PlayerInfoBox.Crown = Game1.content.Load<Texture2D>("MPInfo/Crown");
        }
    }
}
