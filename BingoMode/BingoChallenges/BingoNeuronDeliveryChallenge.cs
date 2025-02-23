using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoNeuronDeliveryChallenge : BingoChallenge
    {
        public SettingBox<int> neurons;
        public int delivered;

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

        public override Phrase ConstructPhrase()
        {
            return new Phrase([new Icon("Symbol_Neuron", 1f, Color.white), new Icon("singlearrow", 1f, Color.white), new Icon("GuidanceMoon", 1f, new Color(1f, 0.8f, 0.3f)), new Counter(delivered, neurons.Value)], [3]);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Gifting neurons");
        }

        public override void Update()
        {
            base.Update();
            if (completed || revealed || TeamsCompleted[SteamTest.team] || hidden) return;
            if (this.game != null && this.game.rainWorld.progression.currentSaveState != null)
            {
                if (this.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.totNeuronsGiven > this.delivered)
                {
                    this.delivered = this.game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.totNeuronsGiven;
                    this.UpdateDescription();
                    ChangeValue();
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
                neurons = new(Mathf.RoundToInt(UnityEngine.Random.Range(1f, Mathf.Lerp(1f, 4f, Mathf.InverseLerp(0.4f, 1f, UnityEngine.Random.value)))), "Amount of Neurons", 0)
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
                this.revealed = (array[3] == "1");
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

        public override void AddHooks()
        {
        }

        public override void RemoveHooks()
        {
        }

        public override List<object> Settings() => [neurons];
    }
}
