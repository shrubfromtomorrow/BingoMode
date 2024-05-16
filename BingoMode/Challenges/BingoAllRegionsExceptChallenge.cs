using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoAllRegionsExcept : BingoChallenge
    {
        public SettingBox<string> region;
        public List<string> regionsToEnter = [];

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Enter all regions except " + region.Value);
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoAllRegionsExcept;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Entering all Regions Except X");
        }

        public override void Reset()
        {
            base.Reset();
            regionsToEnter = SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).ToList();
        }

        public override Challenge Generate()
        {
            List<string> regiones = SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).ToList();
            string regionn = regiones[UnityEngine.Random.Range(0, regiones.Count)];
            regiones.Remove(regionn);

            foreach (var s in regiones) Plugin.logger.LogMessage(s);

            return new BingoAllRegionsExcept
            {
                region = new(regionn, "Region", 0, listName: "regions"),
                regionsToEnter = regiones
            };
        }

        public void Entered(string regionName)
        {
            if (completed && region.Value == regionName)
            {
                completed = false;
                regionsToEnter.Add("failed");
                Failed = true;
                return;
            }
            else if (!completed && regionsToEnter.Contains(regionName))
            {
                Plugin.logger.LogMessage("Visited " + regionName);
                regionsToEnter.Remove(regionName);

                if (regionsToEnter.Count == 0)
                {
                    CompleteChallenge();
                }
                UpdateDescription();
                if (!RequireSave) Expedition.Expedition.coreFile.Save(false);
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
                "BingoAllRegionsExcept",
                "~",
                region.ToString(),
                "><",
                string.Join("|", regionsToEnter),
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
                regionsToEnter = [.. array[1].Split('|')];
                completed = (array[2] == "1");
                hidden = (array[3] == "1");
                revealed = (array[4] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoAllRegionsExcept FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues += WorldLoaderNoRegion2;
        }

        public override void RemoveHooks()
        {
            On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues -= WorldLoaderNoRegion2;
        }

        public override List<object> Settings() => [region];
    }
}