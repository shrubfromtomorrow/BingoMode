using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoPopcornChallenge : Challenge, IBingoChallenge
    {
        public int current;
        public SettingBox<int> amound;
        public int Index { get; set; }
        public bool Locked { get; set; }
        public bool Failed { get; set; }

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Open [<current>/<amount>] popcorn plants")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amound.Value.ToString());
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoPopcornChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Making Popcorn");
        }

        public override Challenge Generate()
        {
            BingoPopcornChallenge ch = new();
            ch.amound = new(UnityEngine.Random.Range(3, 8), "Amount", 0);
            return ch;
        }

        public void Pop()
        {
            if (!completed)
            {
                current++;
                UpdateDescription();
                if (current >= (int)amound.Value) CompleteChallenge();
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

        public override void Reset()
        {
            base.Reset();
            current = 0;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat != MoreSlugcatsEnums.SlugcatStatsName.Saint;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoPopcornChallenge",
                "~",
                current.ToString(),
                "><",
                amound.ToString(),
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
                current = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                amound = SettingBoxFromString(array[1]) as SettingBox<int>;
                completed = (array[2] == "1");
                hidden = (array[3] == "1");
                revealed = (array[4] == "1");
                UpdateDescription();
                Plugin.logger.LogMessage(description);
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoPopcornChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            IL.SeedCob.HitByWeapon += SeedCob_HitByWeapon;
        }

        public void RemoveHooks()
        {
            IL.SeedCob.HitByWeapon -= SeedCob_HitByWeapon;
        }

        public List<object> Settings() => [amound];
    }
}