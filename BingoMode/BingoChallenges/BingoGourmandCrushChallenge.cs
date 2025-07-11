using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoGourmandCrushChallenge : BingoChallenge
    {
        public List<string> crushedTypes = [];
        public int current;
        public SettingBox<int> amount;

        public override Phrase ConstructPhrase()
        {
            return new Phrase(
                [[new Icon("gourmcrush", 1f, Color.white)],
                [new Counter(current, amount.Value)]]);
        }

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Crush [<current>/<amount>] unique creatures by falling")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amount.Value.ToString());
            base.UpdateDescription();
        }

        public override Challenge Generate()
        {
            return new BingoGourmandCrushChallenge
            {
                amount = new(UnityEngine.Random.Range(2, 10), "Amount", 0)
            };
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Crushing creatures");
        }

        public override int Points()
        {
            return 20;
        }

        public override void Reset()
        {
            current = 0;
            crushedTypes?.Clear();
            crushedTypes = [];
            base.Reset();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoGourmandCrushChallenge c;
        }

        public void Crush(string type)
        {
            if (completed || revealed || hidden || TeamsCompleted[SteamTest.team] || crushedTypes.Contains(type)) return;
            crushedTypes.Add(type);
            current++;
            UpdateDescription();
            if (current >= (int)amount.Value) CompleteChallenge();
            else ChangeValue();
        }


        public override bool CombatRequired()
        {
            return true;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand;
        }
        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoGourmandCrushChallenge",
                "~",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                string.Join("|", crushedTypes),
            });
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
                crushedTypes = [];
                crushedTypes = [.. array[4].Split('|')];
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoGourmandCrushChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            IL.Player.Collide += Player_SlugslamIL;
        }

        public override void RemoveHooks()
        {
            IL.Player.Collide -= Player_SlugslamIL;
        }

        public override List<object> Settings() => [amount];
    }
}
