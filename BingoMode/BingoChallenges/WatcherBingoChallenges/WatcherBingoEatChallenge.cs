using BingoMode.BingoRandomizer;
using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using Watcher;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.BingoChallenges
{
    using static ChallengeHooks;

    public class WatcherBingoEatChallenge : BingoChallenge
    {
        public SettingBox<string> foodType;
        public SettingBox<int> amountRequired;
        public SettingBox<bool> starve;
        public int currentEated;
        public bool isCreature;

        public WatcherBingoEatChallenge()
        {
            foodType = new("", "Food type", 0, "Wfood");
            amountRequired = new(0, "Amount", 1);
            starve = new(false, "While Starving", 2);
        }

        // Check customizer dialogue for updating iscreature
        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            description = ChallengeTools.IGT.Translate("Eat [<current>/<amount>] <food_type> <starved>")
                .Replace("<current>", ValueConverter.ConvertToString(currentEated))
                .Replace("<amount>", ValueConverter.ConvertToString(amountRequired.Value))
                .Replace("<food_type>", isCreature ? ChallengeTools.IGT.Translate(ChallengeTools.creatureNames[new CreatureType(foodType.Value).Index]) : ChallengeTools.ItemName(new(foodType.Value)))
                .Replace("<starved>", starve.Value ? ChallengeTools.IGT.Translate("while starving") : "");
            base.UpdateDescription();
        }

        public override Phrase ConstructPhrase()
        {
            Phrase phrase = new(
                [[new Icon("foodSymbol"), Icon.FromEntityName(foodType.Value)],
                [new Counter(currentEated, amountRequired.Value)]]);
            if (starve.Value) phrase.InsertWord(new Icon("Multiplayer_Death"), 1);
            return phrase;
        }

        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Eating specific food");
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not WatcherBingoEatChallenge c || c.foodType.Value != foodType.Value;
        }

        public override Challenge Generate()
        {
            bool c = UnityEngine.Random.value < 0.5f;

            int critStart = Array.IndexOf(ChallengeUtils.GetCorrectListForChallenge("food"), "VultureGrub");
            int foodCount = ChallengeUtils.GetCorrectListForChallenge("food").Length;
            string randomFood;
            if (c)
            {
                randomFood = ChallengeUtils.GetCorrectListForChallenge("food")[UnityEngine.Random.Range(critStart, foodCount)];
            }
            else
            {
                List<string> foob = [.. ChallengeUtils.GetCorrectListForChallenge("food")];
                randomFood = foob[UnityEngine.Random.Range(0, foob.Count - (foodCount - critStart))];
            }

            return new WatcherBingoEatChallenge()
            {
                foodType = new(randomFood, "Food type", 0, listName: "Wfood"),
                isCreature = c,
                starve = new(UnityEngine.Random.value < 0.1f, "While Starving", 2),
                amountRequired = new(UnityEngine.Random.Range(3, 8) * (isCreature && foodType.Value == "Fly" ? 2 : 1), "Amount", 3)
            };
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override int Points()
        {
            return 20;// Mathf.RoundToInt(6 * FoodDifficultyMultiplier()) * amountRequired.Value * (hidden ? 2 : 1);
        }

        //public float FoodDifficultyMultiplier()
        //{
        //    switch (foodType.Value)
        //    {
        //        case "DangleFruit": return 0.5f;
        //        case "SlimeMold": return 1.33f;
        //        case "GlowWeed": return 1.66f;
        //        case "DandelionPeach": return 1.33f;
        //        case "SmallNeedleWorm": return 1.5f;
        //        case "Fly": return 0.33f;
        //    }
        //
        //    return 1f;
        //}

        public void FoodEated(IPlayerEdible thisEdibleIsShit, Player playuh)
        {
            if (!completed && !TeamsCompleted[SteamTest.team] && !hidden && !revealed && thisEdibleIsShit is PhysicalObject p &&
                (isCreature ? (p.abstractPhysicalObject is AbstractCreature g && g.creatureTemplate.type.value == foodType.Value) : (p.abstractPhysicalObject.type.value == foodType.Value)) && (!starve.Value || playuh.Malnourished))
            {
                currentEated++;
                UpdateDescription();
                if (currentEated >= amountRequired.Value) CompleteChallenge();
                else ChangeValue();
            }
        }

        public override void Reset()
        {
            base.Reset();
            currentEated = 0;
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "WatcherBingoEatChallenge",
                "~",
                amountRequired.ToString(),
                "><",
                currentEated.ToString(),
                "><",
                isCreature ? "1" : "0",
                "><",
                foodType.ToString(),
                "><",
                starve.ToString(),
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
                amountRequired = SettingBoxFromString(array[0]) as SettingBox<int>;
                currentEated = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                isCreature = (array[2] == "1");
                foodType = SettingBoxFromString(array[3]) as SettingBox<string>;
                starve = SettingBoxFromString(array[4]) as SettingBox<bool>;
                completed = (array[5] == "1");
                revealed = (array[6] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: WatcherBingoEatChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat == WatcherEnums.SlugcatStatsName.Watcher;
        }

        public override void AddHooks()
        {
            On.Player.ObjectEaten += Watcher_Player_ObjectEaten;
        }

        public override void RemoveHooks()
        {
            On.Player.ObjectEaten -= Watcher_Player_ObjectEaten;
        }

        public override List<object> Settings() => [foodType, amountRequired, starve];
    }
}
