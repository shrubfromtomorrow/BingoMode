using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using ItemType = AbstractPhysicalObject.AbstractObjectType;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoCraftChallenge : Challenge, IBingoChallenge
    {
        public ItemType craftee;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Craft a <item>")
                .Replace("<item>", ChallengeTools.ItemName(craftee).TrimEnd('s'));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoCraftChallenge c;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Crafting");
        }

        public override Challenge Generate()
        {
            return new BingoCraftChallenge
            {
                craftee = ChallengeUtils.CraftableItems[UnityEngine.Random.Range(0, ChallengeUtils.CraftableItems.Length)]
            };
        }

        public void Crafted(ItemType item)
        {
            if (!completed && item == craftee)
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
                "Crafting",
                "~",
                ValueConverter.ConvertToString(craftee),
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
                craftee = new ItemType(array[0], false);
                completed = (array[1] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: Crafting FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            On.Player.CraftingResults += Player_CraftingResults;
        }

        public void RemoveHooks()
        {
            On.Player.CraftingResults -= Player_CraftingResults;
        }
    }
}