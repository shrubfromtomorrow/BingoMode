using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoPearlHoardChallenge : Challenge, IBingoChallenge
    {
        public SettingBox<bool> common;
        public SettingBox<string> region;
        public SettingBox<int> amount;

        public override void UpdateDescription()
        {
            string newValue = this.common.Value ? ChallengeTools.IGT.Translate("common pearls") : ChallengeTools.IGT.Translate("colored pearls");
            this.description = ChallengeTools.IGT.Translate("Store <amount> <target_pearl> in a shelter in <region_name>").Replace("<amount>", ValueConverter.ConvertToString<int>(this.amount.Value)).Replace("<target_pearl>", newValue).Replace("<region_name>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(this.region.Value, ExpeditionData.slugcatPlayer)));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return !(challenge is BingoPearlHoardChallenge);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Pearl Hoarding");
        }

        public override Challenge Generate()
        {
            bool flag = false;
            if ((ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint) || (!ModManager.MSC && ExpeditionData.slugcatPlayer == SlugcatStats.Name.Yellow))
            {
                flag = true;
            }
            string[] array = SlugcatStats.getSlugcatStoryRegions(ExpeditionData.slugcatPlayer);
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
                amount = new((int)Mathf.Lerp(2f, 5f, ExpeditionData.challengeDifficulty), "Amount", 1),
                region = new(text, "From Region", 2, listName: "regions")
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
                this.hidden ? "1" : "0",
                "><",
                this.revealed ? "1" : "0"
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
                hidden = (array[4] == "1");
                revealed = (array[5] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoPearlHoardChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public override bool CanBeHidden()
        {
            return false;
        }

        public void AddHooks()
        {
        }

        public void RemoveHooks()
        {
        }

        public List<object> Settings() => [amount, region, common];
    }
}
