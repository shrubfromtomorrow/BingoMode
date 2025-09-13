using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoEnterRegionsTimesRandomizer : ChallengeRandomizer
    {
        public Randomizer<string> region;
        public Randomizer<int> min;
        public Randomizer<int> max;

        public override Challenge Random()
        {
            BingoEnterRegionsTimesChallenge challenge = new();
            challenge.region.Value = region.Random();
            challenge.min.Value = min.Random();
            challenge.max.Value = max.Random();
            return challenge;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}region-{region.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}min-{min.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}max-{max.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "EnterRegionsTimes").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            Dictionary<string, string> dict = ToDict(serialized);
            region = Randomizer<string>.InitDeserialize(dict["region"]);
            min = Randomizer<int>.InitDeserialize(dict["min"]);
            max = Randomizer<int>.InitDeserialize(dict["max"]);
        }
    }
    public class BingoEnterRegionsTimesChallenge : BingoChallenge
    {
        public SettingBox<string> region;
        public SettingBox<int> min;
        public SettingBox<int> max;
        public List<string> enteredRegions = [];
        public int current;

        public BingoEnterRegionsTimesChallenge()
        {
            region = new("", "Region", 0, listName: "regionsreal");
            min = new(0, "Minimum", 1);
            max = new(0, "Maximum", 2);
        }

        public override void UpdateDescription()
        {
            string descriptionString = "Enter "
                + (region.Value == "Any Region" ? "unvisited regions" : Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer))
                + " <min><<current><<max> times"
                ;
            this.description = ChallengeTools.IGT.Translate(descriptionString)
                .Replace("<min>", min.Value.ToString())
                .Replace("<current>", current.ToString())
                .Replace("<max>", max.Value.ToString())
                ;
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new Phrase([
                [new Icon("keyShiftA", 1f, Color.cyan, 90), new Verse("<>")],
                [new Range(min.Value, current, max.Value)]
            ]);
            phrase.InsertWord(region.Value == "Any Region" ? new Icon("TravellerA") : new Verse(region.Value));
            return phrase;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoAllRegionsExcept;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Entering regions for a limited amount of times");
        }

        public override void Reset()
        {
            base.Reset();
            current = 0;
            enteredRegions = [];
        }

        public override Challenge Generate()
        {
            List<string> regions = ChallengeUtils.GetSortedCorrectListForChallenge("regionsreal").ToList();
            bool anyRegion = UnityEngine.Random.value < 0.5f;
            string region_ = anyRegion ? "Any Region" : regions[UnityEngine.Random.Range(0, regions.Count)];
            int max_ = UnityEngine.Random.Range(3, 6);
            int min_ = UnityEngine.Random.Range(1, 3);

            if (anyRegion) {
                max_ *= 2;
            }

            return new BingoEnterRegionsTimesChallenge
            {
                region = new(region_, "Region", 0, listName: "regions"),
                enteredRegions = [],
                min = new(min_, "Minimum", 1),
                max = new(max_, "Maximum", 2)
            };
        }

        public void Entered(string regionName)
        {
            if (SteamTest.team == 8 || hidden || TeamsFailed[SteamTest.team]) return;

            if (region.Value == "Any Region")
            {
                if (enteredRegions.Contains(regionName)) return;
                enteredRegions.Add(regionName);
                current++;
            }
            else if (region.Value == regionName)
            {
                current++;
            }
            else return;

            ChangeValue();
            UpdateDescription();
            if (completed || TeamsCompleted[SteamTest.team])
            {
                if (current > max.Value)
                    FailChallenge(SteamTest.team);
                return;
            }

            if (!revealed && current >= min.Value)
            {
                CompleteChallenge();
                return;
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
                // Prevents future footguns when renaming
                nameof(BingoEnterRegionsTimesChallenge),
                "~",
                region.ToString(),
                "><",
                string.Join("|", enteredRegions),
                "><",
                current.ToString(),
                "><",
                min.ToString(),
                "><",
                max.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                region = SettingBoxFromString(array[0]) as SettingBox<string>;
                enteredRegions = [.. array[1].Split('|')];
                current = int.Parse(array[2], System.Globalization.NumberStyles.Any);
                min = SettingBoxFromString(array[3]) as SettingBox<int>;
                max = SettingBoxFromString(array[4]) as SettingBox<int>;
                completed = (array[5] == "1");
                revealed = (array[6] == "1");
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
            On.WorldLoader.ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues += WorldLoader_RegionsTimes;
        }

        public override void RemoveHooks()
        {
            On.WorldLoader.ctor_RainWorldGame_Name_Timeline_bool_string_Region_SetupValues -= WorldLoader_RegionsTimes;
        }

        public override List<object> Settings() => [region, min, max];
    }
}
