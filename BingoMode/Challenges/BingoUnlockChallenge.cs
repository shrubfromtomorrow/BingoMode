using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using RWCustom;
using System.Reflection;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoUnlockChallenge : Challenge, IBingoChallenge
    {
        public string unlock;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Get the " + unlock + " unlock");
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoUnlockChallenge || (challenge as BingoUnlockChallenge).unlock != unlock;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Getting Unlocks");
        }

        public override Challenge Generate()
        {
            int type = UnityEngine.Random.Range(0, ModManager.MSC ? (SlugcatStats.IsSlugcatFromMSC(ExpeditionData.slugcatPlayer) ? 4 : 3) : 2);
            string unl = "ERROR";

            try
            {
                unl = BingoData.possibleTokens[type][UnityEngine.Random.Range(0, BingoData.possibleTokens[type].Count)];
            }
            catch (Exception e)
            {
                Plugin.logger.LogError("Oops, errore in generating unlock chalange " + e);
            }
            if (unl == "ERROR") return null;

            return new BingoUnlockChallenge
            {
                unlock = unl
            };
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
            return true;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoUnlockChallenge",
                "~",
                unlock,
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
                unlock = array[0];
                completed = (array[1] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: UnlockChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            IL.Room.Loaded += Room_LoadedUnlock;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_string_bool += MiscProgressionData_GetTokenCollected;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SafariUnlockID += MiscProgressionData_GetTokenCollected_SafariUnlockID;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SlugcatUnlockID += MiscProgressionData_GetTokenCollected_SlugcatUnlockID;
            tokenColorHook = new(typeof(CollectToken).GetProperty("TokenColor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetGetMethod(), CollectToken_TokenColor_get);
            On.CollectToken.Pop += CollectToken_Pop;
        }

        public void RemoveHooks()
        {

            IL.Room.Loaded -= Room_LoadedUnlock;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_string_bool -= MiscProgressionData_GetTokenCollected;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SafariUnlockID -= MiscProgressionData_GetTokenCollected_SafariUnlockID;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SlugcatUnlockID -= MiscProgressionData_GetTokenCollected_SlugcatUnlockID;
            tokenColorHook?.Dispose();
            On.CollectToken.Pop -= CollectToken_Pop;
        }
    }
}