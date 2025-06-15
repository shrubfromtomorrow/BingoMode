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
        public SettingBox<bool> retreive;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate(retreive.Value ? "Obtain Moon's Cloak" : "Deliver the Cloak to Moon");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            if (retreive.Value)
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
            return challenge is not BingoMoonCloak c || (c.retreive.Value != retreive.Value);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Moon's Cloak");
        }

        public override Challenge Generate()
        {
            BingoMoonCloak ch = new BingoMoonCloak
            {
                retreive = new(UnityEngine.Random.value < 0.5f, "retreive", 0)
            };

            return ch;
        }

        public void Delivered()
        {
            if (!completed && !revealed && !TeamsCompleted[SteamTest.team] && !hidden && !retreive.Value)
            {
                CompleteChallenge();
            }
        }

        public void Cloak()
        {
            if (!completed || !revealed || !TeamsCompleted[SteamTest.team] || !hidden || retreive.Value)
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
            //return (slugcat == SlugcatStats.Name.Red || slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand) || slugcat == SlugcatStats.Name.White || slugcat == SlugcatStats.Name.Yellow;
            return slugcat != MoreSlugcatsEnums.SlugcatStatsName.Spear || slugcat != MoreSlugcatsEnums.SlugcatStatsName.Artificer;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoMoonCloak",
                "~",
                retreive.ToString(),
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
                retreive = SettingBoxFromString(array[0]) as SettingBox<bool>;
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
            //IL.Player.GrabUpdate += Player_GrabUpdateCloak;
            On.Player.SlugcatGrab += Player_SlugcatGrabCloak;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += SLOracleBehavior_GrabCloak;
            if (!retreive.Value) IL.Room.Loaded += Room_LoadedMoonCloak;
            //On.SaveState.ctor += SaveState_ctorCloak;
        }

        public override void RemoveHooks()
        {
            //IL.Player.GrabUpdate -= Player_GrabUpdateCloak;
            On.Player.SlugcatGrab -= Player_SlugcatGrabCloak;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents -= SLOracleBehavior_GrabCloak;
            if (!retreive.Value) IL.Room.Loaded -= Room_LoadedMoonCloak;
            //On.SaveState.ctor -= SaveState_ctorCloak;
        }

        public override List<object> Settings() => [retreive];
    }
}
