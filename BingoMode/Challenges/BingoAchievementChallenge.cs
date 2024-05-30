using Expedition;
using MoreSlugcats;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    // Taken from vanilla and modified
    public class BingoAchievementChallenge : BingoChallenge
    {
        public SettingBox<string> ID; //WinState.EndgameID

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Earn <achievement_name> passage").Replace("<achievement_name>", ChallengeTools.IGT.Translate(WinState.PassageDisplayName(new(ID.Value))));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Icon("smallEmptyCircle", 1f, Color.white), new Icon(ID.Value + "A", 1f, Color.white), new Icon("smallEmptyCircle", 1f, Color.white)], []);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Passage");
        }

        public override bool Duplicable(Challenge challenge)
        {
            return !(challenge is BingoAchievementChallenge) || !((challenge as BingoAchievementChallenge).ID.Value == ID.Value);
        }

        public override int Points()
        {
            int num = 0;
            try
            {
                num = ChallengeTools.achievementScores[new(ID.Value)];
                if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear && (ID.Value == "Saint" || ID.Value == "Monk"))
                {
                    num += 40;
                }
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
            return true;
        }

        public override Challenge Generate()
        {
            List<WinState.EndgameID> list = new List<WinState.EndgameID>();
            for (int i = 0; i < ChallengeTools.achievementScores.Count; i++)
            {
                if (!ModManager.MSC || (!(ChallengeTools.achievementScores.ElementAt(i).Key == MoreSlugcatsEnums.EndgameID.Mother) && !(ChallengeTools.achievementScores.ElementAt(i).Key == MoreSlugcatsEnums.EndgameID.Gourmand)))
                {
                    list.Add(ChallengeTools.achievementScores.ElementAt(i).Key);
                }
            }
            WinState.EndgameID id = list[Random.Range(0, list.Count)];
            return new BingoAchievementChallenge
            {
                ID = new(id.value, "Passage", 0, listName: "passage")
            };
        }

        public override bool CombatRequired()
        {
            return ID.Value == "Chieftain" || ID.Value == "DragonSlayer" || ID.Value == "Hunter" || ID.Value == "Outlaw" || ID.Value == "Gourmand";
        }

        public void CheckAchievementProgress(WinState winState)
        {
            if (this.completed || revealed || this.game == null)
            {
                return;
            }
            if (winState != null)
            {
                WinState.EndgameTracker tracker = winState.GetTracker(new(ID.Value), true);
                if (tracker != null && tracker.GoalFullfilled)
                {
                    this.CompleteChallenge();
                }
            }
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoAchievementChallenge",
                "~",
                ID.ToString(),
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
                ID = SettingBoxFromString(array[0]) as SettingBox<string>;
                completed = (array[1] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                TeamsFromString(array[4]);
                UpdateDescription();
            }
            catch (System.Exception ex)
            {
                ExpLog.Log("ERROR: BingoAchievementChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            IL.WinState.CycleCompleted += WinState_CycleCompleted;
        }

        public override void RemoveHooks()
        {

            IL.WinState.CycleCompleted -= WinState_CycleCompleted;
        }

        public override List<object> Settings() => [ID];

    }
}
