using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using CreatureType = CreatureTemplate.Type;
using System.Runtime.CompilerServices;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoCreatureGateChallenge : Challenge, IBingoChallenge
    {
        public SettingBox<int> amount;
        public int current;
        public SettingBox<string> crit;
        public List<string> gates = [];

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            this.description = ChallengeTools.IGT.Translate("Carry a <crit> for [<current>/<amount>] gates")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value))
                .Replace("<crit>", ChallengeTools.creatureNames[new CreatureType(crit.Value).Index].TrimEnd('s'));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoCreatureGateChallenge;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Carrying a Creature Through Gates");
        }

        public override Challenge Generate()
        {
            return new BingoCreatureGateChallenge
            {
                amount = new(UnityEngine.Random.Range(2, 5), "Amount", 0),
                crit = new(ChallengeUtils.Transportable[UnityEngine.Random.Range(0, ChallengeUtils.Transportable.Length - (ModManager.MSC ? 0 : 1))], "Creature Type", 1, listName: "transport")
            };
        }

        public void Gate(string roomName)
        {
            bool g = false;
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null && game.Players[i].realizedCreature is Player player && player.room != null && player.grasps != null && player.grasps.Any(x => x != null && x.grabbed is Creature c && c.Template.type.value == crit.Value))
                {
                    g = true;
                    break;
                }
            }

            if (g && !completed && !gates.Contains(roomName))
            {
                gates.Add(roomName);
                current++;
                UpdateDescription();

                if (current >= amount.Value) CompleteChallenge();
            }
        }

        public override void Reset()
        {
            base.Reset();

            gates = [];
            current = 0;
        }

        public override int Points()
        {
            return amount.Value * 10;
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
                "BingoCreatureGateChallenge",
                "~",
                crit.ToString(),
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
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
                crit = SettingBoxFromString(array[0]) as SettingBox<string>;
                current = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[2]) as SettingBox<int>;
                completed = (array[3] == "1");
                hidden = (array[4] == "1");
                revealed = (array[5] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoCreatureGateChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            On.RegionGate.NewWorldLoaded += RegionGate_NewWorldLoaded1;
        }

        public void RemoveHooks()
        {
            On.RegionGate.NewWorldLoaded -= RegionGate_NewWorldLoaded1;
        }

        public List<object> Settings() => [amount, crit];
    }
}