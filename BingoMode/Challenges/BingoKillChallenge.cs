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
using ItemType = AbstractPhysicalObject.AbstractObjectType;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoKillChallenge : Challenge, IBingoChallenge
    {
        public SettingBox<string> crit; // If "any", then means you can kill any creature
        public SettingBox<string> weapon;
        public int current; 
        public SettingBox<int> amount; 
        public SettingBox<string> region; 
        public SettingBox<string> sub; 
        public SettingBox<string> room; 
        public SettingBox<bool> deathPit; 
        public SettingBox<bool> starve; 
        public SettingBox<bool> oneCycle; 

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            string newValue = "Unknown";
            try
            {
                int indexe = new CreatureType(crit.Value).index;
                if (indexe >= 0)
                {
                    newValue = ChallengeTools.IGT.Translate(ChallengeTools.creatureNames[indexe]);
                }
            }
            catch (Exception ex)
            {
                ExpLog.Log("Error getting creature name for BingoKillChallenge | " + ex.Message);
            }
            string location = room.Value != "" ? room.Value : sub.Value != "" ? sub.Value : region.Value != "" ? Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer) : "";
            description = ChallengeTools.IGT.Translate("Kill [<current>/<amount>] <crit><location><pitorweapon><starving><onecycle>")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amount.Value.ToString())
                .Replace("<crit>", crit.Value != "_AnyCreature" ? newValue : "creatures")
                .Replace("<location>", location != "" ? " in " + location : "")
                .Replace("<pitorweapon>", deathPit.Value ? " with a death pit" : weapon.Value != "_AnyWeapon" ? " with " + ChallengeTools.ItemName(new(weapon.Value)) : "")
                .Replace("<starving>", starve.Value ? " while starving" : "")
                .Replace("<onecycle>", oneCycle.Value ? " in one cycle" : "");
            base.UpdateDescription();
        }

        public override Challenge Generate()
        {
            float diff = UnityEngine.Random.value;
            ChallengeTools.ExpeditionCreature expeditionCreature = ChallengeTools.GetExpeditionCreature(ExpeditionData.slugcatPlayer, diff);
            int num = (int)Mathf.Lerp(3f, 15f, (float)Math.Pow(diff, 2.5));
            if (expeditionCreature.points < 7)
            {
                num += UnityEngine.Random.Range(3, 6);
            }
            if (num > expeditionCreature.spawns)
            {
                num = expeditionCreature.spawns;
            }
            if (num > 15)
            {
                num = 15;
            }
            bool onePiece = UnityEngine.Random.value < 0.2f;
            bool starvv = UnityEngine.Random.value < 0.2f;
            if (onePiece || starvv) num = Mathf.CeilToInt(num / 3);
            num = Mathf.Max(1, num);
            var clone = ChallengeUtils.Weapons.ToList();
            clone.RemoveAll(x => x == "PuffBall" || x == "FlareBomb" || x == "Rock");
            string weapo = clone[UnityEngine.Random.Range(0, clone.Count - (ModManager.MSC ? 0 : 1))];
            if ((expeditionCreature.creature == CreatureType.Centipede ||
                expeditionCreature.creature == CreatureType.Centiwing ||
                expeditionCreature.creature == CreatureType.SmallCentipede ||
                expeditionCreature.creature == CreatureType.RedCentipede ||
                expeditionCreature.creature == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti) && UnityEngine.Random.value < 0.3f) weapo = "PuffBall"; 
            else if ((expeditionCreature.creature == CreatureType.Spider ||
                expeditionCreature.creature == CreatureType.BigSpider ||
                expeditionCreature.creature == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider) && UnityEngine.Random.value < 0.3f) weapo = "FlareBomb"; 
            return new BingoKillChallenge
            {
                crit = new(expeditionCreature.creature.value, "Creature Type", 0),
                amount = new(num, "Amount", 1),
                starve = new(starvv, "While Starving", 2),
                oneCycle = new(onePiece, "In one Cycle", 3),
                sub = new("", "Subregion", 4),
                region = new("", "Region", 5),
                room = new("", "Room", 6),
                weapon = new(weapo, "Weapon Used", 7),
                deathPit = new(false, "Via a Death Pit", 8)
            };
        }

        public override void Update()
        {
            base.Update();
            if (oneCycle.Value && game != null && game.cameras.Length > 0 && game.cameras[0].room != null && this.game.cameras[0].room.shelterDoor != null && this.game.cameras[0].room.shelterDoor.IsClosing)
            {
                if (this.current != 0)
                {
                    this.current = 0;
                    this.UpdateDescription();
                }
                return;
            }
        }

        public void DeathPit(Creature c, Player p)
        {
            if (!deathPit.Value || c == null || game == null || !CritInLocation(c)) return;
            if (starve.Value && !p.Malnourished) return;
            string type = c.abstractCreature.creatureTemplate.type.value;
            bool flag = crit == null || type == crit.Value;
            if (!flag && crit.Value == "DaddyLongLegs" && type == "CreatureType.BrotherLongLegs" && (c as DaddyLongLegs).colorClass)
            {
                flag = true;
            }
            if (flag)
            {
                this.current++;
                this.UpdateDescription();
                if (this.current >= this.amount.Value)
                {
                    this.CompleteChallenge();
                }
            }
        }

        public bool CritInLocation(Creature crit)
        {
            string location = room.Value != "" ? room.Value : sub.Value != "" ? sub.Value : region.Value != "" ? Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer) : "boowomp";
            AbstractRoom rom = crit.room.abstractRoom;
            if (location == room.Value)
            {
                return rom.name == location;
            }
            else if (location == sub.Value)
            {
                return rom.subregionName == location || rom.altSubregionName == location;
            }
            else if (location == region.Value)
            {
                return rom.world.region.name == location;
            }
            else return true;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Creature Killing");
        }

        public override int Points()
        {
            int result = 0;
            try
            {
                float num = 1f;
                CreatureTemplate.Type critTarget = new(crit.Value);
                if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint)
                {
                    num = 1.35f;
                }
                if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear && crit.Value == "DaddyLongLegs")
                {
                    critTarget = CreatureTemplate.Type.BrotherLongLegs;
                }
                result = (int)((float)(ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature c) => c.creature == critTarget).points * this.amount.Value) * num) * (int)(this.hidden ? 2f : 1f);
            }
            catch (Exception ex)
            {
                ExpLog.Log("Creature not found: " + ex.Message);
            }
            return result;
        }

        public override void Reset()
        {
            this.current = 0;
            base.Reset();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return true;// challenge is not BingoKillChallenge c;
        }

        public override string ToString()
        {
            Plugin.logger.LogMessage(amount.ToString());
            Plugin.logger.LogMessage(deathPit.ToString());
            return string.Concat(new string[]
            {
                "BingoKillChallenge",
                "~",
                crit.ToString(),
                "><",
                weapon.ToString(),
                "><",
                amount.ToString(),
                "><",
                current.ToString(),
                "><",
                region.ToString(),
                "><",
                sub.ToString(),
                "><",
                room.ToString(),
                "><",
                oneCycle.ToString(),
                "><",
                deathPit.ToString(),
                "><",
                starve.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0"
            });
        }

        public override bool CombatRequired()
        {
            return true;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                crit = SettingBoxFromString(array[0]) as SettingBox<string>;
                weapon = SettingBoxFromString(array[1]) as SettingBox<string>;
                amount = SettingBoxFromString(array[2]) as SettingBox<int>;
                current = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                region = SettingBoxFromString(array[4]) as SettingBox<string>;
                sub = SettingBoxFromString(array[5]) as SettingBox<string>;
                room = SettingBoxFromString(array[6]) as SettingBox<string>;
                oneCycle = SettingBoxFromString(array[7]) as SettingBox<bool>;
                deathPit = SettingBoxFromString(array[8]) as SettingBox<bool>;
                starve = SettingBoxFromString(array[9]) as SettingBox<bool>;
                completed = (array[10] == "1");
                hidden = (array[11] == "1");
                revealed = (array[12] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoKillChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public override bool RespondToCreatureKill()
        {
            return true;
        }

        public override void CreatureKilled(Creature c, int playerNumber)
        {
            if (deathPit.Value || completed || game == null || c == null || !CritInLocation(c) || !CreatureHitByDesired(c)) return;
            if (starve.Value && game.Players != null && game.Players.Count > 0 && game.Players[playerNumber].realizedCreature is Player p && !p.Malnourished) return;
            CreatureType type = c.abstractCreature.creatureTemplate.type;
            bool flag = crit == null || type.value == crit.Value;
            if (!flag && crit.Value == "DaddyLongLegs" && type == CreatureType.BrotherLongLegs && (c as DaddyLongLegs).colorClass)
            {
                flag = true;
            }
            if (flag)
            {
                this.current++;
                ExpLog.Log("Player " + (playerNumber + 1).ToString() + " killed " + type.value);
                this.UpdateDescription();
                if (this.current >= this.amount.Value)
                {
                    this.CompleteChallenge();
                }
            }
        }

        public bool CreatureHitByDesired(Creature c)
        {
            if (BingoData.hitTimeline.TryGetValue(c.abstractCreature.ID, out var list))
            {
                if (list.Last(x => list.IndexOf(x) != -1 && list.IndexOf(x) > (list.Count - 2)) == new ItemType(weapon.Value)) return true;
            }
            return false;
        }

        public void AddHooks()
        {
            IL.Creature.Update += Creature_UpdateIL;
        }

        public void RemoveHooks()
        {
            IL.Creature.Update -= Creature_UpdateIL;
        }

        public List<object> Settings() => [crit, weapon, amount, region, sub, room, oneCycle, deathPit, starve];
    }
}
