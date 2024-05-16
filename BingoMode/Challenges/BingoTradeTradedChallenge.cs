using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoTradeTradedChallenge : BingoChallenge
    {
        public SettingBox<int> amount;
        public int current;
        public Dictionary<EntityID, EntityID> traderItems; // Key - item, Value - trader (Save this later) (i think i saved this thanks me)

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Trade [<current>/<amount>] " + (amount.Value == 1 ? "item" : "items") + " from Scavenger Merchants to others")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoTradeTradedChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Trading Already Traded Items");
        }

        public override Challenge Generate()
        {
            return new BingoTradeTradedChallenge
            {
                amount = new(UnityEngine.Random.Range(1, 4), "Amount of Items", 0),
                traderItems = []
            };
        }

        public void Traded(EntityID item, EntityID scav)
        {
            Plugin.logger.LogMessage("Traded " + item);
            if (!completed && traderItems.ContainsKey(item) && traderItems[item] != scav)
            {
                Plugin.logger.LogMessage("Suck ces");
                traderItems.Remove(item);
                current++;
                UpdateDescription();
                if (!RequireSave) Expedition.Expedition.coreFile.Save(false);
                if (current >= amount.Value)
                {
                    CompleteChallenge();
                }
            }
        }

        public override void CompleteChallenge()
        {
            base.CompleteChallenge();
            traderItems = [];
        }

        public override int Points()
        {
            return 20;
        }

        public override void Reset()
        {
            base.Reset();
            current = 0;
            traderItems = [];
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
            string text = "";
            foreach (var kvp in traderItems)
            {
                text += kvp.Key.ToString() + "|" + kvp.Value.ToString() + ",";
            }
            text.TrimEnd(',');
            return string.Concat(new string[]
            {
                "BingoTradeTradedChallenge",
                "~",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                text,
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
                traderItems = [];
                string[] dict = array[2].Split(',');
                foreach (var s in dict)
                {
                    string[] kv = s.Split('|');
                    if (kv[0] != string.Empty && kv[1] != string.Empty) traderItems[EntityID.FromString(kv[0])] = EntityID.FromString(kv[1]);
                }
                completed = (array[2] == "1");
                hidden = (array[3] == "1");
                revealed = (array[4] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoTradeTradedChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            IL.ScavengerAI.RecognizeCreatureAcceptingGift += ScavengerAI_RecognizeCreatureAcceptingGift2;
        }

        public override void RemoveHooks()
        {
            IL.ScavengerAI.RecognizeCreatureAcceptingGift -= ScavengerAI_RecognizeCreatureAcceptingGift2;
        }

        public override List<object> Settings() => [amount];
    }
}