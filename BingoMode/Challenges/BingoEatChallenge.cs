using BingoMode.BingoSteamworks;
using Expedition;
using Menu.Remix;
using MoreSlugcats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using CreatureType = CreatureTemplate.Type;

namespace BingoMode.Challenges
{
    using static ChallengeHooks;
    public class BingoEatChallenge : BingoChallenge
    {
        public SettingBox<string> foodType;
        public SettingBox<int> amountRequired;
        public int currentEated;
        public bool isCreature;
        public override Phrase ConstructPhrase() => new Phrase([new Icon("foodSymbol", 1f, Color.white), new Icon(ChallengeUtils.ItemOrCreatureIconName(foodType.Value), 1f, ChallengeUtils.ItemOrCreatureIconColor(foodType.Value)), new Counter(currentEated, amountRequired.Value)], [2]);

        public override void UpdateDescription()
        {
            if (ChallengeTools.creatureNames == null)
            {
                ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
            }
            description = ChallengeTools.IGT.Translate("Eat [<current>/<amount>] <food_type>")
                .Replace("<current>", ValueConverter.ConvertToString(currentEated))
                .Replace("<amount>", ValueConverter.ConvertToString(amountRequired.Value))
                .Replace("<food_type>", isCreature ? ChallengeTools.IGT.Translate(ChallengeTools.creatureNames[new CreatureType(foodType.Value).Index]) : ChallengeTools.ItemName(new(foodType.Value)));
            base.UpdateDescription();
        }
    
        public override string ChallengeName()
        {
            return ChallengeTools.IGT.Translate("Eating specific food");
        }
    
        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoEatChallenge c || c.foodType.Value != foodType.Value;
        }
    
        public override Challenge Generate()
        {
            bool c = UnityEngine.Random.value < 0.5f;

            int critStart = ChallengeUtils.FoodTypes.IndexOf("VultureGrub");
            int foodCount = ChallengeUtils.FoodTypes.Length;
            string randomFood;
            if (c)
            {
                randomFood = ChallengeUtils.FoodTypes[UnityEngine.Random.Range(critStart, foodCount)];
            }
            else
            {
                List<string> foob = [.. ChallengeUtils.FoodTypes];
                if (!ModManager.MSC) foob.RemoveRange(ChallengeUtils.FoodTypes.IndexOf("GooieDuck"), 4);
                else if (ExpeditionData.slugcatPlayer != MoreSlugcatsEnums.SlugcatStatsName.Rivulet ||
                         ExpeditionData.slugcatPlayer != MoreSlugcatsEnums.SlugcatStatsName.Saint) foob.Remove("GlowWeed");
                randomFood = foob[UnityEngine.Random.Range(0, foob.Count - (foodCount - critStart))];
            }

            return new BingoEatChallenge()
            {
                foodType = new(randomFood, "Food type", 0, listName: "food"),
                isCreature = c,
                amountRequired = new(UnityEngine.Random.Range(3, 8) * (isCreature && foodType.Value == "Fly" ? 2 : 1), "Amount", 1)
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
    
        public void FoodEated(IPlayerEdible thisEdibleIsShit)
        {
            if (thisEdibleIsShit is PhysicalObject gasd)
            {
                if (gasd is Creature c)
                {
                    Plugin.logger.LogMessage($"Eated: {c.Template.type}. Our type: {foodType.Value}. Creature? {isCreature}");
                }
                else Plugin.logger.LogMessage($"Eated: {gasd.abstractPhysicalObject.type}. Our type: {foodType.Value}. Creature? {isCreature}");
            }
            if (!completed && !TeamsCompleted[SteamTest.team] && !hidden && !revealed && thisEdibleIsShit is PhysicalObject p &&
                (isCreature ? (p.abstractPhysicalObject is AbstractCreature g && g.creatureTemplate.type.value == foodType.Value) : (p.abstractPhysicalObject.type.value == foodType.Value)))
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
                "BingoEatChallenge",
                "~",
                amountRequired.ToString(),
                "><",
                currentEated.ToString(),
                "><",
                isCreature ? "1" : "0",
                "><",
                foodType.ToString(),
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
                completed = (array[4] == "1");
                revealed = (array[5] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: BingoEatChallenge FromString() encountered an error: " + ex.Message);
                throw ex;
            }
        }

        public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
        {
            return slugcat.value != "Spear";
        }

        public override void AddHooks()
        {
            On.Player.ObjectEaten += Player_ObjectEaten;
        }

        public override void RemoveHooks()
        {
            On.Player.ObjectEaten -= Player_ObjectEaten;
        }

        public override List<object> Settings() => [foodType, amountRequired];
    }
}
