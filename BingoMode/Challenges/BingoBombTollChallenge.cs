using Expedition;
using MoreSlugcats;
using System;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoBombTollChallenge : Challenge, IBingoChallenge
    {
        public bool bombed;
        public SettingBox<bool> pass;
        public SettingBox<string> roomName;

        public override void UpdateDescription()
        {
            string region = roomName.Value.Substring(0, 2);
            description = ChallengeTools.IGT.Translate("Throw a grenade at the <toll> scavenger toll" + (pass.Value ? " and pass it" : ""))
                .Replace("<toll>", Region.GetRegionFullName(region, ExpeditionData.slugcatPlayer) + (roomName.Value == "gw_c05" ? " surface" : roomName.Value == "gw_c11" ? " underground" : ""));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoBombTollChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Throwing Grenades at Scavenger Tolls");
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
            if (!completed && roomName.Value == room.ToLowerInvariant())
            {
                Plugin.logger.LogMessage("bombed");
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
            if (!completed && bombed && roomName.Value == room.ToLowerInvariant())
            {
                bombed = false;
                CompleteChallenge();
            }
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
                hidden ? "1" : "0",
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
                hidden = (array[3] == "1");
                revealed = (array[4] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoBombTollChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            On.ScavengerBomb.Explode += ScavengerBomb_Explode;
            On.ScavengerOutpost.PlayerTracker.Update += PlayerTracker_Update2;
        }

        public void RemoveHooks()
        {
            On.ScavengerBomb.Explode -= ScavengerBomb_Explode;
            On.ScavengerOutpost.PlayerTracker.Update -= PlayerTracker_Update2;
        }

        public List<object> Settings() => [pass, roomName];
    }
}