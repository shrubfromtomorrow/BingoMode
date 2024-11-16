using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoTameChallenge : BingoChallenge
    {
        public SettingBox<string> crit;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Befriend a <crit>")
                .Replace("<crit>", ChallengeTools.creatureNames[new CreatureTemplate.Type(crit.Value).Index].TrimEnd('s'));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase() => new Phrase([new Icon("FriendB", 1f, Color.white), new Icon(ChallengeUtils.ItemOrCreatureIconName(crit.Value), 1f, ChallengeUtils.ItemOrCreatureIconColor(crit.Value))], []);

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoTameChallenge c || c.crit.Value != crit.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Befriending a creature");
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
            if (!completed && !revealed && !hidden && !TeamsCompleted[SteamTest.team] && friend.value == crit.Value)
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
                revealed ? "1" : "0",
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                crit = SettingBoxFromString(array[0]) as SettingBox<string>;
                completed = (array[1] == "1");
                revealed = (array[2] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoTameChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.FriendTracker.Update += FriendTracker_Update;
        }

        public override void RemoveHooks()
        {
            On.FriendTracker.Update -= FriendTracker_Update;
        }

        public override List<object> Settings() => [crit];
    }
}