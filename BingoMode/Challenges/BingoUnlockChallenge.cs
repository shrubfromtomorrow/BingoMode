using Expedition;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoUnlockChallenge : BingoChallenge
    {
        public SettingBox<string> unlock;

        public override void UpdateDescription()
        {
            description = "Get the " + ChallengeTools.IGT.Translate(unlock.Value) + " unlock";
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            UnlockIconData data = IconDataForUnlock(unlock.Value);
            return new Phrase([new Icon("arenaunlock", 1f, data.iconColor), (data.unlockIconName == "" ? new Verse(unlock.Value) : new Icon(data.unlockIconName, 1f, data.unlockIconColor))], [1]);
        }

        public struct UnlockIconData
        {
            public Color iconColor;
            public string unlockIconName;
            public Color unlockIconColor;
        }

        public static UnlockIconData IconDataForUnlock(string unlockName)
        {
            UnlockIconData data = new UnlockIconData();
            if (AbstractPhysicalObject.AbstractObjectType.values.entries.Contains(unlockName) || CreatureTemplate.Type.values.entries.Contains(unlockName))
            {
                data.unlockIconName = ChallengeUtils.ItemOrCreatureIconName(unlockName);
                data.unlockIconColor = ChallengeUtils.ItemOrCreatureIconColor(unlockName);
                data.iconColor = RainWorld.AntiGold.rgb;
            }
            else if (SlugcatStats.Name.values.entries.Contains(unlockName))
            {
                data.unlockIconName = "Kill_Slugcat";
                data.unlockIconColor = PlayerGraphics.SlugcatColor(new SlugcatStats.Name(unlockName, false));
                data.iconColor = CollectToken.GreenColor.rgb;
            }
            else if (unlockName.EndsWith("-safari"))
            {
                data.unlockIconName = "";
                data.unlockIconColor = Color.white;
                data.iconColor = CollectToken.RedColor.rgb;
            }
            else
            {
                data.unlockIconName = "";
                data.unlockIconColor = Color.white;
                data.iconColor = new Color(1f, 0.6f, 0.05f);
            }

            return data;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoUnlockChallenge c || c.unlock != unlock;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Getting Arena Unlocks");
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
                unlock = new(unl, "Unlock", 0, listName: "unlocks")
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
                unlock.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                TeamsToString()
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                unlock = SettingBoxFromString(array[0]) as SettingBox<string>;
                completed = (array[1] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                TeamsFromString(array[4]);
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoUnlockChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            IL.Room.Loaded += Room_LoadedUnlock;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_string_bool += MiscProgressionData_GetTokenCollected;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SafariUnlockID += MiscProgressionData_GetTokenCollected_SafariUnlockID;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SlugcatUnlockID += MiscProgressionData_GetTokenCollected_SlugcatUnlockID;
            //tokenColorHook = new(typeof(CollectToken).GetProperty("TokenColor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetGetMethod(), CollectToken_TokenColor_get);
            On.CollectToken.Pop += CollectToken_Pop;
        }

        public override void RemoveHooks()
        {

            IL.Room.Loaded -= Room_LoadedUnlock;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_string_bool -= MiscProgressionData_GetTokenCollected;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SafariUnlockID -= MiscProgressionData_GetTokenCollected_SafariUnlockID;
            On.PlayerProgression.MiscProgressionData.GetTokenCollected_SlugcatUnlockID -= MiscProgressionData_GetTokenCollected_SlugcatUnlockID;
            //tokenColorHook?.Dispose();
            On.CollectToken.Pop -= CollectToken_Pop;
        }

        public override List<object> Settings() => [unlock];
    }
}