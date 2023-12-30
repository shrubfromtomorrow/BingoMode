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
    public class BingoDodgeLeviathanChallenge : Challenge
    {
        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Dodge a leviathan bite");
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoDodgeLeviathanChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Dodge Leviathan Bite");
        }

        public void Dodged()
        {
            if (!completed) CompleteChallenge();
        }

        public override Challenge Generate()
        {
            return new BingoDodgeLeviathanChallenge
            {
            };
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
                "DodgeLeviathan",
                "~",
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
                completed = (array[0] == "1");
                hidden = (array[1] == "1");
                revealed = (array[2] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: DodgeLeviathan FromString() encountered an error: " + ex.Message);
            }
        }
    }
}
