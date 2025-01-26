using System.Diagnostics.CodeAnalysis;

namespace GlowBuff
{
    internal static class DataIds
    {
        private static string ModId => ModEntry.Instance.ModManifest.UniqueID;

        public static string Texture => ModId + "/GlowTexture";

        public static string TextureAlt => ModId + "/Texture";

        public static string Radius => ModId + "/GlowRadius";

        public static string RadiusAlt => ModId + "/Radius";

        public static string Color => ModId + "/GlowColor";

        public static string ColorAlt => ModId + "/Color";

        public static string DisplayName => ModId + "/DisplayName";

        public static string Description => ModId + "/Description";

        public static string HoverText => ModId + "/HoverText";

        public static bool HasAOrB(IEnumerable<string> keys, string a, string b, [NotNullWhen(true)] out string? key)
        {
            key = null;
            foreach (var item in keys)
            {
                if (item == a || item == b)
                {
                    key = item;
                    return true;
                }
            }
            return false;
        }
    }
}
