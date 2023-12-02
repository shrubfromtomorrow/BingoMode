using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;
using Expedition;
using ItemType = AbstractPhysicalObject.AbstractObjectType;
using CreatureType = CreatureTemplate.Type;
using MSCItemType = MoreSlugcats.MoreSlugcatsEnums.AbstractObjectType;

namespace BingoMode.Challenges
{
    public class BingoEatChallenge : Challenge
    {
        public ItemType itemFoodType;
        public CreatureType creatureFoodType;
        public int amountRequired;
        public int currentEated;
        public bool isCreature;

        public override void UpdateDescription()
        {
            Plugin.logger.LogMessage(creatureFoodType);
            description = ChallengeTools.IGT.Translate("Eat [<current>/<amount>] <food_type>")
                .Replace("<current>", ValueConverter.ConvertToString(currentEated))
                .Replace("<amount>", ValueConverter.ConvertToString(amountRequired))
                .Replace("<food_type>", isCreature ? ChallengeTools.IGT.Translate(ChallengeTools.creatureNames[creatureFoodType.Index]) : ChallengeTools.ItemName(itemFoodType));
            base.UpdateDescription();
        }

        public override string ChallengeName()
        {
            return "Eating Food";
        }

        public override bool Duplicable(Challenge challenge)
        {
            return challenge is not BingoEatChallenge || (isCreature ? (challenge as BingoEatChallenge).creatureFoodType != creatureFoodType : (challenge as BingoEatChallenge).itemFoodType != itemFoodType);
        }

        public override Challenge Generate()
        {
            // Choose random food, if Riv is selected then make glowweed available
            ItemType randomFood = null;
            CreatureType randomCreatureFood = null;
            bool c = UnityEngine.Random.value < 0.5f;

            if (c)
            {
                randomCreatureFood = ChallengeUtils.CreatureFoodTypes[UnityEngine.Random.Range(0, ChallengeUtils.CreatureFoodTypes.Length)];
            }
            else
            {
                randomFood = ChallengeUtils.ItemFoodTypes[UnityEngine.Random.Range(0, ChallengeUtils.ItemFoodTypes.Length -
                            (ModManager.MSC ? (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Rivulet ? 0 : 1) : 4))];
            }

            return new BingoEatChallenge()
            {
                itemFoodType = randomFood,
                creatureFoodType = randomCreatureFood,
                isCreature = c,
                amountRequired = Mathf.RoundToInt(Mathf.Lerp(3, Mathf.Lerp(6, 10, UnityEngine.Random.value), ExpeditionData.challengeDifficulty)) * (isCreature && creatureFoodType == CreatureType.Fly ? 3 : 1)
            };
        }

        public override bool CombatRequired()
        {
            return false;
        }

        public override int Points()
        {
            return Mathf.RoundToInt(6 * FoodDifficultyMultiplier()) * amountRequired * (hidden ? 2 : 1);
        }

        public float FoodDifficultyMultiplier()
        {
            if (!isCreature && itemFoodType == ItemType.DangleFruit) return 0.5f;
            if (!isCreature && itemFoodType == ItemType.SlimeMold) return 1.33f;
            if (!isCreature && itemFoodType == MSCItemType.GlowWeed) return 1.66f;
            if (!isCreature && itemFoodType == MSCItemType.DandelionPeach) return 1.33f;
            if (isCreature && creatureFoodType == CreatureType.SmallNeedleWorm) return 1.5f;
            if (isCreature && creatureFoodType == CreatureType.Fly) return 0.33f;

            return 1f;
        }

        public void FoodEated(IPlayerEdible thisEdibleIsShit)
        {
            if (thisEdibleIsShit != null && thisEdibleIsShit is PhysicalObject p &&
                (isCreature ? (p.abstractPhysicalObject is AbstractCreature g && g.creatureTemplate.type == creatureFoodType) : (p.abstractPhysicalObject.type == itemFoodType)))
            {
                currentEated++;
                UpdateDescription();
                if (currentEated >= amountRequired) CompleteChallenge();
            }
        }

        public override string ToString()
        {
            return string.Concat(new string[]
            {
                "FeastChallenge",
                "~",
                ValueConverter.ConvertToString(amountRequired),
                "><",
                ValueConverter.ConvertToString(currentEated),
                "><",
                isCreature ? "1" : "0",
                "><",
                isCreature ? ValueConverter.ConvertToString(creatureFoodType.value) : ValueConverter.ConvertToString(itemFoodType.value),
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
                amountRequired = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                currentEated = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                isCreature = (array[2] == "1");
                if (isCreature) creatureFoodType = new(array[3], false);
                else itemFoodType = new(array[3], false);
                completed = (array[4] == "1");
                hidden = (array[5] == "1");
                revealed = (array[6] == "1");
                UpdateDescription();
            }
            catch (Exception ex)
            {
                ExpLog.Log("ERROR: FeastChallenge FromString() encountered an error: " + ex.Message);
            }
        }
    }
}
