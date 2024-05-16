using Expedition;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ItemType = AbstractPhysicalObject.AbstractObjectType;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoCraftChallenge : BingoChallenge
    {
        public SettingBox<string> craftee;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Craft a <item>")
                .Replace("<item>", ChallengeTools.ItemName(new(craftee.Value)).TrimEnd('s'));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoCraftChallenge c;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Item Crafting");
        }

        public override Challenge Generate()
        {
            return new BingoCraftChallenge
            {
                craftee = new(ChallengeUtils.CraftableItems[UnityEngine.Random.Range(0, ChallengeUtils.CraftableItems.Length)], "Item to Craft", 0, listName: "craft")
            };
        }

        public void Crafted(ItemType item)
        {
            if (!completed && item.value == craftee.Value)
            {
                CompleteChallenge();
            }
        }

        public override int Points()
        {
            return 20;
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoCraftChallenge",
                "~",
                craftee.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0"
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                craftee = SettingBoxFromString(array[0]) as SettingBox<string>;
                completed = (array[1] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoCraftChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.Player.CraftingResults += Player_CraftingResults;
        }

        public override void RemoveHooks()
        {
            On.Player.CraftingResults -= Player_CraftingResults;
        }

        public override List<object> Settings() => [craftee];
    }
}