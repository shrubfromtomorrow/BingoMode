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
    public class BingoSaintPopcornChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> amound;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Eat [<current>/<amount>] popcorn plant seeds")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amound.Value.ToString());
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase() => new Phrase([new Icon("foodSymbol", 1f, UnityEngine.Color.white), new Icon("Symbol_Seed", 1f, Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey)), new Counter(current, amound.Value)], [2]);

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoSaintPopcornChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Eating popcorn plant seeds");
        }

        public override Challenge Generate()
        {
            BingoSaintPopcornChallenge ch = new();
            ch.amound = new(UnityEngine.Random.Range(2, 10), "Amount", 0);
            return ch;
        }

        public void Consume()
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
            return slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoSaintPopcornChallenge",
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
                ExpLog.Log("ERROR: BingoSaintPopcornChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.Player.ObjectEaten += Player_ObjectEatenSeed;
        }

        public override void RemoveHooks()
        {
            On.Player.ObjectEaten -= Player_ObjectEatenSeed;
        }

        public override List<object> Settings() => [amound];
    }
}
