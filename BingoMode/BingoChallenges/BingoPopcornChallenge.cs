using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class BingoPopcornRandomizer : Randomizer<Challenge>
    {
        public Randomizer<int> amount;

        public override Challenge Random()
        {
            BingoPopcornChallenge challenge = new();
            challenge.amount.Value = amount.Random();
            return challenge;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}amount-{amount.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "Popcorn").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            MatchCollection matches = Regex.Matches(serialized, SUBRANDOMIZER_PATTERN);
            amount = Randomizer<int>.InitDeserialize(matches[0].ToString());
        }
    }

    public class BingoPopcornChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> amount;

        public BingoPopcornChallenge()
        {
            amount = new(0, "Amount", 0);
        }

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Open [<current>/<amount>] popcorn plants")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amount.Value.ToString());
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new(
                [[new Icon("Symbol_Spear"), new Icon("popcorn_plant", 1f, new Color(0.41f, 0.16f, 0.23f))],
                [new Counter(current, amount.Value)]]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoPopcornChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Popping popcorn plants");
        }

        public override Challenge Generate()
        {
            BingoPopcornChallenge ch = new();
            ch.amount = new(UnityEngine.Random.Range(2, 8), "Amount", 0);
            return ch;
        }

        public void Pop()
        {
            if (!completed && !revealed && !hidden && !TeamsCompleted[SteamTest.team])
            {
                current++;
                UpdateDescription();
                if (current >= (int)amount.Value) CompleteChallenge();
                else ChangeValue();
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

        public override void Reset()
        {
            base.Reset();
            current = 0;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat != MoreSlugcatsEnums.SlugcatStatsName.Saint;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoPopcornChallenge",
                "~",
                current.ToString(),
                "><",
                amount.ToString(),
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
                current = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[1]) as SettingBox<int>;
                completed = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoPopcornChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            IL.SeedCob.HitByWeapon += SeedCob_HitByWeapon;
        }

        public override void RemoveHooks()
        {
            IL.SeedCob.HitByWeapon -= SeedCob_HitByWeapon;
        }

        public override List<object> Settings() => [amount];
    }
}