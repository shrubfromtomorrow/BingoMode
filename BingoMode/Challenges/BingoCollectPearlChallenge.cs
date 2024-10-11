using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using PearlType = DataPearl.AbstractDataPearl.DataPearlType;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoCollectPearlChallenge : BingoChallenge
    {
        public SettingBox<string> pearl; //PearlType
        public List<string> collected = [];
        public string region;
        public int current;
        public SettingBox<int> amount;
        public SettingBox<bool> specific;

        public override void UpdateDescription()
        {
            region = pearl.Value.Substring(0, 2);
            this.description = specific.Value ? ChallengeTools.IGT.Translate("Collect the <pearl> pearl from <region>")
                .Replace("<region>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(region, ExpeditionData.slugcatPlayer)))
                .Replace("<pearl>", ChallengeTools.IGT.Translate(ChallengeUtils.NameForPearl(pearl.Value)))
                : ChallengeTools.IGT.Translate("Collect [<current>/<amount>] colored pearls")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", specific.Value ? "1" : ValueConverter.ConvertToString(amount.Value));
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            if (specific.Value)
            {
                return new Phrase([new Icon("Symbol_Pearl", 1f, DataPearl.UniquePearlMainColor(new(pearl.Value, false))), new Counter(current, 1)], [1]);
            }
            return new Phrase([new Icon("pearlhoard_color", 1f, new Color(0.7f, 0.7f, 0.7f)), new Counter(current, amount.Value)], [1]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoCollectPearlChallenge c || c.specific.Value != specific.Value;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Pearl Collecting");
        }

        public void PickedUp(PearlType type)
        {
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden) return;
            if (specific.Value)
            {
                if (type.value != pearl.Value) return;
                current = 1;
                UpdateDescription();
                CompleteChallenge();
            }
            else
            {
                current++;
                collected.Add(type.value);
                UpdateDescription(); 
                if (!RequireSave()) Expedition.Expedition.coreFile.Save(false);
                if (current >= amount.Value) CompleteChallenge();
                else ChangeValue();
            }
        }

        public override void Update()
        {
            base.Update();

            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden) return;
            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null 
                    && game.Players[i].realizedCreature != null 
                    && game.Players[i].realizedCreature.room != null)
                {
                    for (int g = 0; g < game.Players[i].realizedCreature.grasps.Length; g++)
                    {
                        if (game.Players[i].realizedCreature.grasps[g] != null && game.Players[i].realizedCreature.grasps[g].grabbed is DataPearl p && ((!specific.Value && DataPearl.PearlIsNotMisc(p.AbstractPearl.dataPearlType) && !collected.Contains(p.AbstractPearl.dataPearlType.value)) || specific.Value))
                        {
                            PickedUp(p.AbstractPearl.dataPearlType);
                            return;
                        }
                    }
                }
            }
        }

        public override Challenge Generate()
        {
            bool specifi = UnityEngine.Random.value < 0.5f;
            string p = ChallengeUtils.CollectablePearls[UnityEngine.Random.Range(0, ChallengeUtils.CollectablePearls.Length - (ModManager.MSC ? 0 : 4))];
            BingoCollectPearlChallenge chal = new()
            {
                specific = new SettingBox<bool>(specifi, "Specific Pearl", 0),
                collected = []
            };
            chal.pearl = new(p, "Pearl", 1, listName: "pearls");
            chal.region = p.Substring(0, 2);
            chal.amount = new(UnityEngine.Random.Range(2, 7), "Amount", 3);

            return chal;
        }

        public override void Reset()
        {
            base.Reset();
            current = 0;
            collected = [];
        }

        public override int Points()
        {
            return 20;
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return true;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoCollectPearlChallenge",
                "~",
                specific.ToString(),
                "><",
                pearl.ToString(),
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0",
                "><",
                string.Join("cLtD", collected),
                "><",
                TeamsToString()
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                specific = SettingBoxFromString(array[0]) as SettingBox<bool>;
                pearl = SettingBoxFromString(array[1]) as SettingBox<string>;
                region = pearl == null ? "noregion" : pearl.Value.Substring(0, 2);
                current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[3]) as SettingBox<int>;
                completed = (array[4] == "1");
                hidden = (array[5] == "1");
                revealed = (array[6] == "1");
                string[] arr = Regex.Split(array[7], "cLtD");
                collected = [.. arr];
                TeamsFromString(array[8]);

                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoCollectPearlChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
        }

        public override void RemoveHooks()
        {
        }

        public override List<object> Settings() => [amount, specific, pearl];
    }
}
