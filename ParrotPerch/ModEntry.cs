using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.BigCraftables;

namespace ParrotPerch
{
    public class ModEntry : Mod
    {
        public string Id => ModManifest.UniqueID + ".Perch";

        public Data Data;

        public override void Entry(IModHelper helper)
        {
            Helper.Events.GameLoop.GameLaunched += onGameLaunched;
            Helper.Events.GameLoop.SaveLoaded += onSaveLoaded;
            Helper.Events.Content.AssetRequested += onAssetRequested;
            Helper.Events.Player.Warped += onWarp;

            Data = Helper.ModContent.Load<Data>("Assets/data.json");
            Data.Recipe = string.Format(Data.Recipe, Helper.Translation.Get("Object.Perch.Name"));
            Data.Object.DisplayName = Helper.Translation.Get("Object.Perch.Name");
            Data.Object.Description = Helper.Translation.Get("Object.Perch.Description");
        }

        private void onGameLaunched(object? sender, GameLaunchedEventArgs e) => PerchInventoryHandler.Init(this);

        private void onSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (Game1.player.eventsSeen.Contains("463391") && !Game1.player.knowsRecipe(Id) && !Game1.player.mailbox.Contains(ModManifest.UniqueID + "/Perch_Recipe"))
            {
                if (Game1.player.mailReceived.Contains(ModManifest.UniqueID + "/Perch_Recipe"))
                    Game1.player.craftingRecipes.Add(Id, 0);
                else
                    Game1.player.mailbox.Add(ModManifest.UniqueID + "/Perch_Recipe");
            }    
            else
                Game1.player.eventsSeen.OnValueAdded += onEventsSeenChanged;
        }

        private void onEventsSeenChanged(string value)
        {
            if (value == "463391")
                Game1.player.mailForTomorrow.Add(ModManifest.UniqueID + "/Perch_Recipe");
        }

        private void onAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data\\BigCraftables"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, BigCraftableData>().Data;
                    data[Id] = Data.Object;
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data\\CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    data[Id] = Data.Recipe;
                });
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Data\\Mail"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    data[ModManifest.UniqueID + "/Perch_Recipe"] = Helper.Translation.Get("Mail.Emily.Perch_Recipe") + "%item craftingrecipe " + Id + " %%[#]Emily's New Friend";
                });
            }
        }

        //For backwards compat, doing it on warp so it works for indoor locations
        private void onWarp(object? sender, WarpedEventArgs e)
        {
            if (e.NewLocation.modData.ContainsKey(ModManifest.UniqueID + ".Migrated"))
                return;

            var inventoryId = ModManifest.UniqueID + ".InventoryId";

            foreach (var item in e.NewLocation.Objects.Values)
            {
                if (!(item?.QualifiedItemId.Equals("(BC)" + Id) ?? false) || !item.modData.TryGetValue(inventoryId, out string id))
                    continue;
                var inventory = Game1.player.team.GetOrCreateGlobalInventory(id);
                var egg = inventory.GetById("(TR)ParrotEgg").FirstOrDefault();
                if (PerchInventoryHandler.SetHeldParrot(item, egg, out var previous) && previous is not null)
                    Game1.createItemDebris(previous, item.getLocalPosition(Game1.viewport), 0, item.Location);
                item.modData.Remove(inventoryId);
            }
            e.NewLocation.modData.Add(ModManifest.UniqueID + ".Migrated", "2.0.0"); //Prevent checking the same location on every warp
        }
    }
}
