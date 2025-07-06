using BingoMode.BingoSteamworks;
using Expedition;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using MoreSlugcats;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoIteratorChallenge : BingoChallenge
    {
        public SettingBox<bool> moon;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Visit <iterator>")
                .Replace("<iterator>", moon.Value ? ChallengeTools.IGT.Translate("Looks To The Moon") : ChallengeTools.IGT.Translate("Five Pebbles"));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([
                new Icon("singlearrow", 1f, Color.white),
                new Icon(moon.Value ? "GuidanceMoon" : "nomscpebble", 1f, moon.Value ? new Color(1f, 0.8f, 0.3f) : new Color(0.44705883f, 0.9019608f, 0.76862746f))
                ], []);
        }

        public void MeetPebbles()
        {
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden || moon.Value) return;
            UpdateDescription();
            CompleteChallenge();
        }

        public void MeetMoon()
        {
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden || !moon.Value) return;
            UpdateDescription();
            CompleteChallenge();
        }

        public override int Points()
        {
            return 20;
        }

        public override Challenge Generate()
        {
            // Exclude moon for arti
            bool flag = ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer;
            return new BingoIteratorChallenge
            {
                moon = new(flag ? false : Random.value < 0.5f, "Looks to the Moon", 0)
            };
        }

        public override bool RequireSave() => false;

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoIteratorChallenge c || c.moon.Value != moon.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Visiting iterators");
        }

        public override string ToString()
        {
            return string.Concat(
            [
                "BingoIteratorChallenge",
                "~",
                moon.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
            ]);
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                moon = SettingBoxFromString(array[0]) as SettingBox<bool>;
                completed = (array[1] == "1");
                revealed = (array[2] == "1");
                UpdateDescription();
            }
            catch (System.Exception ex)
            {
                ExpLog.Log("ERROR: BingoIteratorChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.SSOracleBehavior.SeePlayer += SSOracleBehavior_SeePlayer;
            On.SLOracleBehaviorHasMark.InitateConversation += SLOracleBehaviorHasMark_InitateConversation;
            On.MoreSlugcats.CLOracleBehavior.Update += CLOracleBehavior_Update;

        }

        public override void RemoveHooks()
        {
            On.SSOracleBehavior.SeePlayer -= SSOracleBehavior_SeePlayer;
            On.SLOracleBehaviorHasMark.InitateConversation -= SLOracleBehaviorHasMark_InitateConversation;
            On.MoreSlugcats.CLOracleBehavior.Update -= CLOracleBehavior_Update;
        }

        public override List<object> Settings() => [moon];
    }
}
