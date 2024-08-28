using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;

namespace MapBuildings
{
    internal class ModEntry : Mod
    {
        private static readonly List<int> categoryIgnoreKeys = [-15, -16, -79, -80, -81, -999];

        public override void Entry(IModHelper helper)
        {
            Helper.Events.Content.AssetRequested += onAssetRequested;
            Helper.Events.Content.AssetsInvalidated += onAssetRequested;
            Helper.Events.GameLoop.SaveLoaded += onSaveLoad;
            Helper.Events.World.LocationListChanged += onLocationListChange;
        }

        private void onLocationListChange(object? sender, LocationListChangedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            ReloadBuildings();
        }

        private void onSaveLoad(object? sender, SaveLoadedEventArgs e) => ReloadBuildings();

        private void onAssetRequested(object? sender, AssetsInvalidatedEventArgs e)
        {
            if (e.NamesWithoutLocale.Any(x => x.IsEquivalentTo(ModManifest.UniqueID + "/Buildings")) && Context.IsWorldReady)
                ReloadBuildings();
        }

        private void onAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(ModManifest.UniqueID + "/Buildings"))
                e.LoadFrom(() => new Dictionary<string, List<MapBuilding>>(), AssetLoadPriority.Exclusive);
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

                        if (!location.IsBuildableLocation())
                            throw new Exception($"Location {building.Location} does not accept buildings to be placed");

                        if (location.buildings.FirstOrDefault(x => x.tileX.Value == building.X && x.tileY.Value == building.Y && x.modData.ContainsKey(ModManifest.UniqueID + "_PlacedBy")) is Building b)
                        {
                            string placer = b.modData[ModManifest.UniqueID + "_PlacedBy"];
                            if (placer != item.Key)
                                throw new Exception($"Tried to place building {building.Building} in {building.Location} at tile {{ X:{building.X}, Y:{building.Y} }}, but {placer} already has a building placed here.");
                            else
                                continue;
                        }

                        if (!buildingData.TryGetValue(building.Building, out var blueprint))
                            throw new KeyNotFoundException($"Requested building {building.Building} was not found in existing building data.");

                        var buildingRect = new Rectangle(new Point(building.X, building.Y), blueprint.Size);
                        if (IsAreaOccupied(location, new(building.X, building.Y), blueprint.Size, out var removeableObjects))
                            throw new Exception($"The building ({building.Building}) to be placed in {building.Location} at tile {{ X:{building.X}, Y:{building.Y} }} with a tile size of {{ W:{blueprint.Size.X}, H:{blueprint.Size.Y} }} could not be placed since it's area overlaps with non-removable terrain or objects.");

                        var structure = new Building(building.Building, new(building.X, building.Y));
                        structure.FinishConstruction(true);
                        structure.LoadFromBuildingData(blueprint, false, true);
                        structure.modData[ModManifest.UniqueID + "_PlacedBy"] = item.Key;

                        foreach (var obj in removeableObjects)
                            location.Objects.Remove(obj);

                        if (structure.GetIndoors() is AnimalHouse animalHouse && blueprint.MaxOccupants > 0)
                        {
                            foreach (var animal in building.Animals)
                            {
                                if (animalHouse.isFull())
                                {
                                    Monitor.Log($"Tried to add animal {animal} to building {building.Building} in {building.Location} at tile {{ X:{building.X}, Y:{building.Y} }} but it was full, no more animals will be added to this building", LogLevel.Warn);
                                    break;
                                }
                                var farmAnimal = new FarmAnimal(animal, Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
                                farmAnimal.Name = Game1.random.NextBool(0.05) ? "Max" : Dialogue.randomName(); // :SDVpuffersquee:
                                if (!blueprint.ValidOccupantTypes.Contains(farmAnimal.buildingTypeILiveIn.Value))
                                {
                                    Monitor.Log($"Tried to add animal {animal} to building {building.Building} in {building.Location} at tile {{ X:{building.X}, Y:{building.Y} }} but it did not fit to this building type, {getBuildingOccupantRejection(farmAnimal, blueprint)}. This animal was not added.", LogLevel.Warn);
                                    continue;
                                }
                                animalHouse.adoptAnimal(farmAnimal);
                            }
                        }

                        location.buildings.Add(structure);

                        if (!location.buildStructure(structure, new(building.X, building.Y), null, true))
                            throw new Exception($"Tried to place {building.Building} in {building.Location} at {{ X:{building.X}, Y:{building.Y} }} but the game rejected it.");

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

                Monitor.Log($"Finished loading buildings for {item.Key}. {loaded} buildings were loaded.");
            }
        }

        public static bool IsAreaOccupied(GameLocation location, Point start, Point size, out List<Vector2> removeableObjects)
        {
            removeableObjects = [];
            for (int x = start.X; x < start.X + size.X; x++)
            {
                for (int y = start.Y; y < start.Y + size.Y; y++)
                {
                    if (IsTileOccupied(location, new(x, y), out var removeable))
                        return true;
                    if (removeable)
                        removeableObjects.Add(new(x, y));
                }
            }
            return false;
        }

        public static bool IsTileOccupied(GameLocation location, Vector2 tile, out bool canBeRemoved)
        {
            canBeRemoved = false;
            if (location.IsTileOccupiedBy(tile, CollisionMask.Buildings | CollisionMask.Furniture | CollisionMask.LocationSpecific) ||
                !location.isTilePlaceable(tile))
                return true;
            if (location.Objects.TryGetValue(tile, out var obj) && !categoryIgnoreKeys.Contains(obj.Category))
                return true;
            if (location.Objects.ContainsKey(tile))
                canBeRemoved = true;
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
    }
}
