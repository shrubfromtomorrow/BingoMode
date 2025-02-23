using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoPopcornChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> amound;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Open [<current>/<amount>] popcorn plants")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amound.Value.ToString());
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase() => new Phrase([new Icon("Symbol_Spear", 1f, UnityEngine.Color.white), new Icon("popcorn_plant", 1f, new UnityEngine.Color(0.41f, 0.16f, 0.23f)), new Counter(current, amound.Value)], [2]);

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoPopcornChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Popping popcorn plants");
        }

        public override Challenge Generate()
        {
            BingoPopcornChallenge ch = new();
            ch.amound = new(UnityEngine.Random.Range(2, 8), "Amount", 0);
            return ch;
        }

        public void Pop()
        {
            if (!completed && !revealed && !hidden && !TeamsCompleted[SteamTest.team])
            {
                current++;
                UpdateDescription();
                if (current >= (int)amound.Value) CompleteChallenge();
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

        public override void Reset()
        {
            base.Reset();
            current = 0;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat != MoreSlugcatsEnums.SlugcatStatsName.Saint;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoPopcornChallenge",
                "~",
                current.ToString(),
                "><",
                amound.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                current = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                amound = SettingBoxFromString(array[1]) as SettingBox<int>;
                completed = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoPopcornChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            IL.SeedCob.HitByWeapon += SeedCob_HitByWeapon;
        }

        public override void RemoveHooks()
        {
            IL.SeedCob.HitByWeapon -= SeedCob_HitByWeapon;
        }

        public override List<object> Settings() => [amound];
    }
}