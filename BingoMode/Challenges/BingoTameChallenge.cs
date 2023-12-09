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
    public class BingoTameChallenge : Challenge
    {
        public CreatureTemplate.Type crit;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Befriend a <crit>")
                .Replace("<crit>", ChallengeTools.creatureNames[(int)crit].TrimEnd('s'));
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
                crit = crug
            };
        }

        public void Fren(CreatureTemplate.Type friend)
        {
            if (!completed && friend == crit)
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
                "Befriending",
                "~",
                ValueConverter.ConvertToString(crit),
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
                crit = new(array[0], false);
                completed = (array[1] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: Befriending FromString() encountered an error: " + ex.Message);
            }
        }
    }
}