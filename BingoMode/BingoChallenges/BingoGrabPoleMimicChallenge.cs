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

        public override Challenge Random()
        {
            BingoGrabPoleMimicChallenge challenge = new();
            challenge.amount.Value = amount.Random();
            return challenge;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}amount-{amount.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "GrabPoleMimic").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            Dictionary<string, string> dict = ToDict(serialized);
            amount = Randomizer<int>.InitDeserialize(dict["amount"]);
        }
    }

    public class BingoGrabPoleMimicChallenge : BingoChallenge
    {
        public SettingBox<int> amount;
        public int current;

        public BingoGrabPoleMimicChallenge()
        {
            amount = new(0, "Amount", 0);
        }

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            description = ChallengeTools.IGT.Translate("Grab [<current>/<amount>] Pole Mimics")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new(
                [
                [new Icon("steal_item"), Icon.FromEntityName("PoleMimic")],
                [new Counter(current, amount.Value)]
                ]);
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
            return new BingoGrabPoleMimicChallenge()
            {
                amount = new(UnityEngine.Random.Range(1, 4), "Amount", 0)
            };
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
            return slugcat.value != "Saint";
        }

        public override void AddHooks()
        {
            On.PoleMimic.BeingClimbedOn += PoleMimic_BeingClimedOn;
        }

        public override void RemoveHooks()
        {
            On.PoleMimic.BeingClimbedOn -= PoleMimic_BeingClimedOn;
        }

        public override List<object> Settings() => [amount];
    }
}
