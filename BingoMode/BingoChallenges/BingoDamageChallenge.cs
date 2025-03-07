using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoDamageChallenge : BingoChallenge
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
                .Replace("<crit>", victim.Value == "Any Creature" ? "creatures" : ChallengeTools.creatureNames[new CreatureType(victim.Value, false).Index])
                .Replace("<weapon>", ChallengeTools.ItemName(new(weapon.Value)))
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new Phrase([new Icon("bingoimpact", 1f, UnityEngine.Color.white)], []);
            if (weapon.Value != "Any Weapon") phrase.words.Insert(0, new Icon(ChallengeUtils.ItemOrCreatureIconName(weapon.Value), 1f, ChallengeUtils.ItemOrCreatureIconColor(weapon.Value)));
            if (victim.Value != "Any Creature") phrase.words.Add(new Icon(ChallengeUtils.ItemOrCreatureIconName(victim.Value), 1f, ChallengeUtils.ItemOrCreatureIconColor(victim.Value)));
            phrase.words.Add(new Counter(current, amount.Value));
            phrase.newLines = [phrase.words.Count - 1];
            return phrase;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoDamageChallenge c || (c.weapon.Value != weapon.Value && c.victim.Value != victim.Value);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Hitting creatures with items");
        }

        public override Challenge Generate()
        {
            List<ChallengeTools.ExpeditionCreature> randoe = ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value];
            string wep = ChallengeUtils.Weapons[UnityEngine.Random.Range(1, ChallengeUtils.Weapons.Length - (ModManager.MSC ? 0 : 1))];

            string crit;
            if (UnityEngine.Random.value < 0.25f)
            {
                crit = "Any Creature";
                if (wep == "Any Weapon") wep = ChallengeUtils.Weapons[UnityEngine.Random.Range(1, ChallengeUtils.Weapons.Length - (ModManager.MSC ? 0 : 1))];
            }
            else crit = randoe[UnityEngine.Random.Range(0, randoe.Count)].creature.value;
            int amound = UnityEngine.Random.Range(2, 7);

            return new BingoDamageChallenge
            {
                weapon = new(wep, "Weapon", 0, listName: "weapons"),
                victim = new(crit, "Creature Type", 1, listName: "creatures"),
                amount = new(amound, "Amount", 2),
            };
        }

        public void Hit(AbstractPhysicalObject.AbstractObjectType weaponn, Creature victimm)
        {
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden) return;
            
            bool glug = false;
            bool weaponCheck = false;
            if (victimm.Template.type.value.ToLowerInvariant() == victim.Value.ToLowerInvariant()) glug = true;
            if (victim.Value == "Any Creature" && victimm is not Player) glug = true;
            if (weaponn.value.ToLowerInvariant() == weapon.Value.ToLowerInvariant()) weaponCheck = true;
            if (weapon.Value == "Any Weapon") weaponCheck = true;

            if (weaponCheck && glug)
            {
                current++;
                UpdateDescription();
                if (current >= amount.Value) CompleteChallenge();
                else ChangeValue();
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
                revealed ? "1" : "0",
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
                revealed = (array[5] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoDamageChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void Reset()
        {
            base.Reset();
            current = 0;
        }

        public override void AddHooks()
        {
        }

        public override void RemoveHooks()
        {
        }

        public override List<object> Settings() => [weapon, victim, amount];
    }
}