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
using ItemType = AbstractPhysicalObject.AbstractObjectType;

namespace BingoMode
{
    using BingoSteamworks;
    using Challenges;
    using MoreSlugcats;
    using Steamworks;
    using System.IO;
    using System.Text.RegularExpressions;

    public static class BingoData
    {
        public static bool BingoMode;
        public static bool MultiplayerGame;
        public static Dictionary<SlugcatStats.Name, BingoSaveData> BingoSaves = []; // slug and board size
        public static List<Challenge> availableBingoChallenges;
        public static List<string> challengeTokens = [];
        public static List<string>[] possibleTokens = new List<string>[4];
        public static int[] heldItemsTime;
        public static List<string> appliedChallenges = [];
        // This prevents the same creatures being hit by the same sources multiple times
        public static Dictionary<Creature, List<EntityID>> blacklist = [];
        public static Dictionary<EntityID, List<ItemType>> hitTimeline = [];
        public static ExpeditionMenu globalMenu;
        public static LobbySettings globalSettings = new LobbySettings();
        public static string BingoDen = "random";
        public static List<int> TeamsInBingo = [0];

        public static bool MoonDead => ExpeditionData.challengeList.Any(x => x is BingoGreenNeuronChallenge c && c.moon.Value);

        public struct BingoSaveData
        {
            public int size;
            public SteamNetworkingIdentity hostID;
            public bool isHost;
            public string connectedPlayers;
            public int team;
            public bool lockout;

            public BingoSaveData(int size)
            {
                this.size = size;
            }

            public BingoSaveData(int size, int team, SteamNetworkingIdentity hostID, bool isHost, string connectedPlayers, bool lockout)
            {
                this.size = size;
                this.team = team;
                this.hostID = hostID;
                this.isHost = isHost;
                this.connectedPlayers = connectedPlayers;
                this.lockout = lockout;
            }
        }

        public static List<Challenge> GetAdequateChallengeList(SlugcatStats.Name slug)
        {
            List<Challenge> list = [.. availableBingoChallenges];
            list.RemoveAll(x => !x.ValidForThisSlugcat(slug));
            return list;
        }

        public static void InitializeBingo()
        {
            BingoMode = true;
            appliedChallenges = [];
            HookAll(ExpeditionData.challengeList, false);
            HookAll(ExpeditionData.challengeList, true);

            challengeTokens.Clear();
            foreach (Challenge challenge in ExpeditionData.challengeList)
            {
                if (challenge is BingoUnlockChallenge c && !challengeTokens.Contains(c.unlock.Value))
                {
                    challengeTokens.Add(c.unlock.Value);
                    Plugin.logger.LogMessage("Addunlock " + c.unlock.Value);
                }
            }
            heldItemsTime = new int[ExtEnum<ItemType>.values.Count];
            blacklist = [];
            hitTimeline = [];
        }

        public static void FinishBingo()
        {
            ExpeditionData.ClearActiveChallengeList();
            BingoMode = false;
            Expedition.Expedition.coreFile.Save(false);
        }

        // Mostly taken from vanilla game
        public static string RandomBingoDen(SlugcatStats.Name slug)
        {
            Plugin.logger.LogMessage($"Getting bingo den! - {BingoDen}");
            if (BingoDen.Trim().ToLowerInvariant() != "random") return BingoDen;

            List<string> bannedRegions = [];
            foreach (Challenge ch in ExpeditionData.challengeList)
            {
                if (ch is BingoAllRegionsExcept a)
                {
                    bannedRegions.Add(a.region.Value);
                }
                else if (ch is BingoNoRegionChallenge b)
                {
                    bannedRegions.Add(b.region.Value);
                }
            }

            Dictionary<string, int> dictionary = [];
            Dictionary<string, List<string>> dictionary2 = [];
            List<string> list2 = SlugcatStats.SlugcatStoryRegions(slug);
            if (File.Exists(AssetManager.ResolveFilePath("randomstarts.txt")))
            {
                string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("randomstarts.txt"));
                for (int i = 0; i < array.Length; i++)
                {
                    if (!array[i].StartsWith("//") && array[i].Length > 0)
                    {
                        string text = Regex.Split(array[i], "_")[0];
                        if (ExpeditionGame.lastRandomRegion != text && !bannedRegions.Contains(text))
                        {
                            if (!dictionary2.ContainsKey(text))
                            {
                                dictionary2.Add(text, new List<string>());
                            }
                            if (list2.Contains(text))
                            {
                                dictionary2[text].Add(array[i]);
                            }
                            else if (ModManager.MSC && (slug == SlugcatStats.Name.White || slug == SlugcatStats.Name.Yellow))
                            {
                                if (text == "OE" && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
                                {
                                    dictionary2[text].Add(array[i]);
                                }
                                if (text == "LC" && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Artificer))
                                {
                                    dictionary2[text].Add(array[i]);
                                }
                                if (text == "MS" && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Rivulet) && array[i] != "MS_S07")
                                {
                                    dictionary2[text].Add(array[i]);
                                }
                            }

                            if (dictionary2[text].Contains(array[i]) && !dictionary.ContainsKey(text))
                            {
                                dictionary.Add(text, ExpeditionGame.GetRegionWeight(text));
                            }
                        }
                    }
                }
                System.Random random = new();
                int maxValue = dictionary.Values.Sum();
                int randomIndex = random.Next(0, maxValue);
                string key = dictionary.First(delegate (KeyValuePair<string, int> x)
                {
                    randomIndex -= x.Value;
                    return randomIndex < 0;
                }).Key;
                ExpeditionGame.lastRandomRegion = key;
                int num = (from list in dictionary2.Values
                           select list.Count).Sum();
                string text2 = dictionary2[key].ElementAt(UnityEngine.Random.Range(0, dictionary2[key].Count - 1));
                ExpLog.Log(string.Format("{0} | {1} valid regions for {2} with {3} possible dens", new object[]
                {
                    text2,
                    dictionary.Keys.Count,
                    slug.value,
                    num
                }));
                return text2;
            }
            return "SU_S01";
        }

        public static void HookAll(IEnumerable<Challenge> challenges, bool add)
        {
            if (!BingoMode) return;
            // literally what is this syntax
            foreach (BingoChallenge challenge in from challenge in challenges where challenge is BingoChallenge select challenge)
            {
                string name = (challenge as Challenge).ChallengeName();
                Plugin.logger.LogMessage("Hooking " + name);
                if (add && !appliedChallenges.Contains(name))
                {
                    challenge.AddHooks();
                    appliedChallenges.Add(name);
                    Plugin.logger.LogMessage("Adding this one");
                }
                else if (!add)
                {
                    challenge.RemoveHooks();
                    Plugin.logger.LogMessage("Removing this one");
                }
            }
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
                    if (!Custom.rainWorld.regionBlueTokensAccessibility.ContainsKey(kvp.Key)) continue;
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
                    if (!Custom.rainWorld.regionGoldTokensAccessibility.ContainsKey(kvp.Key)) continue;
                    if (kvp.Key.ToLowerInvariant() == "lc" && slug != MoreSlugcatsEnums.SlugcatStatsName.Artificer) continue;
                    if (kvp.Key.ToLowerInvariant() == "cl" && slug != MoreSlugcatsEnums.SlugcatStatsName.Saint) continue;
                    if (kvp.Key.ToLowerInvariant() == "rm" && slug != MoreSlugcatsEnums.SlugcatStatsName.Rivulet) continue;
                    if (Custom.rainWorld.regionGoldTokensAccessibility[kvp.Key][n].Contains(slug))
                    {
                        Plugin.logger.LogMessage("ACCESSIBLE GOLD: " + kvp.Value[n].value);
                        possibleTokens[1].Add(kvp.Value[n].value);
                    }
                }
            }
            foreach (var kvp in Custom.rainWorld.regionRedTokens)
            {
                if (!Custom.rainWorld.regionRedTokensAccessibility.ContainsKey(kvp.Key)) continue;
                if (kvp.Key.ToLowerInvariant() == "lc" && slug != MoreSlugcatsEnums.SlugcatStatsName.Artificer) continue;
                if (kvp.Key.ToLowerInvariant() == "cl" && slug != MoreSlugcatsEnums.SlugcatStatsName.Saint) continue;
                if (kvp.Key.ToLowerInvariant() == "rm" && slug != MoreSlugcatsEnums.SlugcatStatsName.Rivulet) continue;
                for (int n = 0; n < kvp.Value.Count; n++)
                {
                    if (Custom.rainWorld.regionRedTokensAccessibility[kvp.Key][n].Contains(slug) && SlugcatStats.SlugcatStoryRegions(slug).Concat(SlugcatStats.SlugcatOptionalRegions(slug)).Contains(kvp.Key.ToUpperInvariant()))
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
