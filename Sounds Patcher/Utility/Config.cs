using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace SoundsPatcher.Utility
{
    public class Config
    {
        public Dictionary<string, bool> Sounds { get; set; }

        public Dictionary<string, bool> Songs { get; set; }

        public Dictionary<string, bool> UnknownSounds { get; set; }

        public KeybindList MenuKeys { get; set; } = KeybindList.Parse("O, RightStick");

        public Config() { }

        public Config(Dictionary<string, bool> sounds, Dictionary<string, bool> songs, Dictionary<string, bool> unknownSounds, KeybindList key)
        {
            Sounds = sounds;
            Songs = songs;
            UnknownSounds = unknownSounds;
            MenuKeys = key;
        }
    }
}
