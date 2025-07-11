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
    public class BingoMaulXChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> amound;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Maul creatures [<current>/<amount>] times")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amound.Value.ToString());
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new(
                [[new Icon("artimaul", 1f, UnityEngine.Color.white)],
                [new Counter(current, amound.Value)]]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoMaulXChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Mauling creatures a certain amount of times");
        }

        public override Challenge Generate()
        {
            BingoMaulXChallenge ch = new();
            ch.amound = new(UnityEngine.Random.Range(7, 29), "Amount", 0);
            return ch;
        }

        public void Maul()
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
            return slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoMaulXChallenge",
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
                ExpLog.Log("ERROR: BingoMaulXChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            IL.Player.GrabUpdate += Player_GrabUpdateArtiMaulX;
        }

        public override void RemoveHooks()
        {
            IL.Player.GrabUpdate -= Player_GrabUpdateArtiMaulX;
        }

        public override List<object> Settings() => [amound];
    }
}
