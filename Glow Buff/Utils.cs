using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Objects;
using StardewValley.TokenizableStrings;
using System.Diagnostics.CodeAnalysis;

namespace GlowBuff
{
    internal static class Utils
    {
        private static ModEntry context => ModEntry.Instance;
        private static string ModId => context.ModManifest.UniqueID;

        public static LightSourceData? TryGetBuffFromItem(Item? item)
        {
            if (!HasBuffs(item, out var data))
                return null;

            LightSourceData? result = null;

            foreach (var buff in data.Buffs)
            {
                if (!IsGlowBuff(buff))
                    continue;
                result = new();
                if (DataIds.HasAOrB(buff.CustomFields.Keys, DataIds.Texture, DataIds.TextureAlt, out string? textureKey) && int.TryParse(buff.CustomFields[textureKey], out int textureId))
                    result.TextureId = textureId;
                if (DataIds.HasAOrB(buff.CustomFields.Keys, DataIds.Radius, DataIds.RadiusAlt, out string? radiusKey) && float.TryParse(buff.CustomFields[radiusKey], out float radius))
                    result.Radius = radius;
                if (DataIds.HasAOrB(buff.CustomFields.Keys, DataIds.Color, DataIds.ColorAlt, out string? colorKey))
                {
                    if (buff.CustomFields[colorKey].Equals("prismatic", StringComparison.OrdinalIgnoreCase))
                        result.Prismatic = true;
                    else
                    {
                        Color c = ColorParser.Read(buff.CustomFields[colorKey]);
                        result.Color = new(c.R ^ 255, c.G ^ 255, c.B ^ 255, c.A);
                    }
                }
                if (buff.CustomFields.TryGetValue(DataIds.DisplayName, out string? displayName))
                    result.DisplayName = TokenParser.ParseText(displayName);
                if (buff.CustomFields.TryGetValue(DataIds.Description, out string? description))
                    result.Description = TokenParser.ParseText(description);

                result.Duration = buff.Duration;
                if (result.Duration != -2)
                    result.Duration *= Game1.realMilliSecondsPerGameMinute;
            }

            return result;
        }

        public static void TryUpdateBuffIconArray(Item hoverItem, ref string[] buffIconArray) //Force the buff icon array to not be empty for hover if the item has a glow buff
        {
            if (!HasBuffs(hoverItem, out var data) || (buffIconArray is not null && buffIconArray.Length > 12 && int.TryParse(buffIconArray[12], out var time) && time > 0))
                return;
            foreach (var buff in data.Buffs)
            {
                if (!IsGlowBuff(buff))
                    continue;
                buffIconArray = new BuffEffects(buff.CustomAttributes).ToLegacyAttributeFormat();
                buffIconArray[12] = " " + Utility.getMinutesSecondsStringFromMilliseconds(buff.Duration * Game1.realMilliSecondsPerGameMinute);
                return;
            }
        }

        public static void TryDrawGlowBuffHoverIcon(SpriteBatch b, SpriteFont font, int x, ref int y, Item hoverItem)
        {
            if (!HasBuffs(hoverItem, out var data))
                return;

            foreach (var buff in data.Buffs)
            {
                if (!IsGlowBuff(buff))
                    continue;
                Utility.drawWithShadow(b, context.HoverIcon, new(x + 16 + 4, y + 16), new(0, 0, 10, 10), Color.White, 0f, Vector2.Zero, 3f, false, .95f);
                Utility.drawTextWithShadow(b, GetHoverText(buff), font, new(x + 16 + 34 + 4, y + 16), Game1.textColor);
                y += 39;
            }
        }

        public static void TryUpdateHoverBoxHeight(Item hoverItem, ref int height)
        {
            if (!HasBuffs(hoverItem, out var data))
                return;
            
            foreach (var buff in data.Buffs)
            {
                if (!IsGlowBuff(buff))
                    continue;
                height += 39;
            }
        }

        public static void TryUpdateHoverBoxWidth(Item hoverItem, SpriteFont font, int horizontalBuffer, ref int width)
        {
            if (!HasBuffs(hoverItem, out var data))
                return;

            foreach (var buff in data.Buffs)
            {
                if (!IsGlowBuff(buff))
                    continue;
                width = (int)Math.Max(width, font.MeasureString(GetHoverText(buff)).X + horizontalBuffer);
            }
        }

        private static bool HasBuffs(Item? item, [NotNullWhen(true)] out ObjectData? data)
        {
            data = null;
            return item is not null &&
                   Game1.objectData.TryGetValue(item.ItemId, out data) &&
                   (data.Buffs?.Any() ?? false);
        }

        private static bool IsGlowBuff(ObjectBuffData buff) => buff is not null && (buff.BuffId == ModId + "/Glow" || (buff.CustomFields?.ContainsKey(ModId + "/Glow") ?? false));

        private static string GetHoverText(ObjectBuffData buff)
        {
            if (!buff.CustomFields.ContainsKey(DataIds.HoverText))
                return $"{GetHoverStrengthString(buff)}{GetDisplayName(buff)}";
            return TokenParser.ParseText(buff.CustomFields[DataIds.HoverText]);
        }

        private static string GetHoverStrengthString(ObjectBuffData buff)
        {
            if (!DataIds.HasAOrB(buff.CustomFields.Keys, DataIds.Radius, DataIds.RadiusAlt, out string? radiusKey) || !float.TryParse(buff.CustomFields[radiusKey], out float radius))
                return "";
            int val = (int)Math.Round(radius);
            return $"+{(val < 1 ? 1 : val)} ";
        }

        private static string GetDisplayName(ObjectBuffData buff)
        {
            if (buff.CustomFields.TryGetValue(DataIds.DisplayName, out var displayName))
                return TokenParser.ParseText(displayName);
            return context.Helper.Translation.Get("Buff.DefaultName");
        }
    }
}
