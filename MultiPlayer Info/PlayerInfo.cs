using StardewValley;
using System.Text;

namespace MPInfo
{
    public class PlayerInfo
    {
        public int Health { get; set; }

        public int MaxHealth { get; set; }

        public float Stamina { get; set; }

        public int MaxStamina { get; set; }

        public PlayerInfo() { }

        public PlayerInfo(Farmer? player)
        {
            Health = player?.health ?? 100;
            MaxHealth = player?.maxHealth ?? 100;
            Stamina = player?.Stamina ?? 270;
            MaxStamina = player?.MaxStamina ?? 270;
        }

        //Using this instead of $"{Health}|{MaxHealth}|{Stamina}|{MaxStamina}"; takes 18 less il instructions... WTF c#?
        public string Serialize() => new StringBuilder().Append(Health).Append('|').Append(MaxHealth).Append('|').Append(Stamina).Append('|').Append(MaxStamina).ToString();

        public static PlayerInfo Deserialize(string value)
        {
            var items = value.Split('|');
            return new()
            {
                Health = int.Parse(items[0]),
                MaxHealth = int.Parse(items[1]),
                Stamina = float.Parse(items[2]),
                MaxStamina = int.Parse(items[3])
            };
        }

        public bool IsMatch(Farmer player) => player.health == Health && player.maxHealth == MaxHealth && player.Stamina == Stamina && player.MaxStamina == MaxStamina;
    }
}
