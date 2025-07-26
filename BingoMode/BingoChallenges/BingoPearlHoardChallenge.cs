using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    using static MonoMod.InlineRT.MonoModRule;

    public class BingoPearlHoardRandomizer : ChallengeRandomizer
    {
        public Randomizer<bool> common;
        public Randomizer<string> region;
        public Randomizer<int> amount;

        public override Challenge Random()
        {
            BingoPearlHoardChallenge challenge = new();
            challenge.common.Value = common.Random();
            challenge.region.Value = region.Random();
            challenge.amount.Value = amount.Random();
            return challenge;
        }

        public override StringBuilder Serialize(string indent)
        {
            string surindent = indent + INDENT_INCREMENT;
            StringBuilder serializedContent = new();
            serializedContent.AppendLine($"{surindent}common-{common.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}region-{region.Serialize(surindent)}");
            serializedContent.AppendLine($"{surindent}amount-{amount.Serialize(surindent)}");
            return base.Serialize(indent).Replace("__Type__", "PearlHoard").Replace("__Content__", serializedContent.ToString());
        }

        public override void Deserialize(string serialized)
        {
            Dictionary<string, string> dict = ToDict(serialized);
            common = Randomizer<bool>.InitDeserialize(dict["common"]);
            region = Randomizer<string>.InitDeserialize(dict["region"]);
            amount = Randomizer<int>.InitDeserialize(dict["amount"]);
        }
    }

    public class BingoPearlHoardChallenge : BingoChallenge
    {
        public SettingBox<bool> common;
        public SettingBox<string> region;
        public SettingBox<int> amount;

        public BingoPearlHoardChallenge()
        {
            common = new(false, "Common Pearls", 0);
            amount = new(0, "Amount", 1);
            region = new("", "In Region", 2, listName: "regionsreal");
        }

        public override void UpdateDescription()
        {
            string newValue = this.common.Value ? ChallengeTools.IGT.Translate("common pearls") : ChallengeTools.IGT.Translate("colored pearls");
            this.description = ChallengeTools.IGT.Translate("Store <amount> <target_pearl> in a shelter in <region_name>").Replace("<amount>", ValueConverter.ConvertToString<int>(this.amount.Value)).Replace("<target_pearl>", newValue).Replace("<region_name>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(this.region.Value, ExpeditionData.slugcatPlayer)));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase(
                [[new Icon("ShelterMarker"), common.Value ? Icon.PEARL_HOARD_NORMAL : Icon.PEARL_HOARD_COLOR, new Verse(region.Value)],
                [new Counter(completed ? amount.Value : 0, amount.Value)]]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoPearlHoardChallenge c || (c.common.Value != common.Value && c.region.Value != region.Value);
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
            string text = array[UnityEngine.Random.Range(0, array.Length)];
            return new BingoPearlHoardChallenge
            {
                common = new(flag, "Common Pearls", 0),
                amount = new(UnityEngine.Random.Range(2, 4), "Amount", 1),
                region = new(text, "In Region", 2, listName: "regionsreal")
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

        public override void Update()
        {
            base.Update();
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden || Custom.rainWorld.processManager.upcomingProcess != null) return;
            for (int i = 0; i < this.game.Players.Count; i++)
            {
                int num = 0;
                int num2 = 0;
                if (this.game.Players[i] != null && this.game.Players[i].realizedCreature != null && this.game.Players[i].realizedCreature.room != null && this.game.Players[i].realizedCreature.room.abstractRoom.shelter && this.game.Players[i].world.name == this.region.Value)
                {
                    for (int j = 0; j < this.game.Players[i].realizedCreature.room.updateList.Count; j++)
                    {
                        if (this.game.Players[i].realizedCreature.room.updateList[j] is DataPearl)
                        {
                            if ((this.game.Players[i].realizedCreature.room.updateList[j] as DataPearl).AbstractPearl.dataPearlType.value != DataPearl.AbstractDataPearl.DataPearlType.Misc.value && (this.game.Players[i].realizedCreature.room.updateList[j] as DataPearl).AbstractPearl.dataPearlType.value != DataPearl.AbstractDataPearl.DataPearlType.Misc2.value)
                            {
                                num2++;
                            }
                            else
                            {
                                num++;
                            }
                        }
                        if (this.game.Players[i].realizedCreature.room.updateList[j] is PebblesPearl)
                        {
                            num2++;
                        }
                        if ((this.common.Value && num >= this.amount.Value) || (!this.common.Value && num2 >= this.amount.Value))
                        {
                            this.CompleteChallenge();
                            return;
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoPearlHoardChallenge",
                "~",
                common.ToString(),
                "><",
                amount.ToString(),
                "><",
                this.region.ToString(),
                "><",
                this.completed ? "1" : "0",
                "><",
                this.revealed ? "1" : "0",
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                common = SettingBoxFromString(array[0]) as SettingBox<bool>;
                amount = SettingBoxFromString(array[1]) as SettingBox<int>;
                region = SettingBoxFromString(array[2]) as SettingBox<string>;
                completed = (array[3] == "1");
                revealed = (array[4] == "1");
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

        public override List<object> Settings() => [amount, region, common];
    }
}
