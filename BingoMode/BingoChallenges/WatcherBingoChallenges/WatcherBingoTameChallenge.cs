using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using Watcher;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class WatcherBingoTameChallenge : BingoChallenge
    {
        public SettingBox<string> crit;
        public List<string> tamed = [];
        public int current;
        public SettingBox<int> amount;
        public SettingBox<bool> specific;

        public WatcherBingoTameChallenge()
        {
            specific = new(false, "Specific Creature Type", 0);
            crit = new("", "Creature Type", 1, listName: "Wfriend");
            amount = new(0, "Amount", 2);
            tamed = [];
        }

        public override void UpdateDescription()
        {
            this.description = specific.Value ? ChallengeTools.IGT.Translate("Befriend a <crit>")
                .Replace("<crit>", ChallengeTools.creatureNames[new CreatureTemplate.Type(crit.Value).Index].TrimEnd('s'))
                : ChallengeTools.IGT.Translate("Befriend [<current>/<amount>] unique creature types")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", specific.Value ? "1" : ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new([[new Icon("FriendB")]]);
            if (specific.Value) phrase.InsertWord(Icon.FromEntityName(crit.Value));
            else phrase.InsertWord(new Counter(current, amount.Value), 1);
            return phrase;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not WatcherBingoTameChallenge c || specific.Value != c.specific.Value || (specific.Value && c.specific.Value && crit.Value != c.crit.Value);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Befriending creatures");
        }

        public override Challenge Generate()
        {
            bool specific = UnityEngine.Random.value < 0.5f;
            var crug = ChallengeUtils.WBefriendable[UnityEngine.Random.Range(0, ChallengeUtils.WBefriendable.Length)];

            return new WatcherBingoTameChallenge
            {
                specific = new SettingBox<bool>(specific, "Specific Creature Type", 0),
                crit = new(crug, "Creature Type", 1, listName: "Wfriend"),
                amount = new(UnityEngine.Random.Range(2, 7), "Amount", 2),
                tamed = []
            };
        }

        public void Fren(CreatureTemplate.Type friend)
        {
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden) return;
            if (specific.Value)
            {
                if (friend.value != crit.Value) return;
                current = 1;
                UpdateDescription();
                CompleteChallenge();
            }
            else
            {
                if (tamed.Contains(friend.value)) return;
                current++;
                tamed.Add(friend.value);
                UpdateDescription();
                if (current >= amount.Value) CompleteChallenge();
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
            return slugcat == WatcherEnums.SlugcatStatsName.Watcher;
        }

        public override void Reset()
        {
            base.Reset();
            current = 0;
            tamed?.Clear();
            tamed = [];
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "WatcherBingoTameChallenge",
                "~",
                specific.ToString(),
                "><",
                crit.ToString(),
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                string.Join("cLtD", tamed),
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                specific = SettingBoxFromString(array[0]) as SettingBox<bool>;
                crit = SettingBoxFromString(array[1]) as SettingBox<string>;
                current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[3]) as SettingBox<int>;
                completed = (array[4] == "1");
                revealed = (array[5] == "1");
                string[] arr = Regex.Split(array[6], @"cLtD");
                tamed = [.. arr];
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: WatcherBingoTameChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.FriendTracker.Update += Watcher_FriendTracker_Update;
        }

        public override void RemoveHooks()
        {
            On.FriendTracker.Update -= Watcher_FriendTracker_Update;
        }

        public override List<object> Settings() => [crit, amount, specific];
    }
}