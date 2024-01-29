using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoDamageChallenge : Challenge, IBingoChallenge
    {
        public AbstractPhysicalObject.AbstractObjectType weapon;
        public CreatureTemplate.Type victim;
        public int amount;
        public int current;
        // This prevents the same creatures being hit by the same sources multiple times
        public Dictionary<Creature, List<UpdatableAndDeletable>> blacklist = [];

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null && victim != null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            this.description = ChallengeTools.IGT.Translate("Damage <crit> with <weapon> [<current>/<amount>] times")
                .Replace("<crit>", victim == null ? "creatures" : ChallengeTools.creatureNames[victim.Index])
                .Replace("<weapon>", ChallengeTools.ItemName(weapon))
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount));
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
            AbstractPhysicalObject.AbstractObjectType wep = ChallengeUtils.Weapons[UnityEngine.Random.Range(0, ChallengeUtils.Weapons.Length - (ModManager.MSC ? 0 : 1))];

            CreatureTemplate.Type crit;
            if (UnityEngine.Random.value < 0.3f) crit = null;
            else crit = randoe[UnityEngine.Random.Range(0, randoe.Count)].creature;
            int amound = UnityEngine.Random.Range(2, 7);

            return new BingoDamageChallenge
            {
                weapon = wep,
                victim = crit,
                amount = amound,
            };
        }

        public void Hit(AbstractPhysicalObject.AbstractObjectType weaponn, Creature victimm, UpdatableAndDeletable nonPhysicalSource = null)
        {
            if (completed || weaponn == null || victimm == null) return;
            if (weaponn == weapon && (victimm.Template.type == victim))
            {
                if (nonPhysicalSource != null) 
                {
                    if (blacklist.TryGetValue(victimm, out var gruh) && gruh.Contains(nonPhysicalSource)) return;
                    if (!blacklist.ContainsKey(victimm)) blacklist.Add(victimm, []);
                    if (blacklist.TryGetValue(victimm, out var list) && !list.Contains(nonPhysicalSource)) list.Add(nonPhysicalSource);
                }

                current++;
                UpdateDescription();
                if (current >= amount) CompleteChallenge();
            }
        }

        public override void Update()
        {
            base.Update();
            foreach (var kvp in blacklist)
            {
                foreach (var item in kvp.Value)
                {
                    if (item.slatedForDeletetion || item == null) kvp.Value.Remove(item);
                }

                if (kvp.Value.Count == 0) blacklist.Remove(kvp.Key);
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
            return string.Concat(new string[]
            {
                "DmgWithItem",
                "~",
                ValueConverter.ConvertToString(weapon),
                "><",
                victim == null ? "NULL" : ValueConverter.ConvertToString(victim),
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
                weapon = new AbstractPhysicalObject.AbstractObjectType(array[0], false);
                victim = array[0] == "NULL" ? null : new CreatureTemplate.Type(array[1], false);
                current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                completed = (array[4] == "1");
                hidden = (array[5] == "1");
                revealed = (array[6] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: DmgWithItem FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            On.Spear.HitSomething += Spear_HitSomething;
            On.Rock.HitSomething += Rock_HitSomething;
            On.MoreSlugcats.LillyPuck.HitSomething += LillyPuck_HitSomething;
            On.PhysicalObject.HitByExplosion += PhysicalObject_HitByExplosion;
            IL.SporeCloud.Update += SporeCloud_Update;
            On.SporePlant.Bee.Attach += Bee_Attach;
            IL.JellyFish.Collide += JellyFish_Collide;
        }

        public void RemoveHooks()
        {
            On.Spear.HitSomething -= Spear_HitSomething;
            On.Rock.HitSomething -= Rock_HitSomething;
            On.MoreSlugcats.LillyPuck.HitSomething -= LillyPuck_HitSomething;
            On.PhysicalObject.HitByExplosion -= PhysicalObject_HitByExplosion;
            IL.SporeCloud.Update -= SporeCloud_Update;
            On.SporePlant.Bee.Attach -= Bee_Attach;
            IL.JellyFish.Collide -= JellyFish_Collide;
        }
    }
}