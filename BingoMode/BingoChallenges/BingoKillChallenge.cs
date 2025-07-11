﻿using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using CreatureType = CreatureTemplate.Type;
using ItemType = AbstractPhysicalObject.AbstractObjectType;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoKillChallenge : BingoChallenge
    {
        public SettingBox<string> crit;
        public SettingBox<string> weapon;
        public int current; 
        public SettingBox<int> amount; 
        public SettingBox<string> region; 
        public SettingBox<string> sub; 
        //public SettingBox<string> room; 
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
            //                room.Value != "" ? room.Value : 
            string location = sub.Value != "Any Subregion" ? sub.Value : region.Value != "Any Region" ? Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer) : "";
            description = ChallengeTools.IGT.Translate("Kill [<current>/<amount>] <crit><location><pitorweapon><starving><onecycle>")
                .Replace("<current>", current.ToString())
                .Replace("<amount>", amount.Value.ToString())
                .Replace("<crit>", crit.Value != "Any Creature" ? newValue : "creatures")
                .Replace("<location>", location != "" ? " in " + location : "")
                .Replace("<pitorweapon>", deathPit.Value ? " with a death pit" : weapon.Value != "Any Weapon" ? " with " + ChallengeTools.ItemName(new(weapon.Value)) : "")
                .Replace("<starving>", starve.Value ? " while starving" : "")
                .Replace("<onecycle>", oneCycle.Value ? " in one cycle" : "");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new([[new Icon("Multiplayer_Bones")]]);
            if (deathPit.Value) phrase.InsertWord(new Icon("deathpiticon"), 0, 0);
            else if (weapon.Value != "Any Weapon") phrase.InsertWord(Icon.FromEntityName(weapon.Value), 0, 0);
            if (crit.Value != "Any Creature") phrase.InsertWord(Icon.FromEntityName(crit.Value));

            int lastLine = 1;
            if (sub.Value != "Any Subregion" || region.Value != "Any Region")
            {
                phrase.InsertWord(new Verse(sub.Value != "Any Subregion" ? sub.Value : region.Value), 1);
                lastLine = 2;
            }

            phrase.InsertWord(new Counter(current, amount.Value), lastLine);
            if (starve.Value) phrase.InsertWord(new Icon("Multiplayer_Death"), lastLine);
            if (oneCycle.Value) phrase.InsertWord(new Icon("cycle_limit"), lastLine);
            return phrase;
        }

        public override Challenge Generate()
        {
            float diff = UnityEngine.Random.value;
            ChallengeTools.ExpeditionCreature expeditionCreature = ChallengeTools.GetExpeditionCreature(ExpeditionData.slugcatPlayer, diff);
            int num = (int)Mathf.Lerp(1f, 10f, (float)Math.Pow(diff, 2.5));
            if (expeditionCreature.points < 7)
            {
                num += UnityEngine.Random.Range(2, 4);
            }
            if (num > expeditionCreature.spawns)
            {
                num = expeditionCreature.spawns;
            }
            if (num > 12)
            {
                num = 12;
            }
            bool onePiece = UnityEngine.Random.value < 0.2f;
            bool starvv = UnityEngine.Random.value < 0.1f;
            if (onePiece || starvv) num = Mathf.CeilToInt(num / 2);
            num = Mathf.Max(2, num);
            List<string> clone = ChallengeUtils.Weapons.ToList();
            clone.RemoveAll(x => x == "PuffBall" || x == "Rock" || x == "JellyFish");
            bool doWeapon = UnityEngine.Random.value < 0.5f;
            bool doCreature = !doWeapon || UnityEngine.Random.value < 0.8f;
            string weapo = doWeapon ? "Any Weapon" : clone[UnityEngine.Random.Range(0, clone.Count - (ModManager.MSC ? 0 : 1))];
            if ((expeditionCreature.creature == CreatureType.Centipede ||
                expeditionCreature.creature == CreatureType.Centiwing ||
                expeditionCreature.creature == CreatureType.SmallCentipede ||
                expeditionCreature.creature == CreatureType.RedCentipede ||
                expeditionCreature.creature == DLCSharedEnums.CreatureTemplateType.AquaCenti) && UnityEngine.Random.value < 0.3f) weapo = "PuffBall";
            return new BingoKillChallenge
            {
                crit = new(doCreature ? expeditionCreature.creature.value : "Any Creature", "Creature Type", 0, listName: "creatures"),
                amount = new(num, "Amount", 1),
                starve = new(starvv, "While Starving", 2),
                oneCycle = new(onePiece, "In one Cycle", 3),
                sub = new("Any Subregion", "Subregion", 4, listName: "subregions"),
                region = new("Any Region", "Region", 5, listName: "regions"),
                weapon = new(weapo, "Weapon Used", 6, listName: "weaponsnojelly"),
                deathPit = new(false, "Via a Death Pit", 7)
            };
        }

        public override void Update()
        {
            base.Update();
            if (!completed && oneCycle.Value && game != null && game.cameras.Length > 0 && game.cameras[0].room != null && game.cameras[0].room.shelterDoor != null && game.cameras[0].room.shelterDoor.IsClosing)
            {
                if (current != 0)
                {
                    current = 0;
                    UpdateDescription();
                }
                return;
            }
        }

        public void DeathPit(Creature c, Player p)
        {
            if (c.Template.smallCreature || !deathPit.Value || TeamsCompleted[SteamTest.team] || hidden || completed || game == null || c == null || revealed || !CritInLocation(c)) return;
            if (starve.Value && !p.Malnourished) return;
            string type = c.abstractCreature.creatureTemplate.type.value;
            bool flag = crit != null && (
                crit.Value == "Any Creature" ||
                type == crit.Value ||
                (crit.Value == "DaddyLongLegs" && type == "CreatureType.BrotherLongLegs" && c is DaddyLongLegs dll && dll.colorClass)
            );
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
                else ChangeValue();
            }
        }

        public bool CritInLocation(Creature crit)
        {
            string location = sub.Value != "Any Subregion" ? sub.Value : region.Value != "Any Region" ? region.Value : "boowomp";
            AbstractRoom room = crit.room.abstractRoom;
            if (location.ToLowerInvariant() == sub.Value.ToLowerInvariant())
            {
                return room.subregionName.ToLowerInvariant() == location.ToLowerInvariant() || room.altSubregionName.ToLowerInvariant() == location.ToLowerInvariant();
            }
            else if (location.ToLowerInvariant() == region.Value.ToLowerInvariant())
            {
                return room.world.region.name.ToLowerInvariant() == location.ToLowerInvariant();
            }
            else return true;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Killing creatures");
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
            current = 0;
            base.Reset();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoKillChallenge c || c.crit.Value != crit.Value || c.weapon.Value != weapon.Value || c.starve.Value != starve.Value;
        }

        public override string ToString()
        {
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
                //room.ToString(),
                //"><",
                oneCycle.ToString(),
                "><",
                deathPit.ToString(),
                "><",
                starve.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
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
                //room = SettingBoxFromString(array[6]) as SettingBox<string>;
                oneCycle = SettingBoxFromString(array[6]) as SettingBox<bool>;
                deathPit = SettingBoxFromString(array[7]) as SettingBox<bool>;
                starve = SettingBoxFromString(array[8]) as SettingBox<bool>;
                completed = (array[9] == "1");
                revealed = (array[10] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoKillChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override bool RespondToCreatureKill()
        {
            return true;
        }

        public override void CreatureKilled(Creature c, int playerNumber)
        {
            
            if (c.Template.smallCreature || deathPit.Value || TeamsCompleted[SteamTest.team] || hidden || completed || game == null || c == null || revealed) return;
            if (!CreatureHitByDesired(c)) return;
            if (!CritInLocation(c)) return;
            if (starve.Value && game.Players != null && game.Players.Count > 0 && game.Players[playerNumber].realizedCreature is Player p && !p.Malnourished) return;
            CreatureType type = c.abstractCreature.creatureTemplate.type;
            bool flag = crit.Value == "Any Creature" || type.value == crit.Value;
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
                else ChangeValue();
            }
        }

        public bool CreatureHitByDesired(Creature c)
        {
            if (weapon.Value == "Any Weapon") return true;
            if (BingoData.hitTimeline.TryGetValue(c.abstractCreature.ID, out var list))
            {
                if (list.Last(x => list.IndexOf(x) != -1 && list.IndexOf(x) > (list.Count - 2)) == new ItemType(weapon.Value)) return true;
            }
            return false;
        }

        public override void AddHooks()
        {
            IL.Creature.Update += Creature_UpdateIL;
        }

        public override void RemoveHooks()
        {
            IL.Creature.Update -= Creature_UpdateIL;
        }

        public override List<object> Settings() => [crit, weapon, amount, region, sub, oneCycle, deathPit, starve];
    }
}
