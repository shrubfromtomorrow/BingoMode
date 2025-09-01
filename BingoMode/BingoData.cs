﻿using Expedition;
using Menu;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ItemType = AbstractPhysicalObject.AbstractObjectType;

namespace BingoMode
{
    using BingoChallenges;
    using BingoSteamworks;
    using MoreSlugcats;
    using Steamworks;
    using System.IO;
    using UnityEngine;

    public static class BingoData
    {
        public static bool BingoMode;
        public static bool MultiplayerGame;
        public static Dictionary<SlugcatStats.Name, BingoSaveData> BingoSaves = []; // slug and board size
        public static List<Challenge> availableBingoChallenges;
        public static List<string> challengeTokens = [];
        public static List<string>[] possibleTokens = new List<string>[5];
        public static int[] heldItemsTime;
        public static List<string> appliedChallenges = [];
        // This prevents the same creatures being hit by the same sources multiple times
        public static Dictionary<Creature, List<EntityID>> blacklist = [];
        public static Dictionary<EntityID, List<ItemType>> hitTimeline = [];
        public static ExpeditionMenu globalMenu;
        public static LobbySettings globalSettings = new LobbySettings();
        public static string BingoDen = "random";
        public static List<int> TeamsInBingo = [0];
        public static bool SpectatorMode = false;
        public static bool CreateKarmaFlower = false;
        public static Dictionary<string, List<string>> pinnableCreatureRegions;
        public static int RandomStartingSeed = -1;
        public static Dictionary<SlugcatStats.Name, List<string>> bannedChallenges = [];
        public static bool listOpen = false;

        public static bool MoonDead => ExpeditionData.challengeList.Any(x => x is BingoGreenNeuronChallenge c && c.moon.Value);

        public enum BingoGameMode
        {
            Bingo,
            Lockout,
            Blackout,
            LockoutNoTies
        }

        public class BingoSaveData
        {
            public int size;
            public SteamNetworkingIdentity hostID;
            public bool isHost;
            public string playerWhiteList;
            public int team;
            public BingoGameMode gamemode;
            public bool showedWin;
            public bool firstCycleSaved;
            public bool passageUsed;
            public string teamsInBingo;
            public bool songPlayed;

            public BingoSaveData(int size, bool showedWin, int team, bool firstCycleSaved, bool passageUsed)
            {
                this.size = size;
                this.showedWin = showedWin;
                this.team = team;
                this.firstCycleSaved = firstCycleSaved;
                this.passageUsed = passageUsed;
            }

            public BingoSaveData(int size, int team, SteamNetworkingIdentity hostID, bool isHost, string playerWhiteList, BingoGameMode gamemode, bool showedWin, bool firstCycleSaved, bool passageUsed, string teamsInBingo, bool songPlayed)
            {
                this.size = size;
                this.team = team;
                this.hostID = hostID;
                this.isHost = isHost;
                this.playerWhiteList = playerWhiteList;
                this.gamemode = gamemode;
                this.showedWin = showedWin;
                this.firstCycleSaved = firstCycleSaved;
                this.passageUsed = passageUsed;
                this.teamsInBingo = teamsInBingo;
                this.songPlayed = songPlayed;
            }
        }

        public static void SaveChallengeBlacklistFor(SlugcatStats.Name slug)
        {
            TryGenerateDefaultBlacklistFor(slug);

            string text = string.Join(";", bannedChallenges[slug]);

            File.WriteAllText(Application.persistentDataPath +
                Path.DirectorySeparatorChar.ToString() +
                "Bingo" +
                Path.DirectorySeparatorChar.ToString() +
                "blacklist-" +
                slug.value +
                ".txt",
                text);
        }

        public static void LoadAllBannedChallengeLists()
        {
            foreach (SlugcatStats.Name slug in ExpeditionData.GetPlayableCharacters())
            {
                try
                {
                    string path = Application.persistentDataPath +
                    Path.DirectorySeparatorChar.ToString() +
                    "Bingo" +
                    Path.DirectorySeparatorChar.ToString() +
                    "blacklist-" +
                    slug.value +
                    ".txt";

                    if (!File.Exists(path))
                    {
                        SaveChallengeBlacklistFor(slug);
                        continue;
                    }

                    string data = File.ReadAllText(path);

                    bannedChallenges[slug] = string.IsNullOrEmpty(data) ? [] : data.Split(';').ToList();
                }
                catch
                {
                    Plugin.logger.LogError("Failed to load banned challenge list for " + slug.value);
                    SaveChallengeBlacklistFor(slug);
                }
            }
        }

        private static void TryGenerateDefaultBlacklistFor(SlugcatStats.Name slug)
        {
            if (!bannedChallenges.ContainsKey(slug))
            {
                bannedChallenges[slug] = new List<string>
                {
                    nameof(BingoHellChallenge)
                };
            }
        }

        public static bool IsCurrentSaveLockout()
        {
            return BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer) && 
                (BingoSaves[ExpeditionData.slugcatPlayer].gamemode == BingoGameMode.Lockout || BingoSaves[ExpeditionData.slugcatPlayer].gamemode == BingoGameMode.LockoutNoTies);
        }

        public static List<int> TeamsStringToList(string teams)
        {
            List<int> teamsList = [];

            for (int i = 0; i < 8; i++)
            {
                if (teams[i] == '1') teamsList.Add(i);
            }

            return teamsList;
        }

        public static string TeamsListToString(List<int> teams)
        {
            StringBuilder builder = new("00000000");

            for (int i = 0; i < 8; i++)
            {
                if (teams.Contains(i)) builder[i] = '1';
            }

            return builder.ToString();
        }

        public static List<Challenge> GetAdequateChallengeList(SlugcatStats.Name slug)
        {
            List<Challenge> list = [.. availableBingoChallenges];
            list.RemoveAll(x => !x.ValidForThisSlugcat(slug));
            return list;
        }

        public static List<Challenge> GetValidChallengeList(SlugcatStats.Name slug)
        {
            List<Challenge> list = [.. availableBingoChallenges];
            list.RemoveAll(x => !x.ValidForThisSlugcat(slug));
            list.RemoveAll(x => bannedChallenges[slug].Contains(x.GetType().Name));
            return list;
        }

        public static void InitializeBingo()
        {
            BingoMode = true;
            appliedChallenges = [];
            HookAll(ExpeditionData.challengeList, false);
            HookAll(ExpeditionData.challengeList, true);
            heldItemsTime = new int[ExtEnum<ItemType>.values.Count];
            blacklist = [];
            hitTimeline = [];
        }

        public static void RedoTokens()
        {
            challengeTokens.Clear();
            foreach (Challenge challenge in ExpeditionData.challengeList)
            {
                if ((challenge is BingoUnlockChallenge c1 &&
                        !c1.TeamsCompleted[SteamTest.team] &&
                        !challengeTokens.Contains(c1.unlock.Value)))
                {
                    challengeTokens.Add(c1.unlock.Value);
                }
                if ((challenge is BingoBroadcastChallenge d1 &&
                        !d1.TeamsCompleted[SteamTest.team] &&
                        !challengeTokens.Contains(d1.chatlog.Value)))
                {
                    challengeTokens.Add(d1.chatlog.Value);
                }
            }
        }

        public static void FinishBingo()
        {
            ExpeditionData.ClearActiveChallengeList();
            BingoMode = false;
            Expedition.Expedition.coreFile.Save(false);
        }

        public static void HookAll(IEnumerable<Challenge> challenges, bool add)
        {
            if (!BingoMode) return;
            // literally what is this syntax
            foreach (BingoChallenge challenge in from challenge in challenges where challenge is BingoChallenge select challenge)
            {
                string name = (challenge as Challenge).ChallengeName();
                //
                if (add && !appliedChallenges.Contains(name))
                {
                    challenge.AddHooks();
                    appliedChallenges.Add(name);
                    //
                }
                else if (!add)
                {
                    challenge.RemoveHooks();
                    //
                }
            }
        }

        public static void FillPossibleTokens(SlugcatStats.Name slug)
        {
            
            possibleTokens[0] = []; // blue
            possibleTokens[1] = []; // gold
            possibleTokens[2] = []; // red
            possibleTokens[3] = []; // green
            possibleTokens[4] = []; // white
            foreach (var kvp in Custom.rainWorld.regionBlueTokens)
            {
                for (int n = 0; n < kvp.Value.Count; n++)
                {
                    if (!Custom.rainWorld.regionBlueTokensAccessibility.ContainsKey(kvp.Key)) continue;
                    if (Custom.rainWorld.regionBlueTokensAccessibility[kvp.Key][n].Contains(slug))
                    {
                        
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
                        //*
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
                    if (Custom.rainWorld.regionRedTokensAccessibility[kvp.Key][n].Contains(slug) && ChallengeUtils.GetSortedCorrectListForChallenge("regionsreal").Contains(kvp.Key.ToUpperInvariant()))
                    {
                        //.*
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
                            
                            possibleTokens[3].Add(kvp.Value[n].value);
                        }
                    }
                }
            }

            if (slug == MoreSlugcatsEnums.SlugcatStatsName.Spear)
            {
                foreach (var kvp in Custom.rainWorld.regionGreyTokens)
                {
                    for (int n = 0; n < kvp.Value.Count; n++)
                    {
                        if (!kvp.Value[n].value.ToLowerInvariant().Contains("broadcast")) {
                            possibleTokens[4].Add(kvp.Value[n].value);
                        }
                    }
                }
            }
        }
    }
}
