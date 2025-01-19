using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;

namespace MPInfo 
{
    public class Config 
    {
        public bool Enabled { get; set; } = true;

        public KeybindList ToggleButton { get; set; } = new(SButton.F6);
        public bool ShowSelf { get; set; } = false;
        public bool ShowHostCrown { get; set; } = true;
        public bool HideHealthBars { get; set; } = false;
        public bool ShowPlayerIcon { get; set; } = true;
        public Position Position { get; set; } = Position.BottomLeft;
        public int XOffset { get; set; } = 0;
        public int YOffset { get; set; } = 0;
        public int SpaceBetween { get; set; } = 112;
    }

    public interface IGenericModConfigMenuApi 
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);

        void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string> tooltip = null, string[] allowedValues = null, Func<string, string> formatAllowedValue = null, string fieldId = null);

        void AddNumberOption(IManifest mod, Func<int> getValue, Action<int> setValue, Func<string> name, Func<string> tooltip = null, int? min = null, int? max = null, int? interval = null, Func<int, string> formatValue = null, string fieldId = null);

        void AddKeybindList(IManifest mod, Func<KeybindList> getValue, Action<KeybindList> setValue, Func<string> name, Func<string> tooltip = null, string fieldId = null);
    }
}
