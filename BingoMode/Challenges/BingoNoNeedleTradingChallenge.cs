using BingoMode.BingoSteamworks;
using Expedition;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoNoNeedleTradingChallenge : BingoChallenge
    {
        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Do not gift Needles to Scavengers");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Icon("spearneedle", 1f, Color.white), 
                               new Icon("commerce", 1f, Color.white), 
                               new Icon("Kill_Scavenger", 1f, Color.white),
                               new Icon("buttonCrossA", 1f, Color.red)], [3]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoNoNeedleTradingChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Avoiding gifting Needles to Scavengers");
        }

        public override Challenge Generate()
        {
            BingoNoNeedleTradingChallenge ch = new BingoNoNeedleTradingChallenge
            {
            };

            return ch;
        }

        public override bool RequireSave() => false;
        public override bool ReverseChallenge () => true;

        public void Traded()
        {
            if (TeamsFailed[SteamTest.team] || !completed) return;
            FailChallenge(SteamTest.team);
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
            return slugcat.value == "Spear";
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoNoNeedleTradingChallenge",
                "~",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0"
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                completed = (array[0] == "1");
                revealed = (array[1] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoNoNeedleTradingChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            IL.ScavengerAI.RecognizeCreatureAcceptingGift += ScavengerAI_RecognizeCreatureAcceptingGiftNeedles;
            On.ScavengerAI.RecognizePlayerOfferingGift += ScavengerAI_RecognizePlayerOfferingGift;
        }

        public override void RemoveHooks()
        {
            IL.ScavengerAI.RecognizeCreatureAcceptingGift -= ScavengerAI_RecognizeCreatureAcceptingGiftNeedles;
            On.ScavengerAI.RecognizePlayerOfferingGift -= ScavengerAI_RecognizePlayerOfferingGift;
        }

        public override List<object> Settings() => [];
    }
}