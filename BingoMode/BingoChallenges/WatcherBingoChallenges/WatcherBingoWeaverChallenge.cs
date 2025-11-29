using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Watcher;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class WatcherBingoWeaverChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> amount;
        public WatcherBingoWeaverChallenge()
        {
        }

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Visit The Weaver");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new Phrase([[new Icon("weaver"), new Verse("WTDA")]]);
            return phrase;
        }

        public void Meet()
        {
            current++;
            UpdateDescription();
            if (current >= amount.Value) CompleteChallenge();
            else ChangeValue();
        }

        public override int Points()
        {
            return 20;
        }

        public override Challenge Generate()
        {
            return new WatcherBingoWeaverChallenge
            {
                amount = new(Random.Range(2, 7), "Amount", 2),
            };
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not WatcherBingoWeaverChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Visiting The Weaver");
        }

        public override void Reset()
        {
            base.Reset();
            current = 0;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat == WatcherEnums.SlugcatStatsName.Watcher;
        }

        public override string ToString()
        {
            return string.Concat(
            [
                "WatcherBingoWeaverChallenge",
                "~",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
            ]);
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                current = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[1]) as SettingBox<int>;
                completed = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (System.Exception ex)
            {
                ExpLog.Log("ERROR: WatcherBingoWeaverChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.Watcher.VoidWeaver.DefaultBehavior.StartMonologue += Watcher_VoidWeaver_DefaultBehavior_StartMonologue;
        }

        public override void RemoveHooks()
        {
            On.Watcher.VoidWeaver.DefaultBehavior.StartMonologue -= Watcher_VoidWeaver_DefaultBehavior_StartMonologue;
        }

        public override List<object> Settings() => [amount];
    }
}
