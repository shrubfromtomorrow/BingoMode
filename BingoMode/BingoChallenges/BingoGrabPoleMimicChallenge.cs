using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class BingoGrabPoleMimicRandomizer : ChallengeRandomizer
    {
        public Randomizer<int> amount;
        public Randomizer<string> region;
        public Randomizer<bool> oneCycle;

        public override Challenge Random()
        {
            BingoGrabPoleMimicChallenge challenge = new();
            challenge.amount.Value = amount.Random();
            challenge.region.Value = region.Random();
            challenge.oneCycle.Value = oneCycle.Random();
            return challenge;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}amount-{amount.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}region-{region.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}oneCycle-{oneCycle.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "GrabPoleMimic").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            Dictionary<string, string> dict = ToDict(serialized);
            amount = Randomizer<int>.InitDeserialize(dict["amount"]);
            region = Randomizer<string>.InitDeserialize(dict["region"]);
            oneCycle = Randomizer<bool>.InitDeserialize(dict["oneCycle"]);
        }
    }

    public class BingoGrabPoleMimicChallenge : BingoChallenge
    {
        public SettingBox<int> amount;
        public SettingBox<string> region;
        public SettingBox<bool> oneCycle;
        public int current;

        public BingoGrabPoleMimicChallenge()
        {
            amount = new(0, "Amount", 0);
            oneCycle = new(false, "In one Cycle", 3);
            region = new("", "Region", 5, listName: "regions");
        }

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            string location = region.Value != "Any Region" ? Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer) : "";
            description = ChallengeTools.IGT.Translate("Grab [<current>/<amount>] Pole Mimics <location><onecycle>")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value))
                .Replace("<location>", location != "" ? " in " + location : "")
                .Replace("<onecycle>", oneCycle.Value ? " in one cycle" : "")
                ;
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new([[new Icon("steal_item"), Icon.FromEntityName("PoleMimic")]]);
            int lastLine = 1;
            if (region.Value != "Any Region")
            {
                phrase.InsertWord(new Verse(region.Value), 1);
                lastLine = 2;
            }

            phrase.InsertWord(new Counter(current, amount.Value), lastLine);
            if (oneCycle.Value) phrase.InsertWord(new Icon("cycle_limit"), lastLine);
            return phrase;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Grabing Pole Mimics");
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoGrabPoleMimicChallenge;
        }

        public override Challenge Generate()
        {
            bool oneCycle = UnityEngine.Random.value < 0.2f;
            int amount = UnityEngine.Random.Range(1, oneCycle ? 2 : 5);
            return new BingoGrabPoleMimicChallenge()
            {
                amount = new(amount, "Amount", 0),
                oneCycle = new(oneCycle, "In one Cycle", 3),
                region = new("Any Region", "Region", 5, listName: "regions"),
            };
        }

        public override void Update()
        {
            base.Update();
            if (!completed && oneCycle.Value && game != null && game.cameras.Length > 0 && game.cameras[0].room != null && game.cameras[0].room.shelterDoor != null && game.cameras[0].room.shelterDoor.IsClosing)
            {
                if (current != 0)
                {
                    current = 0;
                    UpdateDescription();
                }
                return;
            }
        }

        public void Grabbed()
        {
            if (!completed && !TeamsCompleted[SteamTest.team] && !hidden && !revealed)
            {
                current++;
                UpdateDescription();
                if (current >= amount.Value) CompleteChallenge();
                else ChangeValue();
            }
        }

        public override bool CombatRequired()
        {
            return true;
        }

        public override int Points()
        {
            return 20;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoGrabPoleMimicChallenge",
                "~",
                amount.ToString(),
                "><",
                current.ToString(),
                "><",
                region.ToString(),
                "><",
                oneCycle.ToString(),
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
                amount = SettingBoxFromString(array[0]) as SettingBox<int>;
                current = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                region = SettingBoxFromString(array[4]) as SettingBox<string>;
                oneCycle = SettingBoxFromString(array[5]) as SettingBox<bool>;
                completed = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoGrabPoleMimicChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public override void AddHooks()
        {
            On.PoleMimic.BeingClimbedOn += PoleMimic_BeingClimedOn;
        }

        public override void RemoveHooks()
        {
            On.PoleMimic.BeingClimbedOn -= PoleMimic_BeingClimedOn;
        }

        public override List<object> Settings() => [amount, region, oneCycle];
    }
}
