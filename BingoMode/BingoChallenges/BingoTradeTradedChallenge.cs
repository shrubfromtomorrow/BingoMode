using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using BingoMode.BingoSteamworks;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoTradeTradedChallenge : BingoChallenge
    {
        public SettingBox<int> amount;
        public int current;
        public Dictionary<EntityID, string> traderItems; // Key - item, Value - room (Save this later) (i think i saved this thanks me)

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Trade [<current>/<amount>] " + (amount.Value == 1 ? "item" : "items") + " from Scavenger Merchants to other Scavenger Merchants")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Icon("scav_merchant", 1f, Color.white), new Icon("Menu_Symbol_Shuffle", 1f, Color.white), new Icon("scav_merchant", 1f, Color.white), new Counter(current, amount.Value)], [3]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoTradeTradedChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Trading already traded items");
        }

        public override Challenge Generate()
        {
            return new BingoTradeTradedChallenge
            {
                amount = new(UnityEngine.Random.Range(1, 4), "Amount of Items", 0),
                traderItems = []
            };
        }

        public void Traded(EntityID item, string room)
        {
            
            if (!completed && !revealed && !hidden && !TeamsCompleted[SteamTest.team] && traderItems.ContainsKey(item) && traderItems[item].ToLowerInvariant() != room.ToLowerInvariant())
            {
                
                traderItems.Remove(item);
                current++;
                UpdateDescription();
                if (current >= amount.Value)
                {
                    CompleteChallenge();
                }
                else ChangeValue();
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
            if (string.IsNullOrEmpty(text)) text = "empty";
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
                traderItems = [];
                traderItems.Clear();
                if (array[2] != "empty")
                {
                    string[] dict = array[2].Split(',');
                    foreach (var s in dict)
                    {
                        string[] kv = s.Split('|');
                        if (kv[0] != string.Empty && kv[1] != string.Empty) traderItems[EntityID.FromString(kv[0])] = kv[1];
                    }
                }
                completed = (array[3] == "1");
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
            On.Scavenger.Grab += Scavenger_Grab;
            IL.Room.Loaded += Room_LoadedBlessedNeedles;
        }

        public override void RemoveHooks()
        {
            IL.ScavengerAI.RecognizeCreatureAcceptingGift -= ScavengerAI_RecognizeCreatureAcceptingGift2;
            On.Scavenger.Grab -= Scavenger_Grab;
            IL.Room.Loaded -= Room_LoadedBlessedNeedles;
        }

        public override List<object> Settings() => [amount];
    }
}