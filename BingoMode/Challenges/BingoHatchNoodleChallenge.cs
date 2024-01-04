using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoHatchNoodleChallenge : Challenge
    {
        public int amount;
        public int current;
        public bool atOnce;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Hatch [<current>/<amount>] noodlefly eggs" + (atOnce ? " at once" : ""))
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoHatchNoodleChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Noodle hatching");
        }

        public override Challenge Generate()
        {
            bool onc = UnityEngine.Random.value < 0.5f;
            return new BingoHatchNoodleChallenge
            {
                atOnce = onc,
                amount = UnityEngine.Random.Range(1, onc ? 3 : 6)
            };
        }

        public void Hatch()
        {
            if (!completed)
            {
                current++;
                UpdateDescription();
                if (current >= amount) CompleteChallenge();
            }
        }

        public override int Points()
        {
            return amount * 10;
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
                "Hatching",
                "~",
                current.ToString(),
                "><",
                amount.ToString(),
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
                amount = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                completed = (array[2] == "1");
                hidden = (array[3] == "1");
                revealed = (array[4] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: Popcorn FromString() encountered an error: " + ex.Message);
            }
        }
    }
}