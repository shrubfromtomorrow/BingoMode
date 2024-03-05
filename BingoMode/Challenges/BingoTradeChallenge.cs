using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoTradeChallenge : Challenge, IBingoChallenge
    {
        public SettingBox<int> amount;
        public int current;
        public List<EntityID> bannedIDs;
        public int Index { get; set; }
        public bool Locked { get; set; }
        public bool Failed { get; set; }

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Trade [<current>/<amount>] worth of value to Scavenger Merchants")
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value))
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
            int amou = UnityEngine.Random.Range(9, 21);

            return new BingoTradeChallenge
            {
                amount = new(amou, "Value", 0),
                bannedIDs = []
            };
        }

        public void Traded(int pnts, EntityID ID)
        {
            Plugin.logger.LogMessage("Traded " + pnts);
            if (!bannedIDs.Contains(ID))
            {
                current += pnts;
                bannedIDs.Add(ID);
                UpdateDescription();

                if (!completed && current >= amount.Value)
                {
                    CompleteChallenge();
                }
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

        public override void Reset()
        {
            base.Reset();
            current = 0;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat != MoreSlugcatsEnums.SlugcatStatsName.Artificer;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoTradeChallenge",
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
                amount = SettingBoxFromString(array[1]) as SettingBox<int>;
                completed = (array[2] == "1");
                hidden = (array[3] == "1");
                revealed = (array[4] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoTradeChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            IL.ScavengerAI.RecognizeCreatureAcceptingGift += ScavengerAI_RecognizeCreatureAcceptingGift1;
        }

        public void RemoveHooks()
        {
            IL.ScavengerAI.RecognizeCreatureAcceptingGift -= ScavengerAI_RecognizeCreatureAcceptingGift1;
        }

        public List<object> Settings() => [amount];
    }
}