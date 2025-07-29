using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoPearlHoardChallenge : BingoChallenge
    {
        public SettingBox<bool> common;
        public int current;
        public SettingBox<int> amount;
        public SettingBox<bool> anyShelter;
        public SettingBox<string> region;
        public List<string> collected = [];

        public override void UpdateDescription()
        {
            string location = region.Value != "Any Region" ? Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer) : "";
            this.description = ChallengeTools.IGT.Translate("<action> [<current>/<amount>] <target_item> <shelter_type> shelter <location>")
                .Replace("<action>", anyShelter.Value ? "Bring" : "Store")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString<int>(this.amount.Value))
                .Replace("<target_item>", common.Value ? ChallengeTools.IGT.Translate("common pearls") : ChallengeTools.IGT.Translate("colored pearls"))
                .Replace("<shelter_type>", anyShelter.Value ? "to any" : "in the same")
                .Replace("<location>", location != "" ? "in " + location : "");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = anyShelter.Value ?
                new Phrase([[common.Value ? Icon.PEARL_HOARD_NORMAL : Icon.PEARL_HOARD_COLOR, new Icon("singlearrow"), new Icon("doubleshelter")]]) :
                new Phrase([[new Icon("ShelterMarker"), common.Value ? Icon.PEARL_HOARD_NORMAL : Icon.PEARL_HOARD_COLOR]]);
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
            return challenge is not BingoPearlHoardChallenge c || c.common.Value != common.Value || c.region.Value != region.Value || c.anyShelter.Value != anyShelter.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Hoarding pearls in shelters");
        }

        public override Challenge Generate()
        {
            bool flag = false;
            if ((ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint) || (!ModManager.MSC && ExpeditionData.slugcatPlayer == SlugcatStats.Name.Yellow))
            {
                flag = true;
            }
            string[] array = SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).ToArray();
            if (array.Contains("HR"))
            {
                List<string> list = array.ToList<string>();
                list.Remove("HR");
                array = list.ToArray();
            }
            bool spec = UnityEngine.Random.value < 0.5f;
            string region = spec ? "Any Region" : array[UnityEngine.Random.Range(0, array.Length)];
            return new BingoPearlHoardChallenge
            {
                common = new(flag, "Common Pearls", 0),
                amount = new(UnityEngine.Random.Range(2, 4), "Amount", 1),
                anyShelter = new(UnityEngine.Random.value < 0.5f, "Any Shelter", 2),
                region = new(region, "Region", 3, listName: "regions"),
            };
        }

        public override int Points()
        {
            return (this.common.Value ? 10 : 23) * this.amount.Value * (int)(this.hidden ? 2f : 1f);
        }

        public override bool CombatRequired()
        {
            return false;
        }

        // :slughollow:
        public override void Update()
        {
            base.Update();
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden || Custom.rainWorld.processManager.upcomingProcess != null)
                return;

            for (int i = 0; i < this.game.Players.Count; i++)
            {
                var player = this.game.Players[i];
                if (player?.realizedCreature?.room == null || !player.realizedCreature.room.abstractRoom.shelter)
                    continue;

                int num = 0;
                int num2 = 0;

                foreach (var obj in player.realizedCreature.room.updateList)
                {
                    if (obj is DataPearl p && ItemInLocation(p.abstractPhysicalObject))
                    {
                        string id = p.abstractPhysicalObject.ID.ToString();
                        bool isMisc = p.AbstractPearl.dataPearlType.value == DataPearl.AbstractDataPearl.DataPearlType.Misc.value
                                      || p.AbstractPearl.dataPearlType.value == DataPearl.AbstractDataPearl.DataPearlType.Misc2.value;
                        bool isColored = !isMisc || p is PebblesPearl;

                        if (anyShelter.Value)
                        {
                            if (!collected.Contains(id))
                            {
                                if ((isMisc && common.Value) || (isColored && !common.Value))
                                {
                                    collected.Add(id);
                                    current++;
                                    UpdateDescription();

                                    if (current >= amount.Value)
                                    {
                                        CompleteChallenge();
                                        return;
                                    }
                                    else
                                    {
                                        ChangeValue();
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (isMisc)
                            {
                                num++;
                                if (num >= amount.Value)
                                {
                                    current = num;
                                    UpdateDescription();
                                    CompleteChallenge();
                                    return;
                                }
                            }
                            else if (isColored)
                            {
                                num2++;
                                if (num2 >= amount.Value)
                                {
                                    current = num2;
                                    UpdateDescription();
                                    CompleteChallenge();
                                    return;
                                }
                            }

                            UpdateDescription();
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

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoPearlHoardChallenge",
                "~",
                common.ToString(),
                "><",
                anyShelter.ToString(),
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
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
                    common = SettingBoxFromString(array[0]) as SettingBox<bool>;
                    anyShelter = SettingBoxFromString(array[1]) as SettingBox<bool>;
                    current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                    amount = SettingBoxFromString(array[3]) as SettingBox<int>;
                    region = SettingBoxFromString(array[4]) as SettingBox<string>;
                    completed = (array[5] == "1");
                    revealed = (array[6] == "1");
                    string[] arr = Regex.Split(array[7], "cLtD");
                    collected = [.. arr];
                }
                // Legacy board pearl hoard challenge compatibility
                else
                {
                    common = SettingBoxFromString(array[0]) as SettingBox<bool>;
                    amount = SettingBoxFromString(array[1]) as SettingBox<int>;
                    region = SettingBoxFromString(array[2]) as SettingBox<string>;
                    completed = (array[3] == "1");
                    revealed = (array[4] == "1");
                    anyShelter = SettingBoxFromString("System.Boolean|false|Any Shelter|2|NULL") as SettingBox<bool>;
                    collected = [];
                }
                    UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoPearlHoardChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override bool CanBeHidden()
        {
            return false;
        }

        public override void AddHooks()
        {
        }

        public override void RemoveHooks()
        {
        }

        public override List<object> Settings() => [common, amount, anyShelter, region];
    }
}
