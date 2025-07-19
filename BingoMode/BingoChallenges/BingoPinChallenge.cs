using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.BingoChallenges
{
    // Copied from vanilla game and modified
    using static ChallengeHooks;
    public class BingoPinChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<int> target;
        public List<Creature> pinList = [];
        public List<Spear> spearList = [];
        public SettingBox<string> region;
        public List<string> pinRegions = [];
        public SettingBox<string> crit;

        public BingoPinChallenge()
        {
            target = new(0, "Amount", 0);
            crit = new("", "Creature Type", 1, listName: "creatures");
            region = new("", "Region", 2, listName: "regions");
        }

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate("Pin [<current_pin>/<pin_amount>] <crit> to walls or floors<region>")
                .Replace("<current_pin>", current.ToString())
                .Replace("<pin_amount>", target.Value.ToString())
                .Replace("<crit>", crit.Value != "Any Creature" ? ChallengeTools.creatureNames[new CreatureType(crit.Value).Index] : "creatures")
                .Replace("<region>", region.Value != "" ? region.Value == "Any Region" ? " in different regions" : " in " + Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer) : "");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new(
                [[new Icon("pin_creature")],
                [new Counter(current, target.Value)]]);
            if (crit.Value != "Any Creature") phrase.InsertWord(Icon.FromEntityName(crit.Value));
            if (region.Value == "Any Region") phrase.InsertWord(new Icon("TravellerA"));
            else phrase.InsertWord(new Verse(region.Value));
            return phrase;
        }

        public override int Points()
        {
            return 20;
        }

        public override Challenge Generate()
        {
            string r = "";
            int tries = 0;
            List<string> regions = [];
        shitGoBack:
            string c = Random.value < 0.3f ? "Any Creature" : ChallengeUtils.Pinnable[Random.Range(0, ChallengeUtils.Pinnable.Length)];

            if (BingoData.pinnableCreatureRegions == null || tries > 10)
            {
                regions = ChallengeUtils.GetSortedCorrectListForChallenge("regionsreal").ToList();
            }
            else
            {
                if (!BingoData.pinnableCreatureRegions.ContainsKey(c)) 
                {
                    tries += 1;
                    goto shitGoBack;
                };
                regions = BingoData.pinnableCreatureRegions[c].Where(x => x.StartsWith(ExpeditionData.slugcatPlayer.value)).ToList();
            }
            float radom = Random.value; // Radom mentioned
            if (radom < 0.7f && regions.Count > 0) r = regions[Random.Range(0, regions.Count)];
            else r = "Any Region";
            if (r.IndexOf('_') != -1) r = r.Split('_')[1];
    
            return new BingoPinChallenge
            {
                target = new(Mathf.Max(2, Mathf.FloorToInt(Random.Range(2, 7) / (r == "Any Region" ? 2f : 1f))), "Amount", 0),
                crit = new(c, "Creature Type", 1, listName: "creatures"),
                region = new(r, "Region", 2, listName: "regions"),
            };
        }

        public override void Update()
        {
            base.Update();
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden || game.Players == null) return;
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
                if ((region.Value == "Any Region" || rr == region.Value) && !pinRegions.Contains(rr) && this.spearList[k].stuckInObject != null && this.spearList[k].stuckInObject is Creature c && (crit.Value == "Any Creature" || c.Template.type.value == crit.Value) && this.spearList[k].stuckInWall != null && !this.pinList.Contains(c))
                {
                    this.pinList.Add(c);
                    this.current++;
                    if (region.Value == "Any Region") pinRegions.Add(rr);
                    this.UpdateDescription();
                    if (current != target.Value) ChangeValue();
                    this.spearList.Remove(this.spearList[k]);
                    break;
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
            current = 0;
            pinList = [];
            spearList = [];
            base.Reset();
        }
    
        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }
    
        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Pinning creatures to walls");
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
                string.Join("|", pinRegions),
                "><",
                region.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
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
                pinRegions = [];
                pinRegions = [.. array[3].Split('|')];
                region = SettingBoxFromString(array[4]) as SettingBox<string>;
                completed = (array[5] == "1");
                revealed = (array[6] == "1");
                pinList = [];
                spearList = [];
                UpdateDescription();
            }
            catch (System.Exception ex)
            {
                ExpLog.Log("ERROR: BingoPinChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }
    
        public override void AddHooks()
        {
        }
    
        public override void RemoveHooks()
        {
        }
    
        public override List<object> Settings() => [region, crit, target];
    }
}