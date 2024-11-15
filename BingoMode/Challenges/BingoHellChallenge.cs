using BingoMode.BingoSteamworks;
using Expedition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoHellChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> amound;

        public override bool RequireSave() => false;
        public override bool ReverseChallenge() => true;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Do not die before completing [<current>/<amount>] bingo challenges")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amound.Value.ToString());
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase() => new Phrase([
            new Icon("completechallenge", 1f, UnityEngine.Color.white), 
            new Counter(current, amound.Value),
            new Icon("buttonCrossA", 1f, UnityEngine.Color.red),
            new Icon("Multiplayer_Death", 1f, UnityEngine.Color.white)]
        , [2]);

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoHellChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Not dying before completing challenges");
        }

        public override Challenge Generate()
        {
            BingoHellChallenge ch = new();
            ch.amound = new(UnityEngine.Random.Range(2, 6), "Amount", 0);
            return ch;
        }

        public void GetChallenge()
        {
            if (!TeamsFailed[SteamTest.team] && completed && current < amound.Value)
            {
                current++;
                UpdateDescription();
                ChangeValue();
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
            return true;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoHellChallenge",
                "~",
                current.ToString(),
                "><",
                amound.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                TeamsToString()
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
                hidden = (array[3] == "1");
                revealed = (array[4] == "1");
                TeamsFromString(array[5]);
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoHellChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
        }

        public override void RemoveHooks()
        {
        }

        public override List<object> Settings() => [amound];
    }
}
