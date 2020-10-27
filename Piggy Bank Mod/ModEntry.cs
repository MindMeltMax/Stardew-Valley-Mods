using StardewModdingAPI;
using StardewModdingAPI.Events;

using Piggy_Bank_Mod.Data;

using StardewValley;
using StardewValley.Menus;

using Microsoft.Xna.Framework;

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System;
using System.Linq;

namespace Piggy_Bank_Mod
{
    public class ModEntry : Mod
    {
        private PiggyBankGold gold;
        private List<Response> responses;
        private ITranslationHelper i18n => Helper.Translation;

        public static IJsonAssetsApi JA;
        public static bool hasExtendedReach;

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.Saving += GameLoop_Saving;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            helper.Events.Multiplayer.PeerConnected += Multiplayer_PeerConnected;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e) //Get JsonAssets Api and directory on Game launch
        {
            JA = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            JA.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));

            hasExtendedReach = Helper.ModRegistry.IsLoaded("spacechase0.ExtendedReach");
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            /*if (!Context.IsMultiplayer)
                return;*/
        }

        private void Multiplayer_PeerConnected(object sender, PeerConnectedEventArgs e)
        {
            /*var farmerArray = Game1.getAllFarmers();

            foreach (Farmer who in farmerArray)
            {
                var id = who.UniqueMultiplayerID;
            }

            foreach(IMultiplayerPeer peer in Helper.Multiplayer.GetConnectedPlayers())
            {
                if (peer.HasSmapi)
                {
                    this.Monitor.Log($"Found player {peer.PlayerID} running Stardew Valley {peer.GameVersion} and SMAPI {peer.ApiVersion} on {peer.Platform} with {peer.Mods} mods.");
                    if(peer.GetMod("MindMeltMax.PiggyBank") != null)
                    {

                    }
                }
                else
                    this.Monitor.Log($"Found player {peer.PlayerID} running Stardew Valley without SMAPI.");
            }*/

            /*foreach(IMultiplayerPeer peer in Helper.Multiplayer.GetConnectedPlayers())
            {
                Farmer farmHand = peer as Farmer;
                Farmer farmOwner = Game1.MasterPlayer;

                List<> farmHandCookingRecipes = farmHand.cookingRecipes;

                if (farmHand.cookingRecipes != farmOwner.cookingRecipes)
                    farmOwner.cookingRecipes = farmOwner.cookingRecipes;
            }*/
        }

        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;
            Helper.Data.WriteSaveData("MindMeltMax.PiggyBank", gold);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;

            gold = Helper.Data.ReadSaveData<PiggyBankGold>("MindMeltMax.PiggyBank");
            if(gold == null)
                gold = new PiggyBankGold();

            if (JA != null)
            {
                var piggyBankID = JA.GetBigCraftableId("Piggy Bank");
            }

            responses = new List<Response>();
            responses.Add(new Response("Deposit", i18n.Get("Deposit")));
            responses.Add(new Response("Withdraw", i18n.Get("Withdraw")));
            responses.Add(new Response("Close", i18n.Get("Close")));
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.CanPlayerMove)
                return;

            if (e.Button != SButton.MouseRight)
                return;
            Vector2 tile;
            if (hasExtendedReach)
                tile = e.Cursor.Tile;
            else
                tile = e.Cursor.GrabTile;
            var location = Game1.currentLocation;
            var clickedObject = location.getObjectAtTile((int)tile.X, (int)tile.Y);

            if (clickedObject == null)
                return;

            else if (clickedObject.Name == "Piggy Bank")
            {
                openPiggy();
            }
            else
                return;
        }

        private bool openPiggy()
        {
            string text = i18n.Get("Stored") + gold.StoredGold.ToString() + "g.";
            Game1.currentLocation.createQuestionDialogue(text, responses.ToArray(), piggyBankMenu);
            return true;
        }

        private void piggyBankMenu(Farmer who, string key)
        {
            if (key == "Close")
                return;

            string txt = responses.Find(k => k.responseKey == key).responseText;
            Game1.activeClickableMenu = new NumberSelectionMenu(txt, (nr, cost, farmer) => processRequest(nr, cost, farmer, key), -1, 0, (key != "Withdraw") ? (int)Game1.player.Money : (int)gold.StoredGold);
        }

        private void processRequest(int number, int cost, Farmer who, string key)
        {
            if(key == "Deposit")
            {
                Game1.player.Money -= number;
                gold.StoredGold += number;
            }
            if(key == "Withdraw")
            {
                Game1.player.Money += number;
                Game1.player.totalMoneyEarned -= (uint)number;
                gold.StoredGold -= number;
                
            }
            Game1.activeClickableMenu = null;
        }
    }
    public interface IJsonAssetsApi //Get The JsonAssets Api functions
    {
        int GetBigCraftableId(string name);
        void LoadAssets(string path);
    }
}
