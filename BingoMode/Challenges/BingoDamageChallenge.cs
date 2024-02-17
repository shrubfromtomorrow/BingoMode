using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoDamageChallenge : Challenge, IBingoChallenge
    {
        public SettingBox<string> weapon;
        public SettingBox<string> victim;
        public SettingBox<int> amount;
        public int current;

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null && victim != null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            this.description = ChallengeTools.IGT.Translate("Hit <crit> with <weapon> [<current>/<amount>] times")
                .Replace("<crit>", victim.Value == "_AnyCreature" ? "creatures" : ChallengeTools.creatureNames[new CreatureType(victim.Value).Index])
                .Replace("<weapon>", ChallengeTools.ItemName(new(weapon.Value)))
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoDamageChallenge || (challenge as BingoDamageChallenge).weapon != weapon || (challenge as BingoDamageChallenge).victim != victim;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Creature Hitting");
        }

        public override Challenge Generate()
        {
            // Eventually make this generate better with weighted numbers based on the creature and items
            List<ChallengeTools.ExpeditionCreature> randoe = ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value];
            string wep = ChallengeUtils.Weapons[UnityEngine.Random.Range(0, ChallengeUtils.Weapons.Length - (ModManager.MSC ? 0 : 1))];

            string crit;
            if (UnityEngine.Random.value < 0.3f) crit = "_AnyCreature";
            else crit = randoe[UnityEngine.Random.Range(0, randoe.Count)].creature.value;
            int amound = UnityEngine.Random.Range(2, 7);

            return new BingoDamageChallenge
            {
                weapon = new(wep, "Weapon", 0),
                victim = new(crit, "Creature Type", 1),
                amount = new(amound, "Amount", 2),
            };
        }

        public void Hit(AbstractPhysicalObject.AbstractObjectType weaponn, Creature victimm)
        {
            bool glug = false;
            if (victimm.Template.type.value == victim.Value) glug = true;
            if (victim.Value == "_AnyCreature" && victimm is not Player) glug = true;

            if (weaponn.value == weapon.Value && glug)
            {
                current++;
                UpdateDescription();
                if (current >= amount.Value) CompleteChallenge();
            }
        }

        //public override void Update()
        //{
        //    base.Update();
        //    ss:
        //    foreach (var kvp in BingoData.blacklist)
        //    {
        //        s:
        //        foreach (var item in kvp.Value)
        //        {
        //            if (item == null) { kvp.Value.Remove(item); goto s; }
        //        }
        //
        //        if (kvp.Value.Count == 0) { BingoData.blacklist.Remove(kvp.Key); goto ss; }
        //    }
        //}

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
            return string.Concat(new string[]
            {
                "BingoDamageChallenge",
                "~",
                weapon.ToString(),
                "><",
                victim.ToString(),
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
                weapon = SettingBoxFromString(array[0]) as SettingBox<string>;
                victim = SettingBoxFromString(array[1]) as SettingBox<string>;
                current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[3]) as SettingBox<int>;
                completed = (array[4] == "1");
                hidden = (array[5] == "1");
                revealed = (array[6] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoDamageChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
        }

        public void RemoveHooks()
        {
        }

        public List<object> Settings() => [weapon, victim, amount];
    }
}