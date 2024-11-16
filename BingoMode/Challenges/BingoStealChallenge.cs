using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoStealChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> amount;
        public SettingBox<bool> toll;
        public SettingBox<string> subject;
        public List<EntityID> checkedIDs;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Steal [<current>/<amount>] <item> from " + (toll.Value ? "a Scavenger toll" : "Scavengers"))
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value))
                .Replace("<item>", ChallengeTools.ItemName(new(subject.Value)));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoStealChallenge c || c.subject.Value != subject.Value || c.toll.Value != toll.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Stealing items");
        }

        public override Challenge Generate()
        {
            bool taxEvasion = UnityEngine.Random.value < 0.5f;
            string itme = "Spear";
            if (taxEvasion)
            {
                itme = UnityEngine.Random.value < 0.5f ? "Spear": "DataPearl";
            }
            else itme = ChallengeUtils.StealableStolable[UnityEngine.Random.Range(0, ChallengeUtils.StealableStolable.Length - (ModManager.MSC ? 0 : 2))];

            return new BingoStealChallenge
            {
                checkedIDs = [],
                toll = new(taxEvasion, "From Scavenger Toll", 0),
                subject = new(itme, "Item", 1, listName: "theft"),
                amount = new(UnityEngine.Random.Range(1, itme == "ScavengerBomb" ? 3 : 5), "Amount", 2)
            };
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([
                new Icon("steal_item", 1f, Color.white),
                new Icon(ChallengeUtils.ItemOrCreatureIconName(subject.Value), 1f, ChallengeUtils.ItemOrCreatureIconColor(subject.Value)),
                new Icon(toll.Value ? "scavtoll" : "Kill_Scavenger", toll.Value ? 0.8f : 1f, toll.Value ? Color.white : ChallengeUtils.ItemOrCreatureIconColor("Scavenger")),
                new Counter(current, amount.Value)], [3]);
        }

        public override int Points()
        {
            return amount.Value * 10;
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            current = 0;
            checkedIDs = [];
        }

        public void Stoled(AbstractPhysicalObject item, bool tollCheck)
        {
            if (!completed && !revealed && !TeamsCompleted[SteamTest.team] && !hidden && item.type.value == subject.Value && tollCheck == toll.Value && !checkedIDs.Contains(item.ID))
            {
                current++;
                UpdateDescription();
                if (current >= amount.Value)
                {
                    CompleteChallenge();
                }
                else ChangeValue();
                checkedIDs.Add(item.ID);
            }
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoStealChallenge",
                "~",
                subject.ToString(),
                "><",
                toll.ToString(),
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
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
                subject = SettingBoxFromString(array[0]) as SettingBox<string>;
                toll = SettingBoxFromString(array[1]) as SettingBox<bool>;
                current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[3]) as SettingBox<int>;
                completed = (array[4] == "1");
                revealed = (array[5] == "1");
                checkedIDs = [];
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoStealChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.ScavengerOutpost.PlayerTracker.Update += PlayerTracker_Update;
            On.SocialEventRecognizer.Theft += SocialEventRecognizer_Theft;
            On.Player.SlugcatGrab += Player_SlugcatGrabNoStealExploit;
        }

        public override void RemoveHooks()
        {
            On.ScavengerOutpost.PlayerTracker.Update -= PlayerTracker_Update;
            On.SocialEventRecognizer.Theft -= SocialEventRecognizer_Theft;
            On.Player.SlugcatGrab -= Player_SlugcatGrabNoStealExploit;
        }

        public override List<object> Settings() => [amount, toll, subject];
    }
}