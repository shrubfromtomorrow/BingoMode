using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using CreatureType = CreatureTemplate.Type;
using System.Linq;

namespace BingoMode.Challenges
{
    // Copied from vanilla game and modified
    using static ChallengeHooks;
    public class BingoPinChallenge : Challenge, IBingoChallenge
    {
        public int current;
        public SettingBox<int> target;
        public List<Creature> pinList = [];
        public List<Spear> spearList = [];
        public SettingBox<string> region;
        public List<string> pinRegions = [];
        public SettingBox<string> crit;
    
        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Pin [<current_pin>/<pin_amount>] <crit> to walls or floors<region>")
                .Replace("<current_pin>", current.ToString())
                .Replace("<pin_amount>", target.Value.ToString())
                .Replace("<crit>", crit.Value != "_AnyCreature" ? ChallengeTools.creatureNames[new CreatureType(crit.Value).Index] : "creatures")
                .Replace("<region>", region.Value != "" ? region.Value == "multi" ? " in different regions" : " in " + Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer) : "");
            base.UpdateDescription();
        }
    
        public override int Points()
        {
            return 20;
        }
    
        public override Challenge Generate()
        {
            string r = "";
            string c = Random.value < 0.5f ? "_AnyCreature" : ChallengeUtils.Pinnable[Random.Range(0, ChallengeUtils.Pinnable.Length)];
            List<string> regions = [.. SlugcatStats.getSlugcatStoryRegions(ExpeditionData.slugcatPlayer), ..SlugcatStats.getSlugcatOptionalRegions(ExpeditionData.slugcatPlayer)];
            regions.Remove("ss");
            float radom = Random.value;
            if (radom < 0.33f) r = regions[Random.Range(0, regions.Count)];
            else if (radom < 0.66f) r = "multi";
    
            return new BingoPinChallenge
            {
                target = new(Mathf.FloorToInt(Random.Range(4, 8) / (r == "multi" ? 2.5f : 1f)), "Amount", 0),
                crit = new(c, "Creature Type", 1, listName: "creatures"),
                region = new(r, "Region", 2, listName: "regions"),
            };
        }
    
        public override void Update()
        {
            base.Update();
            if (completed || game.Players == null) return;
            for (int i = 0; i < this.game.Players.Count; i++)
            {
                if (this.game.Players[i] != null && this.game.Players[i].realizedCreature != null && this.game.Players[i].realizedCreature.room != null)
                {
                    for (int j = 0; j < this.game.Players[i].realizedCreature.room.updateList.Count; j++)
                    {
                        if (this.game.Players[i].realizedCreature.room.updateList[j] is Spear && (this.game.Players[i].realizedCreature.room.updateList[j] as Spear).thrownBy != null && (this.game.Players[i].realizedCreature.room.updateList[j] as Spear).thrownBy is Player && !this.spearList.Contains(this.game.Players[i].realizedCreature.room.updateList[j] as Spear))
                        {
                            this.spearList.Add(this.game.Players[i].realizedCreature.room.updateList[j] as Spear);
                        }
                    }
                }
            }
            for (int k = 0; k < this.spearList.Count; k++)
            {
                if (spearList[k] == null || spearList[k].room == null || spearList[k].room.world == null || spearList[k].room.world.region == null) continue;
                if ((this.spearList[k].thrownBy != null && !(this.spearList[k].thrownBy is Player)) || this.spearList[k] == null)
                {
                    this.spearList.Remove(this.spearList[k]);
                    break;
                }
                string rr = spearList[k].room.world.region.name;
                if ((region.Value == "" || rr == region.Value) && !pinRegions.Contains(rr) && this.spearList[k].stuckInObject != null && this.spearList[k].stuckInObject is Creature c && (crit == null || c.Template.type.value == crit.Value) && this.spearList[k].stuckInWall != null && !this.pinList.Contains(c))
                {
                    this.pinList.Add(c);
                    this.current++;
                    if (region.Value == "multi") pinRegions.Add(rr);
                    this.UpdateDescription();
                    this.spearList.Remove(this.spearList[k]);
                    return;
                }
            }
            if (this.current >= this.target.Value)
            {
                this.CompleteChallenge();
            }
        }
    
        public override bool CombatRequired()
        {
            return true;
        }
    
        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoPinChallenge;
        }
    
        public override void Reset()
        {
            this.current = 0;
            this.pinList = [];
            this.spearList = [];
            base.Reset();
        }
    
        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }
    
        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Creature Pinning");
        }
    
        public override string ToString()
        {
            return string.Concat(
            [
                "BingoPinChallenge",
                "~",
                ValueConverter.ConvertToString(current),
                "><",
                target.ToString(),
                "><",
                crit.ToString(),
                "><",
                region.ToString(),
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
                current = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                target = SettingBoxFromString(array[1]) as SettingBox<int>;
                crit = SettingBoxFromString(array[2]) as SettingBox<string>;
                region = SettingBoxFromString(array[3]) as SettingBox<string>;
                completed = (array[4] == "1");
                hidden = (array[5] == "1");
                revealed = (array[6] == "1");
                pinList = [];
                spearList = [];
                UpdateDescription();
            }
            catch (System.Exception ex)
            {
                ExpLog.Log("ERROR: BingoPinChallenge FromString() encountered an error: " + ex.Message);
            }
        }
    
        public void AddHooks()
        {
        }
    
        public void RemoveHooks()
        {
        }
    
        public List<object> Settings() => [region, crit];
    }
}
