﻿using BingoMode.BingoSteamworks;
using Expedition;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;
    public class BingoGreenNeuronChallenge : BingoChallenge
    {
        public SettingBox<bool> moon;

        public override void UpdateDescription()
        {
            description = ChallengeTools.IGT.Translate(moon.Value ? "Reactivate Looks to the Moon" : "Deliver the green neuron to Five Pebbles");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            return new Phrase([[new Icon("GuidanceNeuron", 1f, new Color(0f, 1f, 0.3f)), new Icon("singlearrow"), moon.Value ? Icon.MOON : Icon.PEBBLES]]);
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoGreenNeuronChallenge c || (c.moon.Value != moon.Value);
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Delivering the green neuron");
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
            if (!completed && !revealed && !TeamsCompleted[SteamTest.team] && !hidden)
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
                revealed ? "1" : "0",
            });
        }

        public override void FromString(string args)
        {
            try
            {
                string[] array = Regex.Split(args, "><");
                moon = SettingBoxFromString(array[0]) as SettingBox<bool>;
                completed = (array[1] == "1");
                revealed = (array[2] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoGreenNeuronChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override void AddHooks()
        {
            IL.SaveState.ctor += SaveState_ctor;
            On.SLOracleWakeUpProcedure.NextPhase += SLOracleWakeUpProcedure_NextPhase;
            On.SSOracleBehavior.SSOracleGetGreenNeuron.HoldingNeuronUpdate += SSOracleGetGreenNeuron_HoldingNeuronUpdate;
            IL.Room.Loaded += Room_LoadedGreenNeuron;
        }

        public override void RemoveHooks()
        {
            IL.SaveState.ctor -= SaveState_ctor;
            On.SLOracleWakeUpProcedure.NextPhase -= SLOracleWakeUpProcedure_NextPhase;
            On.SSOracleBehavior.SSOracleGetGreenNeuron.HoldingNeuronUpdate -= SSOracleGetGreenNeuron_HoldingNeuronUpdate;
            IL.Room.Loaded -= Room_LoadedGreenNeuron;
        }

        public override List<object> Settings() => [moon];
    }
}