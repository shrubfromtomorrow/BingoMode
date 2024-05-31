using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoItemHoardChallenge : BingoChallenge
    {
        public SettingBox<string> target;
        public SettingBox<int> amount;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Store <amount> <target_item> in the same shelter").Replace("<amount>", ValueConverter.ConvertToString<int>(this.amount.Value)).Replace("<target_item>", ChallengeTools.ItemName(new(target.Value)));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Icon("ShelterMarker", 1f, Color.white), new Icon(ChallengeUtils.ItemOrCreatureIconName(target.Value), 1f, ChallengeUtils.ItemOrCreatureIconColor(target.Value)), new Counter(completed ? amount.Value : 0, amount.Value)], [2]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return !(challenge is BingoItemHoardChallenge) || (challenge as BingoItemHoardChallenge).target != target;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Item Collecting");
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public override Challenge Generate()
        {
            AbstractPhysicalObject.AbstractObjectType abstractObjectType = ChallengeTools.ObjectTypes[UnityEngine.Random.Range(0, ChallengeTools.ObjectTypes.Count - 1)];
            return new BingoItemHoardChallenge
            {
                amount = new((int)Mathf.Lerp(2f, 8f, UnityEngine.Random.value), "Amount", 1),
                target = new(abstractObjectType.value, "Item", 0, listName: "expobject")
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
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden) return;
            for (int i = 0; i < this.game.Players.Count; i++)
            {
                if (this.game.Players[i] != null && this.game.Players[i].realizedCreature != null && this.game.Players[i].realizedCreature.room != null && this.game.Players[i].Room.shelter)
                {
                    int num = 0;
                    for (int j = 0; j < this.game.Players[i].realizedCreature.room.updateList.Count; j++)
                    {
                        if (this.game.Players[i].realizedCreature.room.updateList[j] is PhysicalObject && (this.game.Players[i].realizedCreature.room.updateList[j] as PhysicalObject).abstractPhysicalObject.type.value == target.Value)
                        {
                            num++;
                        }
                    }
                    if (num >= this.amount.Value)
                    {
                        this.CompleteChallenge();
                    }
                }
            }
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoItemHoardChallenge",
                "~",
                amount.ToString(),
                "><",
                target.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                TeamsToString()
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                amount = SettingBoxFromString(array[0]) as SettingBox<int>;
                target = SettingBoxFromString(array[1]) as SettingBox<string>;
                completed = (array[2] == "1");
                hidden = (array[3] == "1");
                revealed = (array[4] == "1");
                TeamsFromString(array[5]);
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

        public override List<object> Settings() => [target];
    }
}
