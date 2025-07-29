using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Globalization;
using System.Text;
using System.Xml;
using SObject = StardewValley.Object;

namespace MapBuildings
{
    internal class ModEntry : Mod
    {
        private static readonly List<int> categoryIgnoreKeys = [-15, -16, -79, -80, -81, -999];

        public override void Entry(IModHelper helper)
        {
            Helper.Events.Content.AssetRequested += onAssetRequested;
            Helper.Events.Content.AssetsInvalidated += onAssetInvalidated;
            Helper.Events.GameLoop.SaveLoaded += onSaveLoad;
            Helper.Events.World.LocationListChanged += onLocationListChange;
        }

        private void onLocationListChange(object? sender, LocationListChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if (e.Added is { } added)
                foreach (var item in added)
                    if (!item.IsOutdoors)
                        return;
            if (e.Removed is { } removed)
                foreach (var item in removed)
                    if (!item.IsOutdoors)
                        return;
            ReloadBuildings();
        }

        private void onSaveLoad(object? sender, SaveLoadedEventArgs e)
        {
            ParseOldBuildings();
            ReloadBuildings();
        }

        private void onAssetInvalidated(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo(ModManifest.UniqueID + "/Buildings")) && Context.IsWorldReady)
                ReloadBuildings();
        }

        private void onAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(ModManifest.UniqueID + "/Buildings"))
            {
                e.LoadFrom(() => new Dictionary<string, List<MapBuilding>>(), AssetLoadPriority.Exclusive);

                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, List<MapBuilding>>().Data;
                    dict[ModManifest.UniqueID] = [];
                    dict[ModManifest.UniqueID].Add(new()
                    {
                        Building = "Cabin",
                        X = 58,
                        Y = 23,
                        Location = "Farm"
                    });
                });
            }
        }

        public void ReloadBuildings()
        {
            var buildings = Helper.GameContent.Load<Dictionary<string, List<MapBuilding>>>(ModManifest.UniqueID + "/Buildings");
            var buildingData = DataLoader.Buildings(Game1.content);

            foreach (var item in buildings)
            {
                Monitor.Log($"Loading buildings for {item.Key}");
                int loaded = 0;

                foreach (var building in item.Value)
                {
                    try
                    {
                        var location = Game1.RequireLocation(building.Location);
                        Building? oldBuilding = null;
                        Building? current = null;

                        if (!location.IsBuildableLocation())
                            throw new($"Location {building.Location} does not accept buildings to be placed");

                        if (location.buildings.FirstOrDefault(x => x.tileX.Value == building.X && x.tileY.Value == building.Y && x.modData.ContainsKey(ModManifest.UniqueID + "_PlacedBy")) is Building b)
                        {
                            string placer = b.modData[ModManifest.UniqueID + "_PlacedBy"];
                            if (placer != item.Key)
                                throw new($"Tried to place building {building.Building} in {building.Location} at tile {{ X:{building.X}, Y:{building.Y} }}, but {placer} already has a building placed here.");
                            else
                            {
                                Monitor.Log($"Replacing building {b.GetData().Name} with {building.Building} at tile {{ X:{building.X}, Y:{building.Y} }} in {building.Location}");
                                oldBuilding = b;
                            }
                        }

                        string uniqueId = buildUniqueID(item.Key, building);
                        if (location.modData.TryGetValue(ModManifest.UniqueID + "/Placed", out string data))
                        {
                            if (data.Contains(uniqueId, StringComparison.OrdinalIgnoreCase))
                            {
                                Monitor.Log($"Tried to place building with id {building.Building} in {building.Location} at tile {{ X:{building.X}, Y:{building.Y} }} but it had already been placed before.");
                                continue;
                            }
                        }

                        if (!buildingData.TryGetValue(building.Building, out var blueprint))
                            throw new KeyNotFoundException($"Requested building {building.Building} was not found in existing building data.");

                        if (oldBuilding is not null)
                            location.buildings.Remove(oldBuilding);

                        if (building.Upgrade)
                        {
                            if (oldBuilding is null)
                                throw new($"Tried to upgrade building at {{ X:{building.X}, Y:{building.Y} }} in {building.Location} but no building was found there");
                            string oldName = oldBuilding.buildingType.Value;
                            if (blueprint.BuildingToUpgrade != oldName)
                                throw new($"Tried to upgrade building {oldName} at {{ X:{building.X}, Y:{building.Y} }} in {building.Location} but the provided upgrade ({building.Building}) is not a valid upgrade for this building");
                            oldBuilding.upgradeName.Value = building.Building;
                            oldBuilding.daysUntilUpgrade.Value = Math.Max(blueprint.BuildDays, 1);
                            oldBuilding.FinishConstruction(true); //Have to instantly finish upgrade construction, otherwise a players active construction might fail
                            oldBuilding.LoadFromBuildingData(blueprint, true, true);
                            oldBuilding.modData[ModManifest.UniqueID + "_PlacedBy"] = item.Key;
                            current = oldBuilding;
                        }
                        else
                        {
                            Rectangle buildingRect = new(new(building.X, building.Y), blueprint.Size);
                            if (IsAreaOccupied(location, new(building.X, building.Y), blueprint.Size, out var removeableObjects, out var removeableTerrainFeatures) && !building.Upgrade)
                                throw new($"The building ({building.Building}) to be placed in {building.Location} at tile {{ X:{building.X}, Y:{building.Y} }} with a tile size of {{ W:{blueprint.Size.X}, H:{blueprint.Size.Y} }} could not be placed since it's area overlaps with non-removable terrain or objects.");

                            Building structure = Building.CreateInstanceFromId(building.Building, new(building.X, building.Y));
                            structure.FinishConstruction(true);
                            structure.LoadFromBuildingData(blueprint, false, true);
                            structure.modData[ModManifest.UniqueID + "_PlacedBy"] = item.Key;
                            current = structure;

                            foreach (var obj in removeableObjects)
                                location.Objects.Remove(obj);

                            foreach (var tf in removeableTerrainFeatures)
                            {
                                if (location.getLargeTerrainFeatureAt((int)tf.X, (int)tf.Y) is { } ltf)
                                    location.largeTerrainFeatures.Remove(ltf);
                                if (location.terrainFeatures.ContainsKey(tf))
                                    location.terrainFeatures.Remove(tf);
                            }
                        }

                        if (current.GetIndoors() is { } indoors && building.Objects.Count > 0)
                        {
                            foreach (var obj in building.Objects)
                            {
                                var tile = parseVector(obj.Key);
                                if (!indoors.CanItemBePlacedHere(tile, ignorePassables: CollisionMask.Characters | CollisionMask.Farmers | CollisionMask.Flooring | CollisionMask.LocationSpecific))
                                {
                                    Monitor.Log($"Tried to place item with id {obj.Value} at tile {{ X:{building.X}, Y:{building.Y} }} in building {building.Building} but the tile was occupied", LogLevel.Warn);
                                    continue;
                                }
                                var placable = ItemRegistry.Create(obj.Value, allowNull: true);
                                if (placable is null)
                                {
                                    Monitor.Log($"Tried to place item with id {obj.Value} at tile {{ X:{building.X}, Y:{building.Y} }} in building {building.Building} but no item with this id was found", LogLevel.Warn);
                                    continue;
                                }
                                if (placable is not SObject o)
                                {
                                    Monitor.Log($"Tried to place item with id {obj.Value} at tile {{ X:{building.X}, Y:{building.Y} }} in building {building.Building} but this item is not placable", LogLevel.Warn);
                                    continue;
                                }
                                //Object.placementAction inconsistently expects me to add the object to the location
                                if (o.placementAction(indoors, (int)tile.X * 64, (int)tile.Y * 64) && !indoors.Objects.ContainsKey(tile))
                                    indoors.setObject(tile, o);
                            }
                        }

                        if (current.GetIndoors() is AnimalHouse animalHouse && blueprint.MaxOccupants > 0)
                        {
                            foreach (var animal in building.Animals)
                            {
                                if (animalHouse.isFull())
                                {
                                    Monitor.Log($"Tried to add animal {animal} to building {building.Building} in {building.Location} at tile {{ X:{building.X}, Y:{building.Y} }} but it was full, no more animals will be added to this building", LogLevel.Warn);
                                    break;
                                }
                                FarmAnimal farmAnimal = new(animal, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID)
                                {
                                    Name = Game1.random.NextBool(0.05) ? "Max" : Dialogue.randomName() // :SDVpuffersquee:
                                };
                                if (!blueprint.ValidOccupantTypes.Contains(farmAnimal.buildingTypeILiveIn.Value))
                                {
                                    Monitor.Log($"Tried to add animal {animal} to building {building.Building} in {building.Location} at tile {{ X:{building.X}, Y:{building.Y} }} but it did not fit to this building type, {getBuildingOccupantRejection(farmAnimal, blueprint)}. This animal was not added.", LogLevel.Warn);
                                    continue;
                                }
                                animalHouse.adoptAnimal(farmAnimal);
                            }
                        }

                        location.buildings.Add(current);

                        if (!building.Upgrade)
                        {
                            if (!location.buildStructure(current, new(building.X, building.Y), null, true))
                            {
                                location.buildings.Remove(current);
                                if (oldBuilding is not null)
                                {
                                    location.buildings.Add(oldBuilding);
                                    location.buildStructure(oldBuilding, new(building.X, building.Y), null, true);
                                }
                                throw new($"Tried to place {building.Building} in {building.Location} at {{ X:{building.X}, Y:{building.Y} }} but the game rejected it.");
                            }
                        }

                        //Fixes buildings like cabins
                        current.GetParentLocation().OnBuildingConstructed(current, null);

                        if (location.modData.ContainsKey(ModManifest.UniqueID + "/Placed"))
                            location.modData[ModManifest.UniqueID + "/Placed"] += $", {uniqueId}";
                        else
                            location.modData[ModManifest.UniqueID + "/Placed"] = uniqueId;

                        Monitor.Log($"Placed {building.Building} in {building.Location} at {{ X:{building.X}, Y:{building.Y} }}");
                        loaded++;
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log($"Failed placing building {building.Building} in {building.Location} at tile {{ X:{building.X}, Y:{building.Y} }}.", LogLevel.Error);
                        Monitor.Log($"[{ex.GetType().Name}] {ex.Message}\n{ex.StackTrace}", LogLevel.Error);
                        continue;
                    }
                }

                Monitor.Log($"Finished loading buildings for {item.Key}. {loaded} / {item.Value.Count} buildings were loaded.");
            }
        }

        public void ParseOldBuildings() //Gonna at least try to prevent anymore duplication
        {
            var buildings = Helper.GameContent.Load<Dictionary<string, List<MapBuilding>>>(ModManifest.UniqueID + "/Buildings");
            var buildingData = DataLoader.Buildings(Game1.content);

            foreach (var item in buildings)
            {
                foreach (var data in item.Value)
                {
                    var location = Game1.RequireLocation(data.Location);
                    if (location.getBuildingAt(new(data.X, data.Y)) is not { } building || !isBuildingOrUpgrade(buildingData, building, data))
                        continue;
                    if (!building.modData.TryGetValue(ModManifest.UniqueID + "_PlacedBy", out string placer) || placer != item.Key)
                        continue;

                    string uniqueId = buildUniqueID(item.Key, data);
                    if (location.modData.ContainsKey(ModManifest.UniqueID + "/Placed"))
                        location.modData[ModManifest.UniqueID + "/Placed"] += $", {uniqueId}";
                    else
                        location.modData[ModManifest.UniqueID + "/Placed"] = uniqueId;
                }
            }
        }

        public static bool IsAreaOccupied(GameLocation location, Point start, Point size, out List<Vector2> removeableObjects, out List<Vector2> removeableTerrainFeatures)
        {
            removeableObjects = [];
            removeableTerrainFeatures = [];
            for (int x = start.X; x < start.X + size.X; x++)
            {
                for (int y = start.Y; y < start.Y + size.Y; y++)
                {
                    Vector2 tile = new(x, y);
                    if (IsTileOccupied(location, tile, out var removeable, out var removeablTF))
                        return true;
                    if (removeable)
                        removeableObjects.Add(tile);
                    if (removeablTF)
                        removeableTerrainFeatures.Add(tile);
                }
            }
            return false;
        }

        public static bool IsTileOccupied(GameLocation location, Vector2 tile, out bool canBeRemoved, out bool canRemoveTerrainFeature)
        {
            canBeRemoved = false;
            canRemoveTerrainFeature = false;
            if (location.IsTileOccupiedBy(tile, CollisionMask.Buildings | CollisionMask.Furniture | CollisionMask.LocationSpecific) ||
                !location.isTilePlaceable(tile))
                return true;
            if (location.Objects.TryGetValue(tile, out var obj) && !categoryIgnoreKeys.Contains(obj.Category))
                return true;
            if (location.Objects.ContainsKey(tile))
                canBeRemoved = true;
            if (location.getLargeTerrainFeatureAt((int)tile.X, (int)tile.Y) is not null)
                canRemoveTerrainFeature = true;
            if (location.terrainFeatures.ContainsKey(tile))
            {
                var tf = location.terrainFeatures[tile];
                if (tf is FruitTree)
                    return true;
                if (tf is not Flooring and not Grass)
                    canRemoveTerrainFeature = true;
            }
            return false;
        }

        private string getBuildingOccupantRejection(FarmAnimal animal, BuildingData blueprint)
        {
            if ((blueprint.ValidOccupantTypes?.Count ?? 0) <= 0)
                return "this building does not accept any occupant types";
            if (blueprint.ValidOccupantTypes!.Count == 1)
                return $"this building only accepts occupants of type {blueprint.ValidOccupantTypes[0]}";
            return $"this building only accepts occupants of types: {string.Join(',', blueprint.ValidOccupantTypes)}";
        }

        private Vector2 parseVector(string tile)
        {
            string[] values = tile.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (values.Length == 0)
                return Vector2.Zero;
            if (values.Length == 1)
                return new Vector2(int.Parse(values[0]));
            return new Vector2(int.Parse(values[0]), int.Parse(values[1]));
        }

        private string buildUniqueID(string key, MapBuilding building)
        {
            return new StringBuilder(key).Append('_')
                                         .Append(building.Building)
                                         .Append('_')
                                         .Append(building.Location)
                                         .Append('_')
                                         .Append($"{building.X}-{building.Y}")
                                         .Append('_')
                                         .ToString();
        }

        private bool isBuildingOrUpgrade(Dictionary<string, BuildingData> buildings, Building building, MapBuilding data)
        {
            if (building.buildingType.Value == data.Building)
                return true;
            foreach (var item in buildings)
                if (building.buildingType.Value == item.Key && item.Value.BuildingToUpgrade == data.Building)
                    return true;
            return false;
        }
    }
}
