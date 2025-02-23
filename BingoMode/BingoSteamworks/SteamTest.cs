using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Expedition;
using RWCustom;
using Steamworks;
using UnityEngine;

namespace BingoMode.BingoSteamworks
{
    using BingoMenu;

    internal class SteamTest
    {
        public static List<SteamNetworkingIdentity> LobbyMembers = new ();
        public static int team = 0;
        public static CSteamID CurrentLobby;
        public static LobbyFilters CurrentFilters;

        public static SteamNetworkingIdentity selfIdentity;

        protected static Callback<SteamNetworkingMessagesSessionRequest_t> sessionRequest;
        protected static Callback<LobbyChatUpdate_t> lobbyChatUpdate;
        protected static Callback<LobbyDataUpdate_t> lobbyDataUpdate;
        protected static Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;

        public static CallResult<LobbyMatchList_t> lobbyMatchList = new();
        public static CallResult<LobbyCreated_t> lobbyCreated = new();
        public static CallResult<LobbyEnter_t> lobbyEntered = new();

        public static void Apply()
        {
            CurrentFilters = new LobbyFilters("", 1, false);

            if (SteamManager.Initialized) 
            {
                sessionRequest = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequested);
                lobbyChatUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
                lobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
                lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
                SteamNetworkingSockets.GetIdentity(out selfIdentity);
                Plugin.logger.LogMessage(selfIdentity.GetSteamID());
            }

            BingoData.globalSettings.gamemode = BingoData.BingoGameMode.Bingo;
            BingoData.globalSettings.perks = LobbySettings.AllowUnlocks.None;
            BingoData.globalSettings.burdens = LobbySettings.AllowUnlocks.None;
        }

        public static void CreateLobby(int maxPlayers)
        {
            LobbyMembers.Clear();
            SteamAPICall_t call = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, maxPlayers);
            lobbyCreated.Set(call, OnLobbyCreated);
            BingoData.MultiplayerGame = true;
        }

        public static void GetJoinableLobbies()
        {
            Plugin.logger.LogMessage("Getting lobby list! Friends only? " + CurrentFilters.friendsOnly);

            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            if (CurrentFilters.text != "") SteamMatchmaking.AddRequestLobbyListStringFilter("name", CurrentFilters.text, ELobbyComparison.k_ELobbyComparisonEqualToOrGreaterThan);
            SteamMatchmaking.AddRequestLobbyListResultCountFilter(100);
            SteamAPICall_t call = SteamMatchmaking.RequestLobbyList();
            lobbyMatchList.Set(call, OnLobbyMatchList);
        }

        public static void LeaveLobby()
        {
            SteamMatchmaking.LeaveLobby(CurrentLobby);
            if (BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page))
            {
                page.Switch(false, false);
            }
            Plugin.logger.LogMessage("Left lobby " + CurrentLobby);
            CurrentLobby = default;
            LobbyMembers.Clear();
            BingoData.MultiplayerGame = false;
        }

        public static void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
        {
            if (BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page))
            {
                if (BingoData.globalMenu.currentPage != 4)
                {
                    if (page.grid == null)
                    {
                        BingoHooks.GlobalBoard.GenerateBoard(BingoHooks.GlobalBoard.size);
                        if (page.grid != null)
                        {
                            page.grid.RemoveSprites();
                            page.RemoveSubObject(page.grid);
                            page.grid = null;
                        }
                        page.grid = new BingoGrid(BingoData.globalMenu, page, new(BingoData.globalMenu.manager.rainWorld.screenSize.x / 2f, BingoData.globalMenu.manager.rainWorld.screenSize.y / 2f), 500f);
                        page.subObjects.Add(page.grid);
                    }
                    page.multiButton.Clicked();
                    BingoData.globalMenu.UpdatePage(4);
                    BingoData.globalMenu.MovePage(new Vector2(1500f, 0f));
                }
                if (page.slideStep == -1f)
                {
                    page.multiButton.buttonBehav.greyedOut = false;
                    page.multiButton.Clicked();
                }

                var call = SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
                lobbyEntered.Set(call, OnLobbyEntered);
            }
            else
            {
                Custom.rainWorld.processManager.ShowDialog(new InfoDialog(Custom.rainWorld.processManager, "Please enter the expedition menu to accept an invite."));
            }
        }

        public static void OnSessionRequested(SteamNetworkingMessagesSessionRequest_t callback)
        {
            SteamNetworkingMessages.AcceptSessionWithUser(ref callback.m_identityRemote);
            Plugin.logger.LogMessage("Accepted session with " + callback.m_identityRemote.GetSteamID64());
        }

        public static void OnLobbyCreated(LobbyCreated_t result, bool bIOFailure)
        {
            if (bIOFailure) { Plugin.logger.LogError("OnLobbyCreated bIOfailure"); return; }
            if (result.m_eResult != EResult.k_EResultOK)
            {
                Plugin.logger.LogError("Failed to create the lobby!!");
                return;
            }

            Plugin.logger.LogMessage("Resetting activated perks and burdens");
            if (BingoData.globalSettings.perks == LobbySettings.AllowUnlocks.None) ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("unl-"));
            if (BingoData.globalSettings.burdens == LobbySettings.AllowUnlocks.None) ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("bur-"));
            team = 0;
            Plugin.logger.LogMessage("Set team number to " + team);

            Plugin.logger.LogMessage("Lobby created with ID " + result.m_ulSteamIDLobby + "! Setting lobby data");
            CSteamID lobbyID = (CSteamID)result.m_ulSteamIDLobby;
            CurrentLobby = lobbyID;
            string hostName = SteamFriends.GetPersonaName();
            SteamMatchmaking.SetLobbyData(lobbyID, "mode", "BingoMode");
            Plugin.logger.LogMessage("SETTING IT TO BINJO??? " + SteamMatchmaking.GetLobbyData(lobbyID, "mode"));
            SteamMatchmaking.SetLobbyData(lobbyID, "name", hostName + "'s lobby");
            SteamMatchmaking.SetLobbyData(lobbyID, "isHost", hostName);
            SteamMatchmaking.SetLobbyData(lobbyID, "hostID", selfIdentity.GetSteamID64().ToString());
            SteamMatchmaking.SetLobbyData(lobbyID, "slugcat", ExpeditionData.slugcatPlayer.value);
            SteamMatchmaking.SetLobbyData(lobbyID, "gamemode", ((int)BingoData.globalSettings.gamemode).ToString());
            SteamMatchmaking.SetLobbyData(lobbyID, "friendsOnly", BingoData.globalSettings.friendsOnly ? "1" : "0");
            SteamMatchmaking.SetLobbyData(lobbyID, "banCheats", BingoData.globalSettings.banMods ? "1" : "0");
            SteamMatchmaking.SetLobbyData(lobbyID, "perks", ((int)BingoData.globalSettings.perks).ToString());
            SteamMatchmaking.SetLobbyData(lobbyID, "burdens", ((int)BingoData.globalSettings.burdens).ToString());
            SteamMatchmaking.SetLobbyData(lobbyID, "nextTeam", (team + 1).ToString());
            SteamMatchmaking.SetLobbyMemberData(lobbyID, "playerTeam", team.ToString());
            SteamMatchmaking.SetLobbyData(lobbyID, "perkList", Expedition.Expedition.coreFile.ActiveUnlocksString(ExpeditionGame.activeUnlocks.Where(x => x.StartsWith("unl-")).ToList()));
            SteamMatchmaking.SetLobbyData(lobbyID, "burdenList", Expedition.Expedition.coreFile.ActiveUnlocksString(ExpeditionGame.activeUnlocks.Where(x => x.StartsWith("bur-")).ToList()));
            SteamMatchmaking.SetLobbyData(lobbyID, "lobbyVersion", Plugin.VERSION);
            SteamMatchmaking.SetLobbyData(lobbyID, "randomSeed", UnityEngine.Random.Range(1000, 10000).ToString());
            // other settings idk
            UpdateOnlineBingo();
            SteamMatchmaking.SetLobbyJoinable(lobbyID, true);

            if (BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page))
            {
                page.Switch(true, true);
            }
        }

        public static void OnLobbyEntered(LobbyEnter_t callback, bool bIOFailure)
        {
            if (bIOFailure) { Plugin.logger.LogError("OnLobbyEntered bIOfailure"); return; }
            if (callback.m_EChatRoomEnterResponse != 1)
            {
                Plugin.logger.LogError("Failed to enter lobby " + callback.m_ulSteamIDLobby + "! " + callback.m_EChatRoomEnterResponse);
                return;
            }
            if (selfIdentity.GetSteamID() == SteamMatchmaking.GetLobbyOwner((CSteamID)callback.m_ulSteamIDLobby)) return;
            LobbyMembers.Clear();
            CSteamID lobbyID = (CSteamID)callback.m_ulSteamIDLobby;
            CurrentLobby = lobbyID;
            Plugin.logger.LogMessage("Entered lobby " + callback.m_ulSteamIDLobby + "! ");
            if (BingoData.globalMenu != null)
            {
                string slug = SteamMatchmaking.GetLobbyData(lobbyID, "slugcat");
                int valveIndex = ExpeditionGame.playableCharacters.IndexOf(new (slug, false));
                if (valveIndex != -1)
                {
                    BingoData.globalMenu.currentSelection = valveIndex;
                    BingoData.globalMenu.characterSelect.UpdateSelectedSlugcat(valveIndex);
                }
                else
                {
                    Plugin.logger.LogError("UNAVAILABLE SLUGCAT DETECTED: " + slug);
                    LeaveLobby();
                    return;
                }
            }

            FetchLobbySettings();
            Plugin.logger.LogMessage($"Parsing next team from {SteamMatchmaking.GetLobbyData(CurrentLobby, "nextTeam")}");
            team = int.Parse(SteamMatchmaking.GetLobbyData(CurrentLobby, "nextTeam"), NumberStyles.Any);

            Plugin.logger.LogMessage("HOST TEAM IS " + SteamMatchmaking.GetLobbyMemberData(lobbyID, SteamMatchmaking.GetLobbyOwner(lobbyID), "playerTeam"));
            SteamMatchmaking.SetLobbyMemberData(lobbyID, "playerTeam", team.ToString());
            Plugin.logger.LogMessage("Set team number to " + team);

            string challenjes = SteamMatchmaking.GetLobbyData(lobbyID, "challenges");
            try
            {
                BingoHooks.GlobalBoard.FromString(challenjes);
            }
            catch (Exception e)
            {
                Plugin.logger.LogError(e + "\nFAILED TO RECREATE BINGO BOARD FROM STRING FROM LOBBY: " + challenjes);
                LeaveLobby();
                return;
            }
            SteamNetworkingIdentity ownere = new SteamNetworkingIdentity();
            ownere.SetSteamID(SteamMatchmaking.GetLobbyOwner(lobbyID));
            LobbyMembers.Add(ownere);
            int members = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
            BingoData.MultiplayerGame = true;
            for (int i = 0; i < members; i++)
            {
                SteamNetworkingIdentity member = new SteamNetworkingIdentity();
                member.SetSteamID(SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i));
                if (!LobbyMembers.Contains(member) && member.GetSteamID64() != selfIdentity.GetSteamID64()) { LobbyMembers.Add(member); }
                InnerWorkings.SendMessage($"Jello im {SteamFriends.GetPersonaName()} and i joined loby!", member);
            }

            if (BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page))
            {
                page.Switch(true, false);
            }
        }

        public static void FetchLobbySettings()
        {
            Plugin.logger.LogMessage("Setting lobby settings");
            Plugin.logger.LogMessage($"Parsing gamemodes from {SteamMatchmaking.GetLobbyData(CurrentLobby, "gamemode")}");
            int gamjs = int.Parse(SteamMatchmaking.GetLobbyData(CurrentLobby, "gamemode").Trim(), NumberStyles.Any);
            BingoData.globalSettings.gamemode = (BingoData.BingoGameMode)gamjs;

            Plugin.logger.LogMessage("- setting perks burdens");
            Plugin.logger.LogMessage($"Parsing perks from {SteamMatchmaking.GetLobbyData(CurrentLobby, "perks")}");
            int perjs = int.Parse(SteamMatchmaking.GetLobbyData(CurrentLobby, "perks").Trim(), NumberStyles.Any);
            Plugin.logger.LogMessage($"Parsing burdens from {SteamMatchmaking.GetLobbyData(CurrentLobby, "burdens")}");
            int burjens = int.Parse(SteamMatchmaking.GetLobbyData(CurrentLobby, "burdens").Trim(), NumberStyles.Any);
            BingoData.globalSettings.perks = (LobbySettings.AllowUnlocks)perjs;
            BingoData.globalSettings.burdens = (LobbySettings.AllowUnlocks)burjens;
            Plugin.logger.LogMessage("Success!");

            Plugin.logger.LogMessage("Adjusting own perks to the lobby perks");
            if (BingoData.globalSettings.perks != LobbySettings.AllowUnlocks.Any)
            {
                ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("unl-"));

                string[] perks = Regex.Split(SteamMatchmaking.GetLobbyData(CurrentLobby, "perkList"), "><");
                FetchUnlocks(perks);
            }
            if (BingoData.globalSettings.burdens != LobbySettings.AllowUnlocks.Any)
            {
                ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("bur-"));

                string[] burdens = Regex.Split(SteamMatchmaking.GetLobbyData(CurrentLobby, "burdenList"), "><");
                FetchUnlocks(burdens);
            }
        }

        public static void FetchUnlocks(string[] unlockables)
        {
            foreach (string unlock in unlockables)
            {
                Plugin.logger.LogMessage($"Adding {unlock} to active expedition unlocks");
                ExpeditionGame.activeUnlocks.Add(unlock);
            }
        }

        public static void OnLobbyChatUpdate(LobbyChatUpdate_t callback)
        {
            Plugin.logger.LogMessage($"Lobby chat update " + callback.m_rgfChatMemberStateChange);
            string text = "";
            switch (callback.m_rgfChatMemberStateChange)
            {
                case 0x0001:
                    text = "entered";
                    Plugin.logger.LogMessage($"1User {callback.m_ulSteamIDUserChanged} {text} {callback.m_ulSteamIDLobby}");
                    SteamNetworkingIdentity newMember = new SteamNetworkingIdentity();
                    newMember.SetSteamID((CSteamID)callback.m_ulSteamIDUserChanged);
                    if (!LobbyMembers.Contains(newMember) && newMember.GetSteamID64() != selfIdentity.GetSteamID64()) { LobbyMembers.Add(newMember); }
                    if (SteamMatchmaking.GetLobbyOwner(CurrentLobby) == selfIdentity.GetSteamID())
                    {
                        int tim = int.Parse(SteamMatchmaking.GetLobbyData(CurrentLobby, "nextTeam"), NumberStyles.Any) + 1;
                        if (tim >= 8) tim = 0;
                        SteamMatchmaking.SetLobbyData(CurrentLobby, "nextTeam", tim.ToString());
                    }

                    if (Custom.rainWorld.processManager.upcomingProcess == ProcessManager.ProcessID.Game) break;
                    if (BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page2) && page2.inLobby)
                    {
                        page2.ResetPlayerLobby();
                    }

                    break;
                case 0x0002:
                case 0x0004:
                case 0x0008:
                case 0x0010:
                    Plugin.logger.LogMessage($"HERE");
                    text = "left";
                    Plugin.logger.LogMessage($"2User {callback.m_ulSteamIDUserChanged} {text} {callback.m_ulSteamIDLobby}");
                    LobbyMembers.RemoveAll(x => x.GetSteamID64() == callback.m_ulSteamIDUserChanged);

                    if (Custom.rainWorld.processManager.upcomingProcess == ProcessManager.ProcessID.Game) break;
                    if (SteamMatchmaking.GetLobbyData(CurrentLobby, "startGame") != "") break;
                    if (LobbyMembers.Count > 0 && BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page3) && page3.inLobby)
                    {
                        page3.ResetPlayerLobby();
                    }

                    break;
            }
            Plugin.logger.LogMessage($"User {callback.m_ulSteamIDUserChanged} {text} {callback.m_ulSteamIDLobby}");

            Plugin.logger.LogMessage("Current lobby members");
            foreach (var member in LobbyMembers)
            {
                Plugin.logger.LogMessage(member.GetSteamID64());
            }
        }

        public static void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
        {
            if (callback.m_bSuccess == 0 || CurrentLobby == (CSteamID)0 || callback.m_ulSteamIDLobby == 0) return;
            if (callback.m_ulSteamIDLobby == callback.m_ulSteamIDMember && selfIdentity.GetSteamID() != SteamMatchmaking.GetLobbyOwner((CSteamID)callback.m_ulSteamIDLobby))
            {
                string den = SteamMatchmaking.GetLobbyData(CurrentLobby, "startGame");
                if (den != "")
                {
                    Plugin.logger.LogMessage("TRYING TO START GAME BECAUSE HOST STARTED - " + den);
                    if (BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page))
                    {
                        Plugin.logger.LogMessage("ACTUALLY STARTING GAME BECAUSE HOST STARTED");
                        BingoData.BingoDen = den;
                        page.Singal(null, "STARTBINGO");
                    }
                    return;
                }

                string challenjes = SteamMatchmaking.GetLobbyData(CurrentLobby, "challenges");
                try
                {
                    BingoHooks.GlobalBoard.FromString(challenjes);
                }
                catch (Exception e)
                {
                    Plugin.logger.LogError(e + "\nFAILED TO RECREATE BINGO BOARD FROM STRING FROM LOBBY: " + challenjes);
                    LeaveLobby();
                    return;
                }

                FetchLobbySettings();
            }
            else
            {
                if (BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page2) && page2.inLobby)
                {
                    page2.ResetPlayerLobby();
                }
            }
        }

        public static void OnLobbyMatchList(LobbyMatchList_t result, bool bIOFailure)
        {
            if (bIOFailure) { Plugin.logger.LogError("OnLobbyMatchList bIOfailure"); return; }
            if (result.m_nLobbiesMatching < 1)
            {
                Plugin.logger.LogWarning("FOUND ZERO LOBBIES!!!");
                return;
            }
            Plugin.logger.LogMessage("Found " + result.m_nLobbiesMatching + " lobbies.");
            List<CSteamID> frens = [];
            int frenCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
            if (frenCount != -1)
            {
                for (int f = 0; f < frenCount; f++)
                {
                    frens.Add(SteamFriends.GetFriendByIndex(f, EFriendFlags.k_EFriendFlagImmediate));
                }
            } 

            List<CSteamID> JoinableLobbies = new();
            for (int i = 0; i < result.m_nLobbiesMatching; i++)
            {
                var lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                if (SteamMatchmaking.GetLobbyData(lobbyID, "mode") != "BingoMode") continue;
                if (SteamMatchmaking.GetLobbyData(lobbyID, "friendsOnly") == "1" || CurrentFilters.friendsOnly)
                {
                    CSteamID owner = (CSteamID)ulong.Parse(SteamMatchmaking.GetLobbyData(lobbyID, "hostID"), NumberStyles.Any);
                    //Plugin.logger.LogMessage($"Relationship to owner: {SteamFriends.GetFriendRelationship(owner) != EFriendRelationship.k_EFriendRelationshipFriend} - {SteamFriends.GetFriendRelationship(owner)}");
                    if (frens.Count > 0 && !frens.Contains(owner))
                    {
                        continue;
                    }
                }
                JoinableLobbies.Add(lobbyID);
                //Plugin.logger.LogMessage("Found and joining lobby with ID " + lobbyID);
                //var call = SteamMatchmaking.JoinLobby(lobbyID);
                //lobbyEntered.Set(call, OnLobbyEntered);
            }
            if (JoinableLobbies.Count > 0)
            {
                Plugin.logger.LogMessage("All available lobbies:");
                foreach (var lob in JoinableLobbies)
                {
                    Plugin.logger.LogMessage(lob);
                }
            }

            if (JoinableLobbies.Count > 0 && BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page))
            {
                page.AddLobbies(JoinableLobbies);
            }
        }

        public static void OnLobbyMatchListFromContinue(LobbyMatchList_t result, bool bIOFailure)
        {
            Plugin.logger.LogMessage("Lobby search from conitnue game. Searching for host's game");
            if (bIOFailure) { Plugin.logger.LogError("OnLobbyMatchList bIOfailure"); return; }
            if (result.m_nLobbiesMatching < 1)
            {
                Plugin.logger.LogError("FOUND ZERO LOBBIES!!!");
                return;
            }
            List<CSteamID> JoinableLobbies = new();
            for (int i = 0; i < result.m_nLobbiesMatching; i++)
            {
                var lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                if (SteamMatchmaking.GetLobbyData(lobbyID, "mode") != "BingoMode") continue;
                JoinableLobbies.Add(lobbyID);
                //Plugin.logger.LogMessage("Found and joining lobby with ID " + lobbyID);
                //var call = SteamMatchmaking.JoinLobby(lobbyID);
                //lobbyEntered.Set(call, OnLobbyEntered);
            }
            if (JoinableLobbies.Count > 0)
            {
                Plugin.logger.LogMessage("All available lobbies:");
                foreach (var lob in JoinableLobbies)
                {
                    Plugin.logger.LogMessage(lob);
                }
            }

            if (JoinableLobbies.Count > 0 && BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page))
            {
                page.AddLobbies(JoinableLobbies);
            }
        }

        public static void UpdateOnlineBingo()
        {
            if (CurrentLobby == default) return;

            try
            {
                string asfgas = string.Join("bChG", ExpeditionData.challengeList);
                Plugin.logger.LogMessage("SETTING " + asfgas);
                SteamMatchmaking.SetLobbyData(CurrentLobby, "challenges", asfgas);
                //foreach (var id in LobbyMembers)
                //{
                //    InnerWorkings.SendMessage("*", id);
                //}
            }
            catch (Exception e)
            {
                Plugin.logger.LogError(e + "\nFAILED TO UPDATE ONLINE BINGO BOARD");
            }
            Plugin.logger.LogWarning(SteamMatchmaking.GetLobbyData(CurrentLobby, "challenges"));
        }

        public static void UpdateOnlineSettings()
        {
            SteamMatchmaking.SetLobbyData(CurrentLobby, "gamemode", ((int)BingoData.globalSettings.gamemode).ToString());
            SteamMatchmaking.SetLobbyData(CurrentLobby, "friendsOnly", BingoData.globalSettings.friendsOnly ? "1" : "0");
            SteamMatchmaking.SetLobbyData(CurrentLobby, "perks", ((int)BingoData.globalSettings.perks).ToString());
            SteamMatchmaking.SetLobbyData(CurrentLobby, "burdens", ((int)BingoData.globalSettings.burdens).ToString());
        }

        //public static void BroadcastCompletedChallenge(Challenge ch)
        //{
        //    Plugin.logger.LogMessage("BROADCASTING CHALLENGE COMPLETED " + ch);
        //    int x = -1;
        //    int y = -1;
        //    for (int i = 0; i < BingoHooks.GlobalBoard.challengeGrid.GetLength(0); i++)
        //    {
        //        bool b = false;
        //        for (int j = 0; j < BingoHooks.GlobalBoard.challengeGrid.GetLength(1); j++)
        //        {
        //            if (BingoHooks.GlobalBoard.challengeGrid[i, j] == ch)
        //            {
        //                x = i;
        //                y = j;
        //                b = true;
        //                break;
        //            }
        //        }
        //        if (b) break;
        //    }
        //    foreach (var id in LobbyMembers)
        //    {
        //        Plugin.logger.LogMessage("Sending challenge completed to " + id.GetSteamID64());
        //        InnerWorkings.SendMessage($"#{x};{y};{team};{selfIdentity.GetSteamID64()}", id);
        //    }
        //}
        //
        //public static void BroadcastFailedChallenge(Challenge ch)
        //{
        //    Plugin.logger.LogMessage("BROADCASTING CHALLENGE FAILED " + ch);
        //    int x = -1;
        //    int y = -1;
        //    for (int i = 0; i < BingoHooks.GlobalBoard.challengeGrid.GetLength(0); i++)
        //    {
        //        bool b = false;
        //        for (int j = 0; j < BingoHooks.GlobalBoard.challengeGrid.GetLength(1); j++)
        //        {
        //            if (BingoHooks.GlobalBoard.challengeGrid[i, j] == ch)
        //            {
        //                x = i;
        //                y = j;
        //                b = true;
        //                break;
        //            }
        //        }
        //        if (b) break;
        //    }
        //    foreach (var id in LobbyMembers)
        //    {
        //        InnerWorkings.SendMessage($"^{x};{y};{team};{selfIdentity.GetSteamID64()}", id);
        //    }
        //}

        //public static void BroadcastStartGame()
        //{
        //    Plugin.logger.LogMessage("BROADCASTING GAME STARTING TO LOBBY " + CurrentLobby);
        //    foreach (var id in LobbyMembers)
        //    {
        //        InnerWorkings.SendMessage("!" + BingoData.BingoDen, id);
        //    }
        //}

        public static void SetMemberTeam(CSteamID persone, int newTeam)
        {
            SteamNetworkingIdentity receiver = new SteamNetworkingIdentity();
            receiver.SetSteamID(persone);
            InnerWorkings.SendMessage("%" + newTeam, receiver);
        }
    }
}
