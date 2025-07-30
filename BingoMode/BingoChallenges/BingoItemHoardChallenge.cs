﻿using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;


namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class BingoItemHoardRandomizer : ChallengeRandomizer
    {
        public Randomizer<string> target;
        public Randomizer<int> amount;
        public Randomizer<bool> anyShelter;
        public Randomizer<string> region;

        public override Challenge Random()
        {
            BingoItemHoardChallenge challenge = new();
            challenge.target.Value = target.Random();
            challenge.amount.Value = amount.Random();
            challenge.anyShelter.Value = anyShelter.Random();
            challenge.region.Value = region.Random();
            return challenge;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}target-{target.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}amount-{amount.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}anyShelter-{anyShelter.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}region-{region.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "ItemHoard").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            Dictionary<string, string> dict = ToDict(serialized);
            target = Randomizer<string>.InitDeserialize(dict["target"]);
            amount = Randomizer<int>.InitDeserialize(dict["amount"]);
            anyShelter = Randomizer<bool>.InitDeserialize(dict["anyShelter"]);
            region = Randomizer<string>.InitDeserialize(dict["region"]);
        }
    }

    public class BingoItemHoardChallenge : BingoChallenge
    {
        public int current;
        public SettingBox<string> target;
        public SettingBox<int> amount;
        public SettingBox<bool> anyShelter;
        public SettingBox<string> region;
        public List<string> collected = [];

        public BingoItemHoardChallenge()
        {
            amount = new(0, "Amount", 0);
            target = new("", "Item", 1, listName: "expobject");
            anyShelter = new(false, "Any Shelter", 2);
            region = new("", "Region", 3, listName: "regions");
        }

        public override void UpdateDescription()
        {
            string location = region.Value != "Any Region" ? Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer) : "";
            this.description = ChallengeTools.IGT.Translate("<action> [<current>/<amount>] <target_item> <shelter_type> shelter <location>")
                .Replace("<action>", anyShelter.Value ? "Bring" : "Store")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString<int>(this.amount.Value))
                .Replace("<target_item>", ChallengeTools.ItemName(new(target.Value)))
                .Replace("<shelter_type>", anyShelter.Value ? "to any" : "in the same")
                .Replace("<location>", location != "" ? "in " + location : "");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = anyShelter.Value ?
                new Phrase([[Icon.FromEntityName(target.Value), new Icon("singlearrow"), new Icon("doubleshelter")]]):
                new Phrase([[new Icon("ShelterMarker"), Icon.FromEntityName(target.Value)]]);
            int lastLine = 1;
            if (region.Value != "Any Region")
            {
                phrase.InsertWord(new Verse(region.Value), 1);
                lastLine = 2;
            }
            phrase.InsertWord(new Counter(current, amount.Value), lastLine);
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
            List<string> liste = [.. ChallengeUtils.GetSortedCorrectListForChallenge("expobject")];
            if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer || ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear)
            {
                liste.Remove("BubbleGrass");
            }
            if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint)
            {
                liste.Remove("LillyPuck");
            }
            return new BingoItemHoardChallenge
            {
                amount = new((int)Mathf.Lerp(2f, 8f, UnityEngine.Random.value), "Amount", 0),
                target = new(liste[UnityEngine.Random.Range(0, liste.Count)], "Item", 1, listName: "expobject"),
                anyShelter = new(UnityEngine.Random.value < 0.5f, "Any Shelter", 2),
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
                                    if (!collected.Contains(id))
                                    {
                                        collected.Add(id);
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
            string location = region.Value != "Any Region" ? region.Value : "boowomp";
            AbstractRoom room = apo.Room;
            if (location.ToLowerInvariant() == region.Value.ToLowerInvariant())
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
                completed ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                string.Join("cLtD", collected)
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                if (array.Length == 8)
                {
                    anyShelter = SettingBoxFromString(array[0]) as SettingBox<bool>;
                    current = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    amount = SettingBoxFromString(array[2]) as SettingBox<int>;
                    target = SettingBoxFromString(array[3]) as SettingBox<string>;
                    region = SettingBoxFromString(array[4]) as SettingBox<string>;
                    completed = (array[5] == "1");
                    revealed = (array[6] == "1");
                    string[] arr = Regex.Split(array[7], "cLtD");
                    collected = [.. arr];
                }
                // Legacy board hoard challenge compatibility
                else if (array.Length == 7)
                {
                    anyShelter = SettingBoxFromString(array[0]) as SettingBox<bool>;
                    current = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    amount = SettingBoxFromString(array[2]) as SettingBox<int>;
                    target = SettingBoxFromString(array[3]) as SettingBox<string>;
                    completed = (array[4] == "1");
                    revealed = (array[5] == "1");
                    string[] arr = Regex.Split(array[6], "cLtD");
                    region = SettingBoxFromString("System.String|Any Region|Region|3|regions") as SettingBox<string>;
                    collected = [.. arr];
                }
                else if (array.Length == 4)
                {
                    amount = SettingBoxFromString(array[0]) as SettingBox<int>;
                    target = SettingBoxFromString(array[1]) as SettingBox<string>;
                    completed = (array[2] == "1");
                    revealed = (array[3] == "1");
                    anyShelter = SettingBoxFromString("System.Boolean|false|Any Shelter|2|NULL") as SettingBox<bool>;
                    current = 0;
                    collected = [];
                    region = SettingBoxFromString("System.String|Any Region|Region|3|regions") as SettingBox<string>;
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

        public override List<object> Settings() => [target, amount, anyShelter, region];

    }
}
