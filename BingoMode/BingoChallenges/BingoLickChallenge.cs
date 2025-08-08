using BingoMode.BingoSteamworks;
using Expedition;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoLickChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> amound;
        public List<string> lickers = [];

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Get licked by [<current>/<amount>] unique lizards")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amound.Value.ToString());
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new(
                [[new Icon("lizlick")],
                [new Counter(current, amound.Value)]]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoLickChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Getting licked by lizards");
        }

        public override Challenge Generate()
        {
            BingoLickChallenge ch = new();
            ch.amound = new(UnityEngine.Random.Range(2, 8), "Amount", 0);
            return ch;
        }

        public void Licked(Lizard licker)
        {
            if (!completed && !revealed && !hidden && !TeamsCompleted[SteamTest.team] && !lickers.Contains(licker.abstractCreature.ID.ToString()))
            {
                lickers.Add(licker.abstractCreature.ID.ToString());
                current++;
                UpdateDescription();
                if (current >= amound.Value) CompleteChallenge();
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
            lickers = [];
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoLickChallenge",
                "~",
                current.ToString(),
                "><",
                amound.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                string.Join("|", lickers),
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
                string[] arr = Regex.Split(array[6], "|");
                lickers = [.. arr];
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoLickChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.LizardTongue.Update += LizardTongue_Update;
        }

        public override void RemoveHooks()
        {
            On.LizardTongue.Update -= LizardTongue_Update;
        }

        public override List<object> Settings() => [amound];
    }
}
