using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using Watcher;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using PearlType = DataPearl.AbstractDataPearl.DataPearlType;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class WatcherBingoCollectPearlChallenge : BingoChallenge
    {
        public SettingBox<string> pearl;
        public List<string> collected = [];
        public string region;
        public int current;
        public SettingBox<int> amount;
        public SettingBox<bool> specific;

        public WatcherBingoCollectPearlChallenge()
        {
            pearl = new("", "Pearl", 1, listName: "Wpearls");
            collected = [];
            amount = new(0, "Amount", 3);
            specific = new(false, "Specific Pearl", 0);
        }

        public override void UpdateDescription()
        {
            region = pearl.Value.Substring(0, 4);
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
                return new Phrase(
                    [[new Verse(pearl.Value.Substring(pearl.Value.LastIndexOf('_') + 1))],
                    [new Icon("Symbol_Pearl", 1f, DataPearl.UniquePearlMainColor(new(pearl.Value.Substring(5), false))) { background = new FSprite("radialgradient") }]]);
            }
            return new Phrase(
                [[Icon.PEARL_HOARD_COLOR],
                [new Counter(current, amount.Value)]]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not WatcherBingoCollectPearlChallenge c || ((c.specific.Value && specific.Value) && c.pearl.Value.Substring(5) != pearl.Value.Substring(5)) || (c.specific.Value != specific.Value);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Collecting pearls");
        }

        public void PickedUp(PearlType type)
        {
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden) return;
            if (specific.Value)
            {
                if (type.value != pearl.Value.Substring(5)) return;
                current = 1;
                UpdateDescription();
                CompleteChallenge();
            }
            else
            {
                current++;
                collected.Add(type.value);
                UpdateDescription();
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
            List<string> fromList = ChallengeUtils.WCollectablePearls.ToList();

            string p = fromList[UnityEngine.Random.Range(0, fromList.Count)];
            WatcherBingoCollectPearlChallenge chal = new()
            {
                specific = new SettingBox<bool>(specifi, "Specific Pearl", 0),
                collected = []
            };
            chal.pearl = new(p, "Pearl", 1, listName: "Wpearls");
            chal.region = p.Substring(0, 4);
            chal.amount = new(UnityEngine.Random.Range(2, 7), "Amount", 3);

            return chal;
        }

        public override void Reset()
        {
            base.Reset();
            current = 0;
            collected?.Clear();
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
            return slugcat == WatcherEnums.SlugcatStatsName.Watcher;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "WatcherBingoCollectPearlChallenge",
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
                revealed ? "1" : "0",
                "><",
                string.Join("cLtD", collected),
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                specific = SettingBoxFromString(array[0]) as SettingBox<bool>;
                pearl = SettingBoxFromString(array[1]) as SettingBox<string>;
                region = pearl == null ? "noregion" : pearl.Value.Substring(0, 4);
                current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = SettingBoxFromString(array[3]) as SettingBox<int>;
                completed = (array[4] == "1");
                revealed = (array[5] == "1");
                string[] arr = Regex.Split(array[6], "cLtD");
                collected = [.. arr];

                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: WatcherBingoCollectPearlChallenge FromString() encountered an error: " + ex.Message);
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
