using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using System.Collections.Generic;
using System.Linq;

namespace BingoMode.Challenges
{
    public class BingoGreenNeuronChallenge : Challenge
    {
        public bool moon;

        public override void UpdateDescription()
        {
            this.description = ChallengeTools.IGT.Translate("Deliver the green neuron to " + (moon ? "Looks to the Moon" : "Five Pebbles"));
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
                moon = UnityEngine.Random.value < 0.5f
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
                "Template",
                "~",
                moon ? "1" : "0",
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
                moon = (array[0] == "1");
                completed = (array[12] == "1");
                hidden = (array[2] == "1");
                revealed = (array[3] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: Green Neuron Delivery FromString() encountered an error: " + ex.Message);
            }
        }
    }
}