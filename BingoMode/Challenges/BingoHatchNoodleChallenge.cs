using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoHatchNoodleChallenge : BingoChallenge
    {
        public SettingBox<int> amount;
        public int current;
        public SettingBox<bool> atOnce;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Hatch [<current>/<amount>] noodlefly eggs" + (atOnce.Value ? " in one cycle" : ""))
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase p = new Phrase([new Icon("needleEggSymbol", 1f, ChallengeUtils.ItemOrCreatureIconColor("needleEggSymbol")), new Icon("Kill_SmallNeedleWorm", 1f, ChallengeUtils.ItemOrCreatureIconColor("SmallNeedleWorm"))], [atOnce.Value ? 3 : 2]);
            if (atOnce.Value) p.words.Add(new Icon("cycle_limit", 1f, UnityEngine.Color.white));
            p.words.Add(new Counter((atOnce.Value && completed) ? amount.Value : current, amount.Value));
            return p;
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
            bool onc = UnityEngine.Random.value < 0.5f;
            return new BingoHatchNoodleChallenge
            {
                atOnce = new(onc, "At Once", 0),
                amount = new(UnityEngine.Random.Range(1, onc ? 4 : 6), "Amount", 1),
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