using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoTameChallenge : Challenge, IBingoChallenge
    {
        public SettingBox<string> crit;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Befriend a <crit>")
                .Replace("<crit>", ChallengeTools.creatureNames[new CreatureTemplate.Type(crit.Value).Index].TrimEnd('s'));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoTameChallenge s || s.crit != crit;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Befriending");
        }

        public override Challenge Generate()
        {
            var crug = ChallengeUtils.Befriendable[UnityEngine.Random.Range(0, ChallengeUtils.Befriendable.Length - (ModManager.MSC ? (ExpeditionData.slugcatPlayer != MoreSlugcatsEnums.SlugcatStatsName.Saint ? 1 : 0) : 3))];

            return new BingoTameChallenge
            {
                crit = new(crug, "Creature Type", 0, listName: "friend")
            };
        }

        public void Fren(CreatureTemplate.Type friend)
        {
            if (!completed && friend.value == crit.Value)
            {
                CompleteChallenge();
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

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoTameChallenge",
                "~",
                crit.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0"
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                crit = SettingBoxFromString(array[0]) as SettingBox<string>;
                completed = (array[1] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoTameChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            On.FriendTracker.Update += FriendTracker_Update;
        }

        public void RemoveHooks()
        {
            On.FriendTracker.Update -= FriendTracker_Update;
        }

        public List<object> Settings() => [crit];
    }
}