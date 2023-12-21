using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Expedition;
using Menu;
using Menu.Remix;
using UnityEngine;
using RWCustom;

namespace BingoMode
{
    using Challenges;
    using MoreSlugcats;

    public static class BingoData
    {
        public static bool BingoMode = false;
        public static List<Challenge> availableBingoChallenges;
        public static List<string> challengeTokens = [];
        public static List<string>[] possibleTokens = new List<string>[4];
        public static int[] heldItemsTime;

        public static bool MoonDead => BingoHooks.GlobalBoard.AllChallenges.Any(x => x is BingoGreenNeuronChallenge c && c.moon);

        public static void InitializeBingo()
        {
            BingoMode = true;

            challengeTokens.Clear();
            foreach (Challenge challenge in BingoHooks.GlobalBoard.AllChallenges)
            {
                if (challenge is BingoUnlockChallenge c && !challengeTokens.Contains(c.unlock)) challengeTokens.Add(c.unlock);
            }
            heldItemsTime = new int[ExtEnum<AbstractPhysicalObject.AbstractObjectType>.values.Count];
        }

        public static void FinishBingo()
        {
            ExpeditionData.ClearActiveChallengeList();
            BingoMode = false;
        }

        public static void FillPossibleTokens(SlugcatStats.Name slug)
        {
            Plugin.logger.LogMessage("Current slug: " + slug);
            possibleTokens[0] = []; // blue
            possibleTokens[1] = []; // gold
            possibleTokens[2] = []; // red
            possibleTokens[3] = []; // green
            foreach (var kvp in Custom.rainWorld.regionBlueTokens)
            {
                for (int n = 0; n < kvp.Value.Count; n++)
                {
                    if (kvp.Key.ToLowerInvariant() == "lc" && slug != MoreSlugcatsEnums.SlugcatStatsName.Artificer) continue;
                    if (Custom.rainWorld.regionBlueTokensAccessibility[kvp.Key][n].Contains(slug))
                    {
                        Plugin.logger.LogMessage("ACCESSIBLE BLUE: " + kvp.Value[n].value);
                        possibleTokens[0].Add(kvp.Value[n].value);
                    }
                }
            }
            foreach (var kvp in Custom.rainWorld.regionGoldTokens)
            {
                for (int n = 0; n < kvp.Value.Count; n++)
                {
                    if (kvp.Key.ToLowerInvariant() == "lc" && slug != MoreSlugcatsEnums.SlugcatStatsName.Artificer) continue;
                    if (Custom.rainWorld.regionGoldTokensAccessibility[kvp.Key][n].Contains(slug))
                    {
                        Plugin.logger.LogMessage("ACCESSIBLE GOLD: " + kvp.Value[n].value);
                        possibleTokens[1].Add(kvp.Value[n].value);
                    }
                }
            }
            foreach (var kvp in Custom.rainWorld.regionRedTokens)
            {
                if (kvp.Key.ToLowerInvariant() == "lc" && slug != MoreSlugcatsEnums.SlugcatStatsName.Artificer) continue;
                for (int n = 0; n < kvp.Value.Count; n++)
                {
                    if (Custom.rainWorld.regionRedTokensAccessibility[kvp.Key][n].Contains(slug) && SlugcatStats.getSlugcatStoryRegions(slug).Concat(SlugcatStats.getSlugcatOptionalRegions(slug)).Contains(kvp.Key.ToUpperInvariant()))
                    {
                        Plugin.logger.LogMessage("ACCESSIBLE SAFARI: " + kvp.Value[n].value + "-safari");
                        possibleTokens[2].Add(kvp.Value[n].value + "-safari");
                    }
                }
            }
            // Painfully hardcoded because greentokenaccessiblity sucks ass
            if (SlugcatStats.IsSlugcatFromMSC(slug))
            {
                foreach (var kvp in Custom.rainWorld.regionGreenTokens)
                {
                    for (int n = 0; n < kvp.Value.Count; n++)
                    {
                        if ((slug == MoreSlugcatsEnums.SlugcatStatsName.Rivulet && kvp.Key.ToLowerInvariant() == "ms") || 
                            (slug == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && kvp.Key.ToLowerInvariant() == "oe") || 
                            (slug == MoreSlugcatsEnums.SlugcatStatsName.Saint && kvp.Key.ToLowerInvariant() == "cl") ||
                            (slug == MoreSlugcatsEnums.SlugcatStatsName.Artificer && kvp.Key.ToLowerInvariant() == "lc") ||
                            (slug == MoreSlugcatsEnums.SlugcatStatsName.Spear && kvp.Key.ToLowerInvariant() == "dm"))
                        {
                            Plugin.logger.LogMessage("ACCESSIBLE GREEN: " + kvp.Value[n].value);
                            possibleTokens[3].Add(kvp.Value[n].value);
                        }
                    }
                }
            }
        }
    }
}
