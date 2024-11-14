using BingoMode.BingoSteamworks;
using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoAllRegionsExcept : BingoChallenge
    {
        public SettingBox<string> region;
        public List<string> regionsToEnter = [];
        public int current;
        public int required;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Enter all regions except " + Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Icon("TravellerA", 1f, Color.white), new Icon("buttonCrossA", 1f, Color.red), new Verse(region.Value), new Counter(current, required)], [3]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoAllRegionsExcept;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Entering all regions except one");
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
            int req = regiones.Count - 1;

            return new BingoAllRegionsExcept
            {
                region = new(regionn, "Region", 0, listName: "regions"),
                regionsToEnter = regiones,
                required = req
            };
        }

        public override bool RequireSave() => false;

        public void Entered(string regionName)
        {
            if (SteamTest.team == 8) return;
            if (region.Value == regionName && !TeamsFailed[SteamTest.team])
            {
                FailChallenge(SteamTest.team);
                return;
            }
            else if (!TeamsFailed[SteamTest.team] && !completed && !revealed && !hidden && !TeamsCompleted[SteamTest.team] && regionsToEnter.Contains(regionName))
            {
                Plugin.logger.LogMessage("Visited " + regionName);
                regionsToEnter.Remove(regionName);

                current++;
                UpdateDescription();
                if (current >= required)
                {
                    CompleteChallenge();
                }
                else
                {
                    ChangeValue();
                    Expedition.Expedition.coreFile.Save(false);
                }
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
                current.ToString(),
                "><",
                required.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                TeamsToString()
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                region = SettingBoxFromString(array[0]) as SettingBox<string>;
                regionsToEnter = [.. array[1].Split('|')];
                current = int.Parse(array[2], System.Globalization.NumberStyles.Any);
                required = int.Parse(array[3], System.Globalization.NumberStyles.Any);
                completed = (array[4] == "1");
                hidden = (array[5] == "1");
                revealed = (array[6] == "1");
                TeamsFromString(array[7]);
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