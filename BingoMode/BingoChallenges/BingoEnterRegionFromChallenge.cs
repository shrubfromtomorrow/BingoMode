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
    public class BingoEnterRegionFromChallenge : BingoChallenge
    {
        public SettingBox<string> from;
        public SettingBox<string> to;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("First time entering <to> must be from <from>")
                .Replace("<to>", Region.GetRegionFullName(to.Value, ExpeditionData.slugcatPlayer))
                .Replace("<from>", Region.GetRegionFullName(from.Value, ExpeditionData.slugcatPlayer));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Verse(from.Value), new Icon("keyShiftA", 1f, new Color(66f / 255f, 135f / 255f, 1f), 180f), new Verse(to.Value)], [1,2]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoEnterRegionFromChallenge;// c || (c.to.Value != to.Value && c.from.Value != from.Value);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Entering a region from another region");
        }

        public string FixSlugSpecificRegions(string gateName)
        {
            string[] regions = ChallengeUtils.GetSortedCorrectListForChallenge("regionsreal");
            if (regions.Contains("UG") && gateName.Contains("DS"))
            {
                gateName = gateName.Replace("DS", "UG");
            }
            if (regions.Contains("CL") && gateName.Contains("SH"))
            {
                gateName = gateName.Replace("SH", "CL");
                gateName = gateName.Replace("UW", "SH");
            }
            //if (regions.Contains("RM") && gateName.Contains("SS"))
            //{
            //    gateName = gateName.Replace("SS", "RM");
            //}
            if (regions.Contains("LM") && gateName.Contains("SL"))
            {
                gateName = gateName.Replace("SL", "LM");
            }
            return gateName;
        }

        public override Challenge Generate()
        {
            List<string> gates = [.. ChallengeUtils.AllGates.ToArray()];

            if (ExpeditionData.slugcatPlayer.value == "Saint") gates.RemoveAll(x => x.Contains("UW"));

            string gateName = gates[UnityEngine.Random.Range(0, gates.Count)];
            gateName = FixSlugSpecificRegions(gateName);            

            string[] regiones = gateName.Split('_');

            BingoEnterRegionFromChallenge ch = new BingoEnterRegionFromChallenge
            {
                from = new(regiones[0], "From", 0, listName: "regionsreal"),
                to = new(regiones[1], "To", 0, listName: "regionsreal")
            };

            return ch;
        }

        public void Gated(string gateName, string newWorld)
        {
            if (completed || TeamsCompleted[SteamTest.team] || hidden || revealed || TeamsFailed[SteamTest.team]) return;

            gateName = FixSlugSpecificRegions(gateName);
            List<string> worlds = gateName.Split('_').ToList();
            worlds.RemoveAt(0);
            //
            //if ((worlds[0] != from.Value || worlds[1] != from.Value) && (worlds[0] != to.Value || worlds[1] != to.Value)) return;
            // 
            string prevWorld = worlds[worlds.IndexOf(newWorld) == 0 ? 1 : 0];
            //

            bool prevCheck = prevWorld == from.Value.ToUpperInvariant();
            bool newCheck = newWorld == to.Value.ToUpperInvariant();

            if (prevCheck && newCheck)
            {
                CompleteChallenge();
                return;
            }

            if (newCheck)
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
                "BingoEnterRegionFromChallenge",
                "~",
                from.ToString(),
                "><",
                to.ToString(),
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
                from = SettingBoxFromString(array[0]) as SettingBox<string>;
                to = SettingBoxFromString(array[1]) as SettingBox<string>;
                completed = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoEnterRegionFromChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.RegionGate.NewWorldLoaded_Room += RegionGate_NewWorldLoaded3;
        }

        public override void RemoveHooks()
        {
            On.RegionGate.NewWorldLoaded_Room -= RegionGate_NewWorldLoaded3;
        }

        public override List<object> Settings() => [from, to];
    }
}