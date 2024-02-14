using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using PearlType = DataPearl.AbstractDataPearl.DataPearlType;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoCollectPearlChallenge : Challenge, IBingoChallenge
    {
        public PearlType pearl;
        public List<string> collected;
        public string region;
        public int current;
        public int amount;
        public bool specific;

        public override void UpdateDescription()
        {
            this.description = specific ? ChallengeTools.IGT.Translate("Collect the <pearl> pearl from <region>")
                .Replace("<region>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(this.region, ExpeditionData.slugcatPlayer)))
                .Replace("<pearl>", ChallengeTools.IGT.Translate(ChallengeUtils.NameForPearl(pearl)))
                : ChallengeTools.IGT.Translate("Collect [<current>/<amount>] colored pearls")
                .Replace("<current>", ValueConverter.ConvertToString(current))
                .Replace("<amount>", ValueConverter.ConvertToString(amount));
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoCollectPearlChallenge c || c.specific != specific;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Pearl Collecting");
        }

        public void PickedUp(PearlType type)
        {
            if (completed) return;

            if (specific)
            {
                if (type == pearl) CompleteChallenge();
            }
            else
            {
                current++;
                collected.Add(type.value);
                UpdateDescription();
                if (current >= amount) CompleteChallenge();
            }
        }

        public override void Update()
        {
            base.Update();

            for (int i = 0; i < game.Players.Count; i++)
            {
                if (game.Players[i] != null 
                    && game.Players[i].realizedCreature != null 
                    && game.Players[i].realizedCreature.room != null)
                {
                    for (int g = 0; g < game.Players[i].realizedCreature.grasps.Length; g++)
                    {
                        if (game.Players[i].realizedCreature.grasps[g] != null && game.Players[i].realizedCreature.grasps[g].grabbed is DataPearl p && ((!specific && DataPearl.PearlIsNotMisc(p.AbstractPearl.dataPearlType) && !collected.Contains(p.AbstractPearl.dataPearlType.value)) || specific))
                        {
                            PickedUp(p.AbstractPearl.dataPearlType);
                        }
                    }
                }
            }
        }

        public override Challenge Generate()
        {
            bool specifi = UnityEngine.Random.value < 0.5f;
            BingoCollectPearlChallenge chal = new()
            {
                specific = specifi
            };
            if (specifi)
            {
                var porl = ChallengeUtils.CollectablePearls[UnityEngine.Random.Range(0, ChallengeUtils.CollectablePearls.Length - (ModManager.MSC ? 0 : 4))];
                string regio = porl.value.Substring(0, 2);
                chal.pearl = porl;
                chal.region = regio;
                Plugin.logger.LogMessage($"Generated pearl {porl} from {regio}");
            }
            else
            {
                chal.collected = [];
                chal.amount = UnityEngine.Random.Range(2, 7);
            }

            return chal;
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

        // Save and load `collected`
        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoCollectPearlChallenge",
                "~",
                specific ? "1" : "0",
                "><",
                specific ?  ValueConverter.ConvertToString(pearl) : "nopearl",
                "><",
                current.ToString(),
                "><",
                amount.ToString(),
                "><",
                completed ? "1" : "0",
                "><",
                hidden ? "1" : "0",
                "><",
                revealed ? "1" : "0"
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                specific = (array[0] == "1");
                pearl = specific ? new PearlType(array[1], false) : null;
                current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                amount = int.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                completed = (array[3] == "1");
                hidden = (array[4] == "1");
                revealed = (array[5] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoCollectPearlChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
        }

        public void RemoveHooks()
        {
        }
    }
}
