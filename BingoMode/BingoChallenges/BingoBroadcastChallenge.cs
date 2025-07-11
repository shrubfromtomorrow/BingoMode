using Expedition;
using Menu.Remix;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoBroadcastChallenge : BingoChallenge
    {
        public SettingBox<string> chatlog;
        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Collect the <chatlog> broadcast")
                .Replace("<chatlog>", ValueConverter.ConvertToString(chatlog.Value));

            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase(
                [[new Icon("Symbol_Satellite")],
                [new Verse(chatlog.Value)]]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoBroadcastChallenge c || c.chatlog.Value != chatlog.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Collecting broadcasts");
        }

        public override Challenge Generate()
        {
            return new BingoBroadcastChallenge
            {
                chatlog = new(BingoData.possibleTokens[4][UnityEngine.Random.Range(0, BingoData.possibleTokens[4].Count)], "Broadcast", 0, listName: "chatlogs"),
            };
        }

        public override bool RequireSave() => true;
        public override bool ReverseChallenge() => false;

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
                "BingoBroadcastChallenge",
                "~",
                chatlog.ToString(),
                "><",
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
                chatlog = SettingBoxFromString(array[0]) as SettingBox<string>;
                completed = (array[1] == "1");
                revealed = (array[2] == "1");

                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoBroadcastChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            IL.Room.Loaded += Room_LoadedUnlock;
            On.PlayerProgression.MiscProgressionData.GetBroadcastListened += MiscProgressionData_GetBroadcastListened;
            On.CollectToken.Pop += CollectToken_Pop;
        }

        public override void RemoveHooks()
        {
            IL.Room.Loaded -= Room_LoadedUnlock;
            On.PlayerProgression.MiscProgressionData.GetBroadcastListened -= MiscProgressionData_GetBroadcastListened;
            On.CollectToken.Pop -= CollectToken_Pop;
        }

        public override List<object> Settings()
        {
            return new List<object> { chatlog };
        }
    }
}