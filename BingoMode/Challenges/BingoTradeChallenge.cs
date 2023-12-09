using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BingoMode.Challenges
{
    public class BingoTradeChallenge : Challenge
    {
        public int amount;
        public int current;
        public List<EntityID> bannedIDs;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Trade [<current>/<amount>] worth of value to Scavenger Merchants")
                .Replace("<amount>", ValueConverter.ConvertToString(amount))
                .Replace("<current>", ValueConverter.ConvertToString(current));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoTradeChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Trading");
        }

        public override Challenge Generate()
        {
            int amou = UnityEngine.Random.Range(5, 21);

            return new BingoTradeChallenge
            {
                amount = amou,
            };
        }

        public void Traded(int pnts, EntityID ID)
        {
            if (!completed && !bannedIDs.Contains(ID))
            {
                current = Mathf.Min(current + pnts, amount);
                UpdateDescription();
                bannedIDs.Add(ID);
                
                if (current >= amount)
                {
                    CompleteChallenge();
                }
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
            return slugcat != MoreSlugcatsEnums.SlugcatStatsName.Artificer;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "Trading",
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
                ExpLog.Log("ERROR: Trading FromString() encountered an error: " + ex.Message);
            }
        }
    }
}