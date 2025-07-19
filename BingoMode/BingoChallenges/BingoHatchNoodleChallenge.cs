using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class BingoHatchNoodleRandomizer : Randomizer<Challenge>
    {
        public Randomizer<int> amount;
        public Randomizer<bool> atOnce;

        public override Challenge Random()
        {
            BingoHatchNoodleChallenge challenge = new();
            challenge.amount.Value = amount.Random();
            challenge.atOnce.Value = atOnce.Random();
            return challenge;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}amount-{amount.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}atOnce-{atOnce.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "HatchNoodle").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            MatchCollection matches = Regex.Matches(serialized, SUBRANDOMIZER_PATTERN);
            amount = Randomizer<int>.InitDeserialize(matches[0].ToString());
            atOnce = Randomizer<bool>.InitDeserialize(matches[1].ToString());
        }
    }

    public class BingoHatchNoodleChallenge : BingoChallenge
    {
        public SettingBox<int> amount;
        public int current;
        public SettingBox<bool> atOnce;

        public BingoHatchNoodleChallenge()
        {
            atOnce = new(false, "At Once", 0);
            amount = new(0, "Amount", 1);
        }

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Hatch [<current>/<amount>] noodlefly eggs" + (atOnce.Value ? " in one cycle" : ""))
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new(
                [[new Icon("needleEggSymbol", 1f, ChallengeUtils.ItemOrCreatureIconColor("needleEggSymbol")), new Icon("Kill_SmallNeedleWorm", 1f, ChallengeUtils.ItemOrCreatureIconColor("SmallNeedleWorm"))],
                [new Counter((atOnce.Value && completed) ? amount.Value : current, amount.Value)]]);
            if (atOnce.Value) phrase.InsertWord(new Icon("cycle_limit"));
            return phrase;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoHatchNoodleChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Hatching noodlefly eggs");
        }

        public override Challenge Generate()
        {
            bool onc = UnityEngine.Random.value < 0.33f;
            return new BingoHatchNoodleChallenge
            {
                atOnce = new(onc, "At Once", 0),
                amount = new(UnityEngine.Random.Range(onc ? 2 : 1, onc ? 4 : 6), "Amount", 1),
            };
        }

        public void Hatch()
        {
            if (!completed && !revealed && !TeamsCompleted[SteamTest.team] && !hidden)
            {
                current++;
                UpdateDescription();
                if (current >= amount.Value) CompleteChallenge();
                else ChangeValue();
            }
        }

        public override int Points()
        {
            return amount.Value * 10;
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat.value != "Saint";
        }

        public override void Reset()
        {
            base.Reset();
            current = 0;
        }

        public override string ToString()
        {
            if (atOnce.Value) current = 0;
            return string.Concat(new string[]
            {
                "BingoHatchNoodleChallenge",
                "~",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                atOnce.ToString(),
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
                atOnce = SettingBoxFromString(array[2]) as SettingBox<bool>;
                current = (atOnce.Value && !completed) ? 0 : int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[1]) as SettingBox<int>;
                completed = (array[3] == "1");
                revealed = (array[4] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoHatchNoodleChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.ShelterDoor.Close += ShelterDoor_Close;
        }

        public override void RemoveHooks()
        {
            On.ShelterDoor.Close -= ShelterDoor_Close;
        }

        public override List<object> Settings() => [atOnce, amount];
        public List<string> SettingNames() => ["At Once", "Amount"];
    }
}