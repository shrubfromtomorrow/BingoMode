using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoGreenNeuronChallenge : Challenge, IBingoChallenge
    {
        public SettingBox<bool> moon;

        public override void UpdateDescription()
        {
            //this.description = ChallengeTools.IGT.Translate("Deliver the green neuron to " + (moon ? "Looks to the Moon" : "Five Pebbles"));
            this.description = ChallengeTools.IGT.Translate(moon.Value ? "Reactivate Looks to the Moon" : "Deliver the green neuron to Five Pebbles");
            base.UpdateDescription();
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoGreenNeuronChallenge c || (c.moon != moon);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Green Neuron Delivery");
        }

        public override Challenge Generate()
        {
            return new BingoGreenNeuronChallenge
            {
                moon = new(UnityEngine.Random.value < 0.5f, "Looks to the Moon", 0)
            };
        }

        public void Delivered()
        {
            if (!completed)
            {
                CompleteChallenge();
            }
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
            return slugcat == SlugcatStats.Name.Red;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "BingoGreenNeuronChallenge",
                "~",
                moon.ToString(),
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
                moon = SettingBoxFromString(array[0]) as SettingBox<bool>;
                completed = (array[1] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoGreenNeuronChallenge FromString() encountered an error: " + ex.Message);
            }
        }

        public void AddHooks()
        {
            IL.SaveState.ctor += SaveState_ctor;
            On.SLOracleWakeUpProcedure.NextPhase += SLOracleWakeUpProcedure_NextPhase;
            On.SSOracleBehavior.SSOracleGetGreenNeuron.ctor += SSOracleGetGreenNeuron_ctor;
            IL.Room.Loaded += Room_LoadedGreenNeuron;
        }

        public void RemoveHooks()
        {
            IL.SaveState.ctor -= SaveState_ctor;
            On.SLOracleWakeUpProcedure.NextPhase -= SLOracleWakeUpProcedure_NextPhase;
            On.SSOracleBehavior.SSOracleGetGreenNeuron.ctor -= SSOracleGetGreenNeuron_ctor;
            IL.Room.Loaded -= Room_LoadedGreenNeuron;
        }

        public List<object> Settings() => [moon];
    }
}