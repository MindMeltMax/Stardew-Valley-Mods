using StardewModdingAPI;
using System;

namespace MPInfo {
    public class Config {
        public bool Enabled { get; set; } = true;
        public bool ShowSelf { get; set; } = false;
        public bool ShowHostCrown { get; set; } = true;
        public bool HideHealthBars { get; set; } = true;
    }

    public interface IGenericModConfigMenuApi {
        void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false);

        void AddBoolOption(IManifest mod, Func<bool> getValue, Action<bool> setValue, Func<string> name, Func<string>? tooltip = null, string? fieldId = null);
    }
}
