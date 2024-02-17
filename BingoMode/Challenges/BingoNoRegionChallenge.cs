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
    public class BingoNoRegionChallenge : Challenge, IBingoChallenge
    {
        public SettingBox<string> region;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Do not enter " + region.Value);
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoNoRegionChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Region Avoiding");
        }

        public override Challenge Generate()
        {
            string[] regiones = SlugcatStats.getSlugcatStoryRegions(ExpeditionData.slugcatPlayer);

            return new BingoNoRegionChallenge
            {
                region = new(regiones[UnityEngine.Random.Range(0, regiones.Length)], "Region", 0, listName: "regions"),
                completed = true,
            };
        }

        public void Entered(string regionName)
        {
            if (completed && region.Value == regionName)
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
            Plugin.logger.LogMessage(region.ToString());
            return string.Concat(new string[]
            {
                "BingoNoRegionChallenge",
                "~",
                region.ToString(),
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
                region = SettingBoxFromString(array[0]) as SettingBox<string>;
                completed = (array[1] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoNoRegionChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues += WorldLoaderNoRegion1;
        }

        public void RemoveHooks()
        {
            On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues -= WorldLoaderNoRegion1;
        }

        public List<object> Settings() => [region];
    }
}