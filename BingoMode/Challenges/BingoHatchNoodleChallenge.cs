using Expedition;
using Menu.Remix;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoHatchNoodleChallenge : Challenge, IBingoChallenge
    {
        public SettingBox<int> amount;
        public int current;
        public SettingBox<bool> atOnce;
        public int Index { get; set; }
        public bool RequireSave { get; set; }
        public bool Failed { get; set; }

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Hatch [<current>/<amount>] noodlefly eggs" + (atOnce.Value ? " at once" : ""))
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoHatchNoodleChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Noodlefly Hatching");
        }

        public override Challenge Generate()
        {
            bool onc = UnityEngine.Random.value < 0.5f;
            return new BingoHatchNoodleChallenge
            {
                atOnce = new(onc, "At Once", 0),
                amount = new(UnityEngine.Random.Range(2, onc ? 3 : 6), "Amount", 1)
            };
        }

        public void Hatch()
        {
            if (!completed)
            {
                current++;
                UpdateDescription();
                if (!RequireSave) Expedition.Expedition.coreFile.Save(false);
                if (current >= amount.Value) CompleteChallenge();
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
            return true;
        }

        public override void Reset()
        {
            base.Reset();
            current = 0;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoHatchNoodleChallenge",
                "~",
                atOnce.Value ? "0" : current.ToString(),
                "><",
                amount.ToString(),
                "><",
                atOnce.ToString(),
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
                current = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[1]) as SettingBox<int>;
                atOnce = SettingBoxFromString(array[2]) as SettingBox<bool>;
                completed = (array[3] == "1");
                hidden = (array[4] == "1");
                revealed = (array[5] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoHatchNoodleChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public void AddHooks()
        {
            On.SmallNeedleWorm.PlaceInRoom += SmallNeedleWorm_PlaceInRoom;
        }

        public void RemoveHooks()
        {
            On.SmallNeedleWorm.PlaceInRoom -= SmallNeedleWorm_PlaceInRoom;
        }

        public List<object> Settings() => [atOnce, amount];
        public List<string> SettingNames() => ["At Once", "Amount"];
    }
}