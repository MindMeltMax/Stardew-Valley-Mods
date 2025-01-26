using Microsoft.Xna.Framework;
using StardewModdingAPI;
using System.Reflection;

namespace GlowBuff
{
    public static class ColorParser
    {
        public static Color Read(string colorStr)
        {
            string[] rgba = colorStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (rgba.Length < 3 || rgba.Length > 4)
            {
                if (rgba.Length == 1)
                {
                    var prop = typeof(Color).GetProperty(rgba[0], BindingFlags.Public | BindingFlags.Static);
                    if (prop is not null && prop.GetMethod?.Invoke(null, null) is Color color)
                        return color;
                }
                ModEntry.Instance.Monitor.Log($"'{colorStr}' is not a valid color format and no predefined color with this name was found, using a white color as a default", LogLevel.Error);
                return Color.White;
            }

            float[] rgba_f = rgba.Select(x => float.Parse(x, System.Globalization.NumberStyles.Float)).ToArray();

            if (rgba_f.Length == 4)
                return new Color(rgba_f[0], rgba_f[1], rgba_f[2], rgba_f[3]);
            return new Color(rgba_f[0], rgba_f[1], rgba_f[2]);
        }
    }
}
