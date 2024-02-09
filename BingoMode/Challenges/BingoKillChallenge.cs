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
        public CreatureType crit; // If null, then means you can kill any creature
        public ItemType weapon;
        public int current; //
        public int amount; //
        public string region; //
        public string sub; //
        public string room; //
        public bool deathPit; //
        public bool starve; //
        public bool oneCycle; //

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            string newValue = "Unknown";
            try
            {
                if (crit.Index >= 0)
                {
                    newValue = ChallengeTools.IGT.Translate(ChallengeTools.creatureNames[this.crit.Index]);
                }
            }
            catch (Exception ex)
            {
                ExpLog.Log("Error getting creature name for BingoKillChallenge | " + ex.Message);
            }
            string location = room != "" ? room : sub != "" ? sub : region != "" ? Region.GetRegionFullName(region, ExpeditionData.slugcatPlayer) : "";
            description = ChallengeTools.IGT.Translate("Kill [<current>/<amount>] <crit><location><pitorweapon><starving><onecycle>")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amount.ToString())
                .Replace("<crit>", crit != null ? ChallengeTools.creatureNames[crit.Index] : "creatures")
                .Replace("<location>", location != "" ? " in " + location : "")
                .Replace("<pitorweapon>", deathPit ? " with a death pit" : weapon != null ? " with " + ChallengeTools.ItemName(weapon) : "")
                .Replace("<starving>", starve ? " while starving" : "")
                .Replace("<onecycle>", oneCycle ? " in one cycle" : "");
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
            num = Mathf.Min(1, num);
            bool onePiece = UnityEngine.Random.value < 0.25f;
            if (onePiece) num = Mathf.CeilToInt(num / 3);
            var clone = ChallengeUtils.Weapons.ToList();
            clone.RemoveAll(x => x == ItemType.PuffBall || x == ItemType.FlareBomb || x == ItemType.Rock);
            ItemType weapo = clone[UnityEngine.Random.Range(0, clone.Count - (ModManager.MSC ? 0 : 1))];
            if ((expeditionCreature.creature == CreatureType.Centipede ||
                expeditionCreature.creature == CreatureType.Centiwing ||
                expeditionCreature.creature == CreatureType.SmallCentipede ||
                expeditionCreature.creature == CreatureType.RedCentipede ||
                expeditionCreature.creature == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti) && UnityEngine.Random.value < 0.3f) weapo = ItemType.PuffBall; 
            else if ((expeditionCreature.creature == CreatureType.Spider ||
                expeditionCreature.creature == CreatureType.BigSpider ||
                expeditionCreature.creature == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider) && UnityEngine.Random.value < 0.3f) weapo = ItemType.FlareBomb; 
            return new BingoKillChallenge
            {
                crit = expeditionCreature.creature,
                amount = num,
                starve = UnityEngine.Random.value < 0.25f,
                oneCycle = onePiece,
                sub = "",
                region = "",
                room = "",
                weapon = weapo
            };
        }

        public override void Update()
        {
            base.Update();
            if (oneCycle && game != null && game.cameras.Length > 0 && game.cameras[0].room != null && this.game.cameras[0].room.shelterDoor != null && this.game.cameras[0].room.shelterDoor.IsClosing)
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
            if (!deathPit || c == null || game == null || !CritInLocation(c)) return;
            if (starve && !p.Malnourished) return;
            CreatureType type = c.abstractCreature.creatureTemplate.type;
            bool flag = crit == null || type == crit;
            if (!flag && crit == CreatureType.DaddyLongLegs && type == CreatureType.BrotherLongLegs && (c as DaddyLongLegs).colorClass)
            {
                flag = true;
            }
            if (flag)
            {
                this.current++;
                this.UpdateDescription();
                if (this.current >= this.amount)
                {
                    this.CompleteChallenge();
                }
            }
        }

        public bool CritInLocation(Creature crit)
        {
            string location = room != "" ? room : sub != "" ? sub : region != "" ? Region.GetRegionFullName(region, ExpeditionData.slugcatPlayer) : "boowomp";
            AbstractRoom rom = crit.room.abstractRoom;
            if (location == room)
            {
                return rom.name == location;
            }
            else if (location == sub)
            {
                return rom.subregionName == location || rom.altSubregionName == location;
            }
            else if (location == region)
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
                CreatureTemplate.Type critTarget = this.crit;
                if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint)
                {
                    num = 1.35f;
                }
                if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear && this.crit == CreatureTemplate.Type.DaddyLongLegs)
                {
                    critTarget = CreatureTemplate.Type.BrotherLongLegs;
                }
                result = (int)((float)(ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature c) => c.creature == critTarget).points * this.amount) * num) * (int)(this.hidden ? 2f : 1f);
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
            return string.Concat(new string[]
            {
                "BingoKillChallenge",
                "~",
                crit == null ? "NULL" : ValueConverter.ConvertToString<CreatureTemplate.Type>(this.crit),
                "><",
                ValueConverter.ConvertToString<int>(this.amount),
                "><",
                ValueConverter.ConvertToString<int>(this.current),
                "><",
                this.completed ? "1" : "0",
                "><",
                this.hidden ? "1" : "0",
                "><",
                this.revealed ? "1" : "0"
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
                crit = array[0] == "NULL" ? null : new CreatureType(array[0], false);
                amount = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                completed = (array[3] == "1");
                hidden = (array[4] == "1");
                revealed = (array[5] == "1");
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
            if (deathPit || completed || game == null || c == null || !CritInLocation(c) || !CreatureHitByDesired(c)) return;
            if (starve && game.Players != null && game.Players.Count > 0 && game.Players[playerNumber].realizedCreature is Player p && !p.Malnourished) return;
            CreatureType type = c.abstractCreature.creatureTemplate.type;
            bool flag = crit == null || type == crit;
            if (!flag && crit == CreatureType.DaddyLongLegs && type == CreatureType.BrotherLongLegs && (c as DaddyLongLegs).colorClass)
            {
                flag = true;
            }
            if (flag)
            {
                this.current++;
                ExpLog.Log("Player " + (playerNumber + 1).ToString() + " killed " + type.value);
                this.UpdateDescription();
                if (this.current >= this.amount)
                {
                    this.CompleteChallenge();
                }
            }
        }

        public bool CreatureHitByDesired(Creature c)
        {
            if (BingoData.hitTimeline.TryGetValue(c.abstractCreature.ID, out var list))
            {
                if (list.Last(x => list.IndexOf(x) != -1 && list.IndexOf(x) > (list.Count - 2)) == weapon) return true;
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
    }
}
