using Expedition;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
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
            else if (SlugcatStats.Name.values.entries.Contains(unlockName) || unlockName == "Spearmaster")
            {
                data.unlockIconName = "Kill_Slugcat";
                data.unlockIconColor = PlayerGraphics.SlugcatColor(new SlugcatStats.Name(unlockName == "Spearmaster" ? "Spear" : unlockName, false));
                data.iconColor = CollectToken.GreenColor.rgb;
            }
            else if (unlockName.EndsWith("-safari"))
            {
                data.unlockIconName = "";
                data.unlockIconColor = Color.white;
                data.iconColor = CollectToken.RedColor.rgb;
            }
            else if (unlockName == "FireSpear")
            {
                data.unlockIconName = "Symbol_FireSpear";
                data.unlockIconColor = new Color(0.9019608f, 0.05490196f, 0.05490196f);
                data.iconColor = RainWorld.AntiGold.rgb;
            }
            else if (unlockName == "Pearl")
            {
                data.unlockIconName = "Symbol_Pearl";
                data.unlockIconColor = new Color(0.7f, 0.7f, 0.7f);
                data.iconColor = RainWorld.AntiGold.rgb;
            }
            else if (unlockName == "BigCentipede")
            {
                data.unlockIconName = "Kill_Centipede3";
                data.unlockIconColor = new Color(0.7f, 0.7f, 0.7f);
                data.iconColor = RainWorld.AntiGold.rgb;
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
            return challenge is not BingoUnlockChallenge c || c.unlock.Value != unlock.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Getting arena unlocks");
        }

        public override Challenge Generate()
        {
            gibacj:
            int type = UnityEngine.Random.Range(0, ModManager.MSC ? (SlugcatStats.IsSlugcatFromMSC(ExpeditionData.slugcatPlayer) ? 4 : 3) : 2);
            string unl = "ERROR";

            for (int i = 0; i < BingoData.possibleTokens.Length; i++)
            {
                Plugin.logger.LogInfo(BingoData.possibleTokens[i].Count);
            }
            unl = BingoData.possibleTokens[type][UnityEngine.Random.Range(0, BingoData.possibleTokens[type].Count)];
            if (unl.ToLowerInvariant().StartsWith("ms"))
            {
                if (ExpeditionData.slugcatPlayer.value == "Rivulet" || ExpeditionData.slugcatPlayer.value == "Saint") { }
                else goto gibacj;
            }
            if (unl.ToLowerInvariant().StartsWith("ds"))
            {
                if (ExpeditionData.slugcatPlayer.value == "Saint") goto gibacj;
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
                revealed ? "1" : "0",
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                unlock = SettingBoxFromString(array[0]) as SettingBox<string>;
                completed = (array[1] == "1");
                revealed = (array[2] == "1");
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