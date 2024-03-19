using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace PiggyBank.Data
{
    [Obsolete("Switching to modData format with PiggyBankItem")]
    public class PiggyBankGold
    {
        public string Label { get; set; }
        public int Id { get; set; }
        public float StoredGold { get; set; } = 0;
        public Vector2 BankTile { get; set; }
        public string BankLocationName { get; set; }
        public long OwnerID { get; set; }

        public PiggyBankGold()
        {
        }

        public PiggyBankGold(string label, float gold, Vector2 bankTile, int id, long playerId, string locName)
        {
            Id = id;
            Label = label;
            StoredGold = gold;
            BankTile = bankTile;
            OwnerID = playerId;
            BankLocationName = locName;
        }
    }
}
