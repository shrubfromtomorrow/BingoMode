using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class BingoBombTollRandomizer : Randomizer<Challenge>
    {
        public Randomizer<bool> pass;
        public Randomizer<string> roomName;

        public override Challenge Random()
        {
            BingoBombTollChallenge challenge = new();
            challenge.pass.Value = pass.Random();
            challenge.roomName.Value = roomName.Random();
            return challenge;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}pass-{pass.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}roomName-{roomName.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "BombToll").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            MatchCollection matches = Regex.Matches(serialized, SUBRANDOMIZER_PATTERN);
            pass = Randomizer<bool>.InitDeserialize(matches[0].ToString());
            roomName = Randomizer<string>.InitDeserialize(matches[1].ToString());
        }
    }

    public class BingoBombTollChallenge : BingoChallenge
    {
        public bool bombed;
        public SettingBox<bool> pass;
        public SettingBox<string> roomName;

        public BingoBombTollChallenge()
        {
            pass = new(false, "Pass the Toll", 0);
            roomName = new("", "Scavenger Toll", 1, listName: "tolls");
        }

        public override void UpdateDescription()
        {
            string region = roomName.Value.Substring(0, 2);
            description = ChallengeTools.IGT.Translate("Throw a grenade at the <toll> scavenger toll" + (pass.Value ? " then pass it" : ""))
                .Replace("<toll>", Region.GetRegionFullName(region, ExpeditionData.slugcatPlayer) + (roomName.Value == "gw_c05" ? " surface" : roomName.Value == "gw_c11" ? " underground" : ""));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new(
                [[Icon.FromEntityName("ScavengerBomb"), Icon.SCAV_TOLL],
                [new Verse(roomName.Value.ToUpperInvariant())]]);
            if (pass.Value) phrase.InsertWord(new Icon("singlearrow"));
            return phrase;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoBombTollChallenge c;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Throwing grenades at scavenger tolls");
        }

        public override Challenge Generate()
        {
            string toll = ChallengeUtils.BombableOutposts[UnityEngine.Random.Range(0, ChallengeUtils.BombableOutposts.Length - (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint ? 0 : 1))];

            return new BingoBombTollChallenge
            {
                pass = new(UnityEngine.Random.value < 0.5f, "Pass the Toll", 0),
                roomName = new(toll, "Scavenger Toll", 1, listName: "tolls")
            };
        }

        public void Boom(string room)
        {
            if (!completed && !revealed && !TeamsCompleted[SteamTest.team] && !hidden && roomName.Value == room.ToLowerInvariant())
            {
                
                if (!pass.Value)
                {
                    CompleteChallenge();
                    return;
                }
                bombed = true;
            }
        }

        public void Pass(string room)
        {
            if (!completed && !revealed && !hidden && !TeamsCompleted[SteamTest.team] && bombed && roomName.Value == room.ToLowerInvariant())
            {
                bombed = false;
                CompleteChallenge();
            }
        }

        public override void Reset()
        {
            base.Reset();
            bombed = false;
        }

        public override int Points()
        {
            return 20;
        }

        public override bool CombatRequired()
        {
            return true;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public override string ToString()
        {
            return string.Concat(
            [
                "BingoBombTollChallenge",
                "~",
                roomName.ToString(),
                "><",
                pass.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0"
            ]);
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                roomName = SettingBoxFromString(array[0]) as SettingBox<string>;
                pass = SettingBoxFromString(array[1]) as SettingBox<bool>;
                completed = (array[2] == "1");
                revealed = (array[3] == "1");
                bombed = false;
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoBombTollChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            On.ScavengerBomb.Explode += ScavengerBomb_Explode;
            On.ScavengerOutpost.PlayerTracker.Update += PlayerTracker_Update2;
        }

        public override void RemoveHooks()
        {
            On.ScavengerBomb.Explode -= ScavengerBomb_Explode;
            On.ScavengerOutpost.PlayerTracker.Update -= PlayerTracker_Update2;
        }

        public override List<object> Settings() => [pass, roomName];
    }
}