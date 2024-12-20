using StardewValley;

namespace BetterPigs
{
    public class API
    {
        public bool CanAnimalGoOutside(FarmAnimal animal)
        {
            var home = animal.home;
            var homeLocation = animal.home.GetParentLocation();

            return Patches.canGoOutside(animal, home, homeLocation);
        }
    }
}
