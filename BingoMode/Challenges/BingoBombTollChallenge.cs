using Expedition;
using MoreSlugcats;
using System;
using System.Text.RegularExpressions;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoBombTollChallenge : Challenge, IBingoChallenge
    {
        public bool bombed;
        public bool pass;
        public string roomName;

        public override void UpdateDescription()
        {
            string region = roomName.Substring(0, 2);
            description = ChallengeTools.IGT.Translate("Throw a grenade at the <toll> scavenger toll" + (pass ? " and pass it" : ""))
                .Replace("<toll>", Region.GetRegionFullName(region, ExpeditionData.slugcatPlayer) + (roomName == "gw_c05" ? " surface" : roomName == "gw_c11" ? " underground" : ""));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoBombTollChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Bombing Tolls");
        }

        public override Challenge Generate()
        {
            string toll = ChallengeUtils.BombableOutposts[UnityEngine.Random.Range(0, ChallengeUtils.BombableOutposts.Length - (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint ? 0 : 1))];

            return new BingoBombTollChallenge
            {
                pass = false,
                roomName = toll
            };
        }

        public void Boom(string room)
        {
            if (!completed && roomName == room.ToLowerInvariant())
            {
                Plugin.logger.LogMessage("bombed");
                if (!pass)
                {
                    CompleteChallenge();
                    return;
                }
                bombed = true;
            }
        }

        public void Pass(string room)
        {
            if (!completed && bombed && roomName == room.ToLowerInvariant())
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
                roomName,
                "><",
                pass ? "1" : "0",
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
                roomName = array[0];
                pass = (array[1] == "1");
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
    }
}