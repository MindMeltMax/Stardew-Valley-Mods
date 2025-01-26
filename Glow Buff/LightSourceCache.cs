using Microsoft.Xna.Framework;
using StardewValley;

namespace GlowBuff
{
    public class LightSourceCache
    {
        public Dictionary<long, string> FarmerToLightSourceId { get; }

        public Dictionary<string, LightSourceData> LightSourceIdToData { get; }

        public LightSourceCache()
        {
            FarmerToLightSourceId = [];
            LightSourceIdToData = [];
        }

        public void CreateOrUpdateLightSource(Farmer who, string id, LightSourceData data)
        {
            FarmerToLightSourceId[who.UniqueMultiplayerID] = id;
            LightSourceIdToData[id] = data;
            who.currentLocation.sharedLights[id] = new(id, data.TextureId, new(who.Position.X + 36f, who.Position.Y), data.Radius, data.Color, playerID: who.UniqueMultiplayerID);
        }

        public void RemoveLightSource(Farmer who, string id)
        {
            LightSourceIdToData.Remove(id);
            UpdateLocation(who, null, who.currentLocation);
            FarmerToLightSourceId.Remove(who.UniqueMultiplayerID);
        }

        public void Clear()
        {
            foreach (var id in FarmerToLightSourceId.Keys)
            {
                var player = Game1.GetPlayer(id)!;
                UpdateLocation(player, null, player.currentLocation);
            }
            FarmerToLightSourceId.Clear();
            LightSourceIdToData.Clear();
        }

        public void UpdateLocation(Farmer who, GameLocation? current, GameLocation old)
        {
            if (!FarmerToLightSourceId.TryGetValue(who.UniqueMultiplayerID, out string? id))
                return;
            old?.removeLightSource(id);
            if (current is not null && LightSourceIdToData.TryGetValue(id, out var data))
                current.sharedLights[id] = new(id, data.TextureId, new(who.Position.X + 36f, who.Position.Y), data.Radius, data.Color, playerID: who.UniqueMultiplayerID);
        }

        public void Tick(Farmer who, GameTime time)
        {
            if (!FarmerToLightSourceId.TryGetValue(who.UniqueMultiplayerID, out string? id) || LightSourceIdToData[id].Duration == -2)
                return;

            LightSourceIdToData[id].Duration -= time.ElapsedGameTime.Milliseconds;
            if (LightSourceIdToData[id].Duration <= 0)
                RemoveLightSource(who, id);
        }
    }
}
