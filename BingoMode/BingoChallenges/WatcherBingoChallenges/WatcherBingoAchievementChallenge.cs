using BingoMode.BingoRandomizer;
using Expedition;
using MoreSlugcats;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Watcher;

namespace BingoMode.BingoChallenges
{
    using BingoSteamworks;
    using System.Text;
    using static ChallengeHooks;

    // Taken from nacu who took it from vanilla and modified
    public class WatcherBingoAchievementChallenge : BingoChallenge
    {
        public SettingBox<string> ID; //WinState.EndgameID

        public WatcherBingoAchievementChallenge()
        {
            ID = new("", "Passage", 0, "Wpassage");
        }

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Earn <achievement_name> passage").Replace("<achievement_name>", ChallengeTools.IGT.Translate(WinState.PassageDisplayName(new(ID.Value))));
            base.UpdateDescription();
        }

        public override bool RequireSave() => false;

        public override Phrase ConstructPhrase()
        {
            return new Phrase([[new Icon("smallEmptyCircle"), new Icon(ID.Value + "A"), new Icon("smallEmptyCircle")]]);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Obtaining passages");
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not WatcherBingoAchievementChallenge c || c.ID.Value != ID.Value;
        }

        public override int Points()
        {
            int num = 0;
            try
            {
                num = ChallengeTools.achievementScores[new(ID.Value)];
                num *= (int)(this.hidden ? 2f : 1f);
            }
            catch
            {
                ExpLog.Log("Could not get achievement score for ID: " + ID.Value);
                num = 0;
            }
            return num;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat == WatcherEnums.SlugcatStatsName.Watcher;
        }

        public override Challenge Generate()
        {
            string id = ChallengeUtils.GetCorrectListForChallenge("Wpassage")[Random.Range(0, ChallengeUtils.GetCorrectListForChallenge("Wpassage").Length)];
            return new WatcherBingoAchievementChallenge
            {
                ID = new(id, "Passage", 0, listName: "Wpassage")
            };
        }

        public override bool CombatRequired()
        {
            return ID.Value == "Chieftain" || ID.Value == "DragonSlayer" || ID.Value == "Hunter" || ID.Value == "Outlaw" || ID.Value == "Gourmand";
        }

        public void GetAchievement()
        {
            if (completed || TeamsCompleted[SteamTest.team] || revealed || hidden) return;
            CompleteChallenge();
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "WatcherBingoAchievementChallenge",
                "~",
                ID.ToString(),
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
                ID = SettingBoxFromString(array[0]) as SettingBox<string>;
                completed = (array[1] == "1");
                revealed = (array[2] == "1");
                UpdateDescription();
            }
            catch (System.Exception ex)
            {
                ExpLog.Log("ERROR: WatcherBingoAchievementChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.Menu.KarmaLadder.AddEndgameMeters += Watcher_KarmaLadder_AddEndgameMeters;
        }

        public override void RemoveHooks()
        {
            On.Menu.KarmaLadder.AddEndgameMeters -= Watcher_KarmaLadder_AddEndgameMeters;
        }

        public override List<object> Settings() => [ID];

    }
}
