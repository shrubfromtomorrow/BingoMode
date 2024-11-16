using BingoMode.BingoSteamworks;
using Expedition;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoPearlDeliveryChallenge : BingoChallenge
    {
        public SettingBox<string> region;
        public int iterator = -1;

        public override void UpdateDescription()
        {
            region.Value = region.Value.Substring(0, 2);
            string newValue = (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer) ? ChallengeTools.IGT.Translate("Five Pebbles") : ChallengeTools.IGT.Translate("Looks To The Moon");
            this.description = ChallengeTools.IGT.Translate("<region> pearl delivered to <iterator>").Replace("<region>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(region.Value, ExpeditionData.slugcatPlayer))).Replace("<iterator>", newValue);
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Verse(region.Value), new Icon("Symbol_Pearl", 1f, new Color(0.7f, 0.7f, 0.7f)), new Icon("singlearrow", 1f, Color.white, 90f), new Icon(ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer ? "nomscpebble" : "GuidanceMoon", 1f, ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer ? new Color(0.44705883f, 0.9019608f, 0.76862746f) : new Color(1f, 0.8f, 0.3f))], [2,3]);
        }

        public override void Update()
        {
            base.Update();
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden) return;
            if (this.iterator == -1)
            {
                this.iterator = ((ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer) ? 1 : 0);
            }
            for (int i = 0; i < this.game.Players.Count; i++)
            {
                if (this.game.Players[i] != null && this.game.Players[i].realizedCreature != null && this.game.Players[i].realizedCreature.room != null && (this.game.Players[i].realizedCreature.room.abstractRoom.name == ((this.iterator == 0) ? "SL_AI" : "SS_AI") || (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear && this.game.Players[i].realizedCreature.room.abstractRoom.name == "DM_AI")))
                {
                    for (int j = 0; j < this.game.Players[i].realizedCreature.room.updateList.Count; j++)
                    {
                        if (this.game.Players[i].realizedCreature.room.updateList[j] is DataPearl && ChallengeTools.ValidRegionPearl(this.region.Value, (this.game.Players[i].realizedCreature.room.updateList[j] as DataPearl).AbstractPearl.dataPearlType) && ((this.game.Players[i].realizedCreature.room.updateList[j] as DataPearl).firstChunk.pos.x > ((this.iterator == 0) ? 1400f : 0f) || (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear)))
                        {
                            this.CompleteChallenge();
                        }
                    }
                }
            }
        }

        public override Challenge Generate()
        {
            string[] slugcatStoryRegions = SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).ToArray();
            List<string> list = new List<string>();
            for (int i = 0; i < slugcatStoryRegions.Length; i++)
            {
                if (!ChallengeTools.PearlRegionBlackList().Contains(slugcatStoryRegions[i]))
                {
                    list.Add(slugcatStoryRegions[i]);
                }
            }
            string text = list[UnityEngine.Random.Range(0, list.Count)];
            return new BingoPearlDeliveryChallenge
            {
                region = new(text, "Pearl from Region", 0, listName: "regions")
            };
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Delivering colored pearls");
        }

        public override int Points()
        {
            return ((ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear) ? 50 : (30 * (int)(this.hidden ? 2f : 1f))) + 10;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoPearlDeliveryChallenge c || c.region.Value != region.Value;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return (!ModManager.MSC || !(slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)) && (ModManager.MSC || !(slugcat == SlugcatStats.Name.Yellow));
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoPearlDeliveryChallenge",
                "~",
                region.ToString(),
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
                this.region = SettingBoxFromString(array[0]) as SettingBox<string>;
                this.completed = (array[1] == "1");
                this.revealed = (array[2] == "1");
                this.UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoPearlDeliveryChallenge FromString() encountered an error: " + ex.Message);
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

        public override List<object> Settings() => [region];
    }
}
