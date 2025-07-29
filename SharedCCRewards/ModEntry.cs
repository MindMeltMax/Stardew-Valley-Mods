using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Menus;
using System.Text;

namespace SharedCCRewards
{
    internal class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            Helper.Events.GameLoop.GameLaunched += onGameLaunch;
            Helper.Events.Content.AssetRequested += onAssetRequested;
        }

        private void onGameLaunch(object? sender, GameLaunchedEventArgs e)
        {
            Harmony harmony = new(ModManifest.UniqueID);

            //Prefix JunimoNoteMenu.openRewardsMenu()
            //Postfix Bundle.completionAnimation(JunimoNoteMenu menu, bool playSound = true, int delay = 0)
            //Or maybe just: Postfix JunimoNoteMenu.rewardGrabbed(Item item, Farmer who)

            //Now just need to decide how to send it to the other players...
            //Probably just going to send it through the mail, too lazy to figure out something else \\I'm sending it through the mail.. still hard for some reason
            harmony.Patch(
                original: AccessTools.Method(typeof(JunimoNoteMenu), "rewardGrabbed", [typeof(Item), typeof(Farmer)]),
                postfix: new(typeof(ModEntry), nameof(JunimoNoteMenu_RewardGrabbed_Postfix))
            );

            //Just figured out I also have to patch the missed rewards chest (or something related..) Fun!
            //But it's almost 1AM rn, so I'm going to bed and vow to look at this another time

            //It's no longer 1AM, I figured out I probably don't have to do anything extra for it... probably
            //Just might want to patch letterviewermenu to add support for junimo text \\Not compatible with MFM, so I'll just leave this idea and maybe do it another time
            
            //It's a few days later and I've decided to leave these comments in for this mod so you can see my struggles
        }

        private void onAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Mail"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    var bundleData = Helper.GameContent.Load<Dictionary<string, string>>("Data/Bundles");
                    foreach (var key in bundleData.Keys)
                    {
                        var keyData = key.Split('/');
                        if (keyData.Length < 2 || !int.TryParse(keyData[1], out int bundleIndex))
                            continue;
                        var bundleItem = bundleData[key].Split('/');
                        if (bundleItem.Length < 2 || string.IsNullOrWhiteSpace(bundleItem[1]))
                            continue;
                        //If there's a better way of handling existing bundle data, I'd love to know, because I cringe every time I see that deprecated warning
                        var item = Utility.getItemFromStandardTextDescription(bundleItem[1], null);
                        if (item is null)
                            continue;
                        StringBuilder builder = new();
                        builder.Append(Helper.Translation.Get($"Missed_Reward_Mail_{Game1.random.Choose(1, 2, 3)}"));
                        builder.Append($"%item id {item.QualifiedItemId} {item.Stack} %%");
                        builder.Append("[#]").Append("Forest friend rewards");
                        data.Add($"CC_Shared_Reward_{bundleIndex}", builder.ToString());
                    }
                });
            }
        }

        private static void JunimoNoteMenu_RewardGrabbed_Postfix(JunimoNoteMenu __instance, Item item, Farmer who)
        {
            var otherPlayers = Game1.getAllFarmers();
            string id = $"CC_Shared_Reward_{item.SpecialVariable}";
            Game1.Multiplayer.broadcastPartyWideMail(id);
            foreach (var player in otherPlayers)
            {
                if (player.UniqueMultiplayerID == who.UniqueMultiplayerID)
                    continue;
                player.mailForTomorrow.Add(id);
            }
            //For some reason broadcastPartyWideMail forcibly adds mail to the main player as well, this is just a backup call in case it's accidentally added
            Game1.player.mailForTomorrow.Remove(id);
        }
    }
}
