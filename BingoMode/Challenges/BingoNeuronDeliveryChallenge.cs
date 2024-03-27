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
    public class BingoNeuronDeliveryChallenge : Challenge, IBingoChallenge
    {
        public SettingBox<int> neurons;
        public int delivered;
        public int Index { get; set; }
        public bool RequireSave { get; set; }
        public bool Failed { get; set; }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return !ModManager.MSC || (!(slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear) && !(slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint) && !(slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer));
        }

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Neurons delivered to Looks to the Moon <progress>").Replace("<progress>", string.Concat(new string[]
            {
                "[",
                this.delivered.ToString(),
                "/",
                this.neurons.Value.ToString(),
                "]"
            }));
            base.UpdateDescription();
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Neuron Gifting");
        }

        public override void Update()
        {
            base.Update();
            if (this.game != null && this.game.rainWorld.progression.currentSaveState != null)
            {
                if (this.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.totNeuronsGiven > this.delivered)
                {
                    this.delivered = this.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.totNeuronsGiven;
                    this.UpdateDescription();
                    if (!RequireSave) Expedition.Expedition.coreFile.Save(false);
                }
                if (!this.completed && this.delivered >= this.neurons.Value)
                {
                    this.CompleteChallenge();
                }
            }
        }

        public override void Reset()
        {
            delivered = 0;
            base.Reset();
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override bool Duplicable(Challenge challenge)
        {
            return !(challenge is BingoNeuronDeliveryChallenge);
        }

        public override Challenge Generate()
        {
            return new BingoNeuronDeliveryChallenge
            {
                neurons = new(Mathf.RoundToInt(UnityEngine.Random.Range(1f, Mathf.Lerp(1f, 4f, Mathf.InverseLerp(0.4f, 1f, ExpeditionData.challengeDifficulty)))), "Amount of Neurons", 0)
            };
        }

        public override int Points()
        {
            return 70 * this.neurons.Value * (int)(this.hidden ? 2f : 1f);
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoNeuronDeliveryChallenge",
                "~",
                neurons.ToString(),
                "><",
                ValueConverter.ConvertToString<int>(this.delivered),
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
                this.neurons = SettingBoxFromString(array[0]) as SettingBox<int>;
                this.delivered = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                this.completed = (array[2] == "1");
                this.hidden = (array[3] == "1");
                this.revealed = (array[4] == "1");
                this.UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoNeuronDeliveryChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
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

        public List<object> Settings() => [neurons];
    }
}
