using BingoMode.BingoRandomizer;
using Expedition;
using MoreSlugcats;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using BingoSteamworks;
    using System.Text;
    using static ChallengeHooks;

    public class BingoAchievementRandomizer : Randomizer<Challenge>
    {
        public Randomizer<string> passage;

        public override Challenge Random()
        {
            BingoAchievementChallenge challenge = new();
            challenge.ID.Value = passage.Random();
            return challenge;
        }
        
        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}passage-{passage.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "Achievement").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            MatchCollection matches = Regex.Matches(serialized, SUBRANDOMIZER_PATTERN);
            passage = Randomizer<string>.InitDeserialize(matches[0].ToString());
        }
    }

    // Taken from vanilla and modified
    public class BingoAchievementChallenge : BingoChallenge
    {
        public SettingBox<string> ID; //WinState.EndgameID

        public BingoAchievementChallenge()
        {
            ID = new("", "Passage", 0, "passage");
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
            return challenge is not BingoAchievementChallenge c || c.ID.Value != ID.Value;
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
                if (ModManager.MSC && 
                    (ChallengeTools.achievementScores.ElementAt(i).Key == MoreSlugcatsEnums.EndgameID.Mother ||
                    ChallengeTools.achievementScores.ElementAt(i).Key == MoreSlugcatsEnums.EndgameID.Gourmand ||
                    (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint && 
                    ChallengeTools.achievementScores.ElementAt(i).Key == WinState.EndgameID.Scholar)))
                    continue;

                list.Add(ChallengeTools.achievementScores.ElementAt(i).Key);
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

        public void GetAchievement()
        {
            if (completed || TeamsCompleted[SteamTest.team] || revealed || hidden) return;
            CompleteChallenge();
        }

        //public void CheckAchievementProgress(WinState winState)
        //{
        //    
        //    if (completed || TeamsCompleted[SteamTest.team] || revealed || hidden)
        //    {
        //        return;
        //    }
        //    if (winState != null)
        //    {
        //        WinState.EndgameTracker tracker = winState.GetTracker(new(ID.Value), true);
        //        
        //        if (tracker != null && tracker.GoalFullfilled)
        //        {
        //            
        //            this.CompleteChallenge();
        //        }
        //    }
        //}

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
                ExpLog.Log("ERROR: BingoAchievementChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            //IL.WinState.CycleCompleted += WinState_CycleCompleted;
            On.Menu.KarmaLadder.ctor_Menu_MenuObject_Vector2_HUD_IntVector2_bool += KarmaLadder_ctor;
        }

        public override void RemoveHooks()
        {

            //IL.WinState.CycleCompleted -= WinState_CycleCompleted;
            On.Menu.KarmaLadder.ctor_Menu_MenuObject_Vector2_HUD_IntVector2_bool -= KarmaLadder_ctor;
        }

        public override List<object> Settings() => [ID];

    }
}
