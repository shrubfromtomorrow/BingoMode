using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoDamageChallenge : BingoChallenge
    {
        public SettingBox<string> weapon;
        public SettingBox<string> victim;
        public SettingBox<int> amount;
        public SettingBox<bool> inOneCycle;
        public SettingBox<string> region;
        public int current;

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null && victim != null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            string location = region.Value != "Any Region" ? Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer) : "";
            this.description = ChallengeTools.IGT.Translate("Hit <crit> with <weapon> [<current>/<amount>] times<location>" + (inOneCycle.Value ? " in one cycle" : ""))
                .Replace("<crit>", victim.Value == "Any Creature" ? "creatures" : ChallengeTools.creatureNames[new CreatureType(victim.Value, false).Index])
                .Replace("<location>", location != "" ? " in " + location : "")
                .Replace("<weapon>", ChallengeTools.ItemName(new(weapon.Value)))
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new([[new Icon("bingoimpact")]]);
            if (weapon.Value != "Any Weapon") phrase.InsertWord(Icon.FromEntityName(weapon.Value), 0, 0);
            if (victim.Value != "Any Creature") phrase.InsertWord(Icon.FromEntityName(victim.Value));

            int lastLine = 1;
            if (region.Value != "Any Region")
            {
                phrase.InsertWord(new Verse(region.Value), 1);
                lastLine = 2;
            }

            phrase.InsertWord(new Counter(current, amount.Value), lastLine);
            if (inOneCycle.Value) phrase.InsertWord(new Icon("cycle_limit"), lastLine);
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
            bool oneCycle = UnityEngine.Random.value < 0.33f;
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
                inOneCycle = new(oneCycle, "In One Cycle", 3),
                region = new("Any Region", "Region", 5, listName: "regions"),
            };
        }

        public bool CritInLocation(Creature crit)
        {
            //                room.Value != "" ? room.Value : 
            string location = region.Value != "Any Region" ? region.Value : "boowomp";
            AbstractRoom room = crit.room.abstractRoom;
            /*if (location == room.Value)
            {
                return rom.name == location;
            }
            else*/
            if (location.ToLowerInvariant() == region.Value.ToLowerInvariant())
            {
                return room.world.region.name.ToLowerInvariant() == location.ToLowerInvariant();
            }
            else return true;
        }

        public void Hit(AbstractPhysicalObject.AbstractObjectType weaponn, Creature victimm)
        {
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden || !CritInLocation(victimm) || (victim.Value == "Any Creature" && victimm.Template.smallCreature)) return;

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

        public override void Update()
        {
            base.Update();
            if (revealed || completed) return;
            if (this.game.cameras[0].room.shelterDoor != null && this.game.cameras[0].room.shelterDoor.IsClosing)
            {
                if (this.current != 0 && this.inOneCycle.Value)
                {
                    this.current = 0;
                    this.UpdateDescription();
                    ChangeValue();
                }
                return;
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
                inOneCycle.ToString(),
                "><",
                region.ToString(),
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
                // 1.2
                if (array.Length == 8)
                {
                    weapon = SettingBoxFromString(array[0]) as SettingBox<string>;
                    victim = SettingBoxFromString(array[1]) as SettingBox<string>;
                    amount = SettingBoxFromString(array[3]) as SettingBox<int>;
                    inOneCycle = SettingBoxFromString(array[4]) as SettingBox<bool>;
                    region = SettingBoxFromString(array[5]) as SettingBox<string>;
                    completed = (array[6] == "1");
                    revealed = (array[7] == "1");
                    current = (inOneCycle.Value && !completed) ? 0 : int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                // 1.1
                else if (array.Length == 9)
                {
                    weapon = SettingBoxFromString(array[0]) as SettingBox<string>;
                    victim = SettingBoxFromString(array[1]) as SettingBox<string>;
                    amount = SettingBoxFromString(array[3]) as SettingBox<int>;
                    inOneCycle = SettingBoxFromString(array[4]) as SettingBox<bool>;
                    region = SettingBoxFromString(array[5]) as SettingBox<string>;
                    completed = (array[7] == "1");
                    revealed = (array[8] == "1");
                    current = (inOneCycle.Value && !completed) ? 0 : int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                }
                // Pre 1.1
                else if (array.Length == 6)
                {
                    weapon = SettingBoxFromString(array[0]) as SettingBox<string>;
                    victim = SettingBoxFromString(array[1]) as SettingBox<string>;
                    current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                    amount = SettingBoxFromString(array[3]) as SettingBox<int>;
                    completed = (array[4] == "1");
                    revealed = (array[5] == "1");
                    inOneCycle = SettingBoxFromString("System.Boolean|false|In One Cycle|3|NULL") as SettingBox<bool>;
                    region = SettingBoxFromString("System.String|Any Region|Region|5|regions") as SettingBox<string>;
                }
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

        public override List<object> Settings() => [weapon, victim, amount, inOneCycle, region];
    }
}