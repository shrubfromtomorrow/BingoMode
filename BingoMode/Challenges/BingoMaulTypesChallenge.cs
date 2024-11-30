using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoMaulTypesChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> amound;
        public List<string> doneTypes = [];

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Maul [<current>/<amount>] different types of creatures")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amound.Value.ToString());
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase() => new Phrase([new Icon("artimaulcrit", 1f, UnityEngine.Color.white), new Counter(current, amound.Value)], [1]);

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoMaulTypesChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Mauling different types of creatures");
        }

        public override Challenge Generate()
        {
            BingoMaulTypesChallenge ch = new();
            ch.amound = new(UnityEngine.Random.Range(4, 10), "Amount", 0);
            return ch;
        }

        public void Maul(string type)
        {
            Plugin.logger.LogMessage($"Mauling {type}");
            if (completed || revealed || hidden || TeamsCompleted[SteamTest.team] || doneTypes.Contains(type)) return;
            doneTypes.Add(type);
            current++;
            UpdateDescription();
            if (current >= (int)amound.Value) CompleteChallenge();
            else ChangeValue();
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
            doneTypes?.Clear();
            doneTypes = [];
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
                "BingoMaulTypesChallenge",
                "~",
                current.ToString(),
                "><",
                amound.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                string.Join("|", doneTypes),
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
                doneTypes = [];
                doneTypes = [.. array[4].Split('|')];
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoMaulTypesChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            IL.Player.GrabUpdate += Player_GrabUpdateArtiMaulTypes;
        }

        public override void RemoveHooks()
        {
            IL.Player.GrabUpdate -= Player_GrabUpdateArtiMaulTypes;
        }

        public override List<object> Settings() => [amound];
    }
}
