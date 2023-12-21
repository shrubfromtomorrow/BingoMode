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
    public class BingoNoRegionChallenge : Challenge
    {
        public string region;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Do not enter " + region);
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoNoRegionChallenge s || s.region != region;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Region ban");
        }

        public override Challenge Generate()
        {
            string[] regiones = SlugcatStats.getSlugcatStoryRegions(ExpeditionData.slugcatPlayer);

            return new BingoNoRegionChallenge
            {
                region = regiones[UnityEngine.Random.Range(0, regiones.Length)],
                completed = true,
            };
        }

        public void Entered(string regionName)
        {
            if (completed && region == regionName)
            {
                completed = false;
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
                "RegionBan",
                "~",
                region,
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
                region = array[0];
                completed = (array[1] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: Region Ban FromString() encountered an error: " + ex.Message);
            }
        }
    }
}