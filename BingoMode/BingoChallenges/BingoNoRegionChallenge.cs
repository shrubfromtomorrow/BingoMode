using BingoMode.BingoSteamworks;
using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
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

        public override Phrase ConstructPhrase()
        {
            return new Phrase([[new Icon("buttonCrossA", 1f, Color.red), new Verse(region.Value)]]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoNoRegionChallenge c || c.region.Value != region.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Avoiding a region");
        }

        public override Challenge Generate()
        {
            string[] regiones = SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).Where(x => x.ToLowerInvariant() != "hr").ToArray();

            BingoNoRegionChallenge ch = new BingoNoRegionChallenge
            {
                region = new(regiones[UnityEngine.Random.Range(0, regiones.Length)], "Region", 0, listName: "regionsreal")
            };

            return ch;
        }

        public override bool RequireSave() => false;
        public override bool ReverseChallenge () => true;

        public void Entered(string regionName)
        {
            if (completed && region.Value == regionName && !TeamsFailed[SteamTest.team])
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
            return string.Concat(new string[]
            {
                "BingoNoRegionChallenge",
                "~",
                region.ToString(),
                "><",
                completed ? "1" : "0",
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
                revealed = (array[2] == "1");
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
            On.WorldLoader.ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues += WorldLoaderNoRegion1;
        }

        public override void RemoveHooks()
        {
            On.WorldLoader.ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues -= WorldLoaderNoRegion1;
        }

        public override List<object> Settings() => [region];
    }
}