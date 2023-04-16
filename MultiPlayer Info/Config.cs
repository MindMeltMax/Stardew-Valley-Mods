using StardewModdingAPI;
using System;

namespace MPInfo 
{
    public class Config 
    {
        public bool Enabled { get; set; } = true;
        public bool ShowSelf { get; set; } = false;
        public bool ShowHostCrown { get; set; } = true;
        public bool HideHealthBars { get; set; } = false;
        public Position Position { get; set; } = Position.BottomLeft;
    }

    public interface IGenericModConfigMenuApi 
    {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);

        void AddTextOption(IManifest mod, Func<string> getValue, Action<string> setValue, Func<string> name, Func<string> tooltip = null, string[] allowedValues = null, Func<string, string> formatAllowedValue = null, string fieldId = null);
    }
}
