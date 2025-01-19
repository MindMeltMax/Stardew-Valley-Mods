using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.FarmAnimals;

namespace BetterPigs
{
    public class API
    {
        public bool CanGoOutside(FarmAnimal animal)
        {
            var home = animal.home;
            var location = animal.home.GetParentLocation();

            return Patches.canGoOutside(animal, home, location);
        }

        public bool CanDigUpProduce(FarmAnimal animal)
        {
            var location = animal.currentLocation;
            if (!location.IsOutdoors)
                return false;
            if (!Patches.CanAnimalGoOutsideInThisWeather(animal))
                return false;

            return animal.currentProduce.Value is not null && animal.isAdult() && animal.GetHarvestType().GetValueOrDefault() == FarmAnimalHarvestType.DigUp && Game1.random.NextDouble() < 0.0002;
        }
    }
}
