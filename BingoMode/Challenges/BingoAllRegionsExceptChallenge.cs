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
    public class BingoAllRegionsExcept : Challenge
    {
        public string region;
        public List<string> regionsToEnter;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Enter all regions except " + region);
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoAllRegionsExcept;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Region ban 2");
        }

        public override Challenge Generate()
        {
            List<string> regiones = SlugcatStats.getSlugcatStoryRegions(ExpeditionData.slugcatPlayer).ToList();
            string regionn = regiones[UnityEngine.Random.Range(0, regiones.Count)];
            regiones.Remove(regionn);

            foreach (var s in regiones) Plugin.logger.LogMessage(s);

            return new BingoAllRegionsExcept
            {
                region = regionn,
                regionsToEnter = regiones
            };
        }

        public void Entered(string regionName)
        {
            if (completed && region == regionName)
            {
                completed = false;
                regionsToEnter.Add("failed");
                return;
            }
            else if (!completed && regionsToEnter.Contains(regionName))
            {
                regionsToEnter.Remove(regionName);
                foreach (var s in regionsToEnter) Plugin.logger.LogMessage(s);

                if (regionsToEnter.Count == 0)
                {
                    CompleteChallenge();
                }
                UpdateDescription();
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
                "RegionBan2",
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
                ExpLog.Log("ERROR: Region Ban 2 FromString() encountered an error: " + ex.Message);
            }
        }
    }
}