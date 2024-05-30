using BingoMode.BingoSteamworks;
using Expedition;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoNoRegionChallenge : BingoChallenge
    {
        public SettingBox<string> region;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Do not enter " + Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer));
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
            string[] regiones = SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).ToArray();

            BingoNoRegionChallenge ch = new BingoNoRegionChallenge
            {
                region = new(regiones[UnityEngine.Random.Range(0, regiones.Length)], "Region", 0, listName: "regions"),
                RequireSave = false,
                ReverseChallenge = true
            };

            return ch;
        }

        public void Entered(string regionName)
        {
            if (completed && region.Value == regionName && !Failed)
            {
                FailChallenge(SteamTest.team);
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
                revealed ? "1" : "0",
                "><",
                TeamsToString(),
                "><",
                Failed ? "1" : "0",
                "><",
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
                TeamsFromString(array[4]);
                Failed = array[5] == "1";
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoNoRegionChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues += WorldLoaderNoRegion1;
        }

        public override void RemoveHooks()
        {
            On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues -= WorldLoaderNoRegion1;
        }

        public override List<object> Settings() => [region];
    }
}