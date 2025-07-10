using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoItemHoardChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<string> target;
        public SettingBox<int> amount;
        public SettingBox<bool> anyShelter;
        public SettingBox<string> region;
        public SettingBox<string> sub;
        public List<string> stored = [];

        public override void UpdateDescription()
        {
            string location = sub.Value != "Any Subregion" ? sub.Value : region.Value != "Any Region" ? Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer) : "";
            this.description = ChallengeTools.IGT.Translate("<action> [<current>/<amount>] <target_item> in <shelter_type> shelter <location>")
                .Replace("<action>", anyShelter.value ? "Bring" : "Store")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString<int>(this.amount.Value))
                .Replace("<target_item>", ChallengeTools.ItemName(new(target.Value)))
                .Replace("<shelter_type>", anyShelter.Value ? "any" : "the same")
                .Replace("<location>", location != "" ? "in " + location : "");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = anyShelter.Value ?
                new Phrase([new Icon(ChallengeUtils.ItemOrCreatureIconName(target.Value), 1f, ChallengeUtils.ItemOrCreatureIconColor(target.Value)), new Icon("singlearrow", 1f, Color.white), new Icon("doubleshelter", 1f, Color.white)], [3]):
                new Phrase([new Icon("ShelterMarker", 1f, Color.white), new Icon(ChallengeUtils.ItemOrCreatureIconName(target.Value), 1f, ChallengeUtils.ItemOrCreatureIconColor(target.Value))], [2]);
            int region_index = anyShelter.Value ? 3 : 2;
            if (sub.Value != "Any Subregion" || region.Value != "Any Region")
            {
                phrase.words.Add(region_index, new Verse(sub.Value != "Any Subregion" ? sub.Value : region.Value));
                phrase.newLines.Add(newLines[0] + 1);
            }
            phrase.words.Add(new Counter(current, amount.Value));
            return phrase;
        }

        public override bool Duplicable(Challenge challenge)
        {
            if (challenge is not BingoItemHoardChallenge c)
                return true;

            return c.target.Value != target.Value || c.anyShelter.Value != anyShelter.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Hoarding items in shelters");
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public override Challenge Generate()
        {
            string[] liste = ChallengeUtils.GetSortedCorrectListForChallenge("expobject");
            return new BingoItemHoardChallenge
            {
                amount = new((int)Mathf.Lerp(2f, 8f, UnityEngine.Random.value), "Amount", 0),
                target = new(liste[UnityEngine.Random.Range(0, liste.Length)], "Item", 1, listName: "expobject"),
                anyShelter = new(UnityEngine.Random.value < 0.5f, "Any Shelter", 2),
                sub = new("Any Subregion", "Subregion", 3, listName: "subregions"),
                region = new("Any Region", "Region", 4, listName: "regions"),
            };
        }

        public override int Points()
        {
            int num = 7 * this.amount.Value * (int)(this.hidden ? 2f : 1f);
            if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
            {
                num = Mathf.RoundToInt((float)num * 0.75f);
            }
            return num;
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override void Update()
        {
            base.Update();
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden || Custom.rainWorld.processManager.upcomingProcess != null)  return;
            for (int i = 0; i < this.game.Players.Count; i++)
            {
                if (this.game.Players[i] != null && this.game.Players[i].realizedCreature != null && this.game.Players[i].realizedCreature.room != null && this.game.Players[i].Room.shelter)
                {
                    int count = 0;
                    for (int j = 0; j < this.game.Players[i].realizedCreature.room.updateList.Count; j++)
                    {
                        if (this.game.Players[i].realizedCreature.room.updateList[j] is PhysicalObject p && p.abstractPhysicalObject.type.value == target.Value)
                        {
                            if (!ItemInLocation(p.abstractPhysicalObject))
                            {
                                return;
                            }
                            else
                            {
                                if (anyShelter.Value)
                                {
                                    string id = p.abstractPhysicalObject.ID.ToString();
                                    if (!stored.Contains(id))
                                    {
                                        stored.Add(id);
                                        current++;
                                        UpdateDescription();
                                        if (current >= amount.Value)
                                        {
                                            this.CompleteChallenge();
                                            return;
                                        }
                                        else ChangeValue();
                                    }
                                }
                                else
                                {
                                    count++;
                                    UpdateDescription();
                                    if (count >= amount.Value)
                                    {
                                        current = count;
                                        this.CompleteChallenge();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool ItemInLocation(AbstractPhysicalObject apo)
        {
            string location = sub.Value != "Any Subregion" ? sub.Value : region.Value != "Any Region" ? region.Value : "boowomp";
            AbstractRoom room = apo.Room;
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

        public override void Reset()
        {
            current = 0;
            base.Reset();
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoItemHoardChallenge",
                "~",
                anyShelter.ToString(),
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                target.ToString(),
                "><",
                region.ToString(),
                "><",
                sub.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                string.Join("cLtD", stored)
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                if (array.Length == 9)
                {
                    anyShelter = SettingBoxFromString(array[0]) as SettingBox<bool>;
                    current = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    amount = SettingBoxFromString(array[2]) as SettingBox<int>;
                    target = SettingBoxFromString(array[3]) as SettingBox<string>;
                    region = SettingBoxFromString(array[4]) as SettingBox<string>;
                    sub = SettingBoxFromString(array[5]) as SettingBox<string>;
                    completed = (array[6] == "1");
                    revealed = (array[7] == "1");
                    string[] arr = Regex.Split(array[8], "cLtD");
                    stored = [.. arr];
                }
                else if (array.Length == 4)
                {
                    amount = SettingBoxFromString(array[0]) as SettingBox<int>;
                    target = SettingBoxFromString(array[1]) as SettingBox<string>;
                    completed = (array[2] == "1");
                    revealed = (array[3] == "1");
                    anyShelter = SettingBoxFromString("System.Boolean|false|Any Shelter|2|NULL") as SettingBox<bool>;
                    current = 0;
                    stored = [];
                    region = SettingBoxFromString("System.String|Any Region|Region|3|regions") as SettingBox<string>;
                    sub = SettingBoxFromString("System.String|Any Subregion|Subregion|4|subregions") as SettingBox<string>;
                }
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoItemHoardChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
        }

        public override void RemoveHooks()
        {
        }

        public override List<object> Settings() => [target, amount, anyShelter, region, sub];

    }
}
