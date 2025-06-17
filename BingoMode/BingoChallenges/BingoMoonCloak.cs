using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoMoonCloak : BingoChallenge
    {
        public SettingBox<bool> deliver;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate(!deliver.Value ? "Grab Moon's Cloak" : "Deliver the Cloak to Moon");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            if (!deliver.Value)
            {
                return new Phrase([new Icon("Symbol_MoonCloak", 1f, new Color(0.8f, 0.8f, 0.8f))], []);
            }
            else
            {
                return new Phrase([
                new Icon("Symbol_MoonCloak", 1f, new Color(0.8f, 0.8f, 0.8f)),
                new Icon("singlearrow", 1f, Color.green),
                new Icon("GuidanceMoon" , 1f, new Color(1f, 0.8f, 0.3f))
                ], []);
            }
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoMoonCloak c || (c.deliver.Value != deliver.Value);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Moon's Cloak");
        }

        public override Challenge Generate()
        {
            BingoMoonCloak ch = new BingoMoonCloak
            {
                deliver = new(UnityEngine.Random.value < 0.5f, "Deliver", 0)
            };

            return ch;
        }

        public void Delivered()
        {
            if (!completed && !revealed && !TeamsCompleted[SteamTest.team] && !hidden && deliver.Value)
            {
                CompleteChallenge();
            }
        }

        public void Cloak()
        {
            if (!completed || !revealed || !TeamsCompleted[SteamTest.team] || !hidden || !deliver.Value)
            {
                CompleteChallenge();
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
            return slugcat == SlugcatStats.Name.Red || slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand || slugcat == SlugcatStats.Name.White || slugcat == SlugcatStats.Name.Yellow;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoMoonCloak",
                "~",
                deliver.ToString(),
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
                deliver = SettingBoxFromString(array[0]) as SettingBox<bool>;
                completed = (array[1] == "1");
                revealed = (array[2] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoMoonCloak FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.Player.SlugcatGrab += Player_SlugcatGrabCloak;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += SLOracleBehavior_GrabCloak;
            if (deliver.Value) IL.Room.Loaded += Room_LoadedMoonCloak;
            //On.SaveState.ctor += SaveState_ctorCloak;
        }

        public override void RemoveHooks()
        {
            On.Player.SlugcatGrab -= Player_SlugcatGrabCloak;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents -= SLOracleBehavior_GrabCloak;
            if (deliver.Value) IL.Room.Loaded -= Room_LoadedMoonCloak;
            //On.SaveState.ctor -= SaveState_ctorCloak;
        }

        public override List<object> Settings() => [deliver];
    }
}
