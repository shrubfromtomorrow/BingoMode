using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using ItemType = AbstractPhysicalObject.AbstractObjectType;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoCraftChallenge : BingoChallenge
    {
        public SettingBox<string> craftee;
        public SettingBox<int> amount;
        public int current;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate($"Craft [{current}/{amount.Value}] <item>")
                .Replace("<item>", ChallengeTools.ItemName(new(craftee.Value)));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Icon("crafticon", 1f, Color.white), new Icon(ChallengeUtils.ItemOrCreatureIconName(craftee.Value), 1f, ChallengeUtils.ItemOrCreatureIconColor(craftee.Value)), new Counter(current, amount.Value)], [2]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoCraftChallenge c || c.craftee.Value != craftee.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Crafting items");
        }

        public override Challenge Generate()
        {
            int thingies = UnityEngine.Random.Range(2, 6);
            return new BingoCraftChallenge
            {
                craftee = new(ChallengeUtils.CraftableItems[UnityEngine.Random.Range(0, ChallengeUtils.CraftableItems.Length)], "Item to Craft", 0, listName: "craft"),
                amount = new(thingies, "Amount", 1)
            };
        }

        public void Crafted(ItemType item)
        {
            if (!completed && !TeamsCompleted[SteamTest.team] && !hidden && !revealed && item.value == craftee.Value)
            {
                current += 1;
                UpdateDescription();
                if (current >= amount.Value)
                {
                    CompleteChallenge();
                }
                else ChangeValue();
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
                amount.ToString(),
                "><",
                current.ToString(),
                "><",
                completed ? "1" : "0",
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
                amount = SettingBoxFromString(array[1]) as SettingBox<int>;
                current = int.Parse(array[2], System.Globalization.NumberStyles.Any);
                completed = (array[3] == "1");
                revealed = (array[4] == "1");
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
            IL.Player.SpitUpCraftedObject += Player_SpitUpCraftedObjectIL;
        }

        public override void RemoveHooks()
        {
            IL.Player.SpitUpCraftedObject -= Player_SpitUpCraftedObjectIL;
        }

        public override List<object> Settings() => [craftee, amount];
    }
}