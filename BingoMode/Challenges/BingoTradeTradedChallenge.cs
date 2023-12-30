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
    public class BingoTradeTradedChallenge : Challenge
    {
        public int amount;
        public int current;
        public List<EntityID> traderItems;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Trade [<current>/<amount>] " + (amount == 1 ? "item" : "items") + " from Scavenger Merchants to others")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoTradeTradedChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Trading traded");
        }

        public override Challenge Generate()
        {
            return new BingoTradeTradedChallenge
            {
                amount = UnityEngine.Random.Range(1, 4),
                traderItems = []
            };
        }

        public void Traded(EntityID id)
        {
            Plugin.logger.LogMessage("Traded " + id);
            if (traderItems.Contains(id))
            {
                Plugin.logger.LogMessage("Suck ces");
                traderItems.Remove(id);
                current++;
                UpdateDescription();
                if (!completed && current >= amount)
                {
                    CompleteChallenge();
                }
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
            return slugcat != MoreSlugcatsEnums.SlugcatStatsName.Artificer;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "TradeTraded",
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
                ExpLog.Log("ERROR: TradeTraded FromString() encountered an error: " + ex.Message);
            }
        }
    }
}