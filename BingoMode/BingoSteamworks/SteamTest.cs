using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace BingoMode.BingoSteamworks
{
    internal class SteamTest
    {
        public static List<SteamNetworkingIdentity> LobbyMembers = new ();
        public static List<SteamNetworkingIdentity> TeamMembers = new ();
        public static CSteamID CurrentLobby;

        public static SteamNetworkingIdentity selfIdentity;

        protected static Callback<SteamNetworkingMessagesSessionRequest_t> sessionRequest;
        protected static Callback<LobbyChatUpdate_t> lobbyUpdate;

        public static CallResult<LobbyMatchList_t> lobbyMatchList = new();
        public static CallResult<LobbyCreated_t> lobbyCreated = new();
        public static CallResult<LobbyEnter_t> lobbyEntered = new();

        public static void Apply()
        {
            if (SteamManager.Initialized) 
            {
                sessionRequest = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequested);
                lobbyUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyUpdate);

                SteamNetworkingSockets.GetIdentity(out selfIdentity);
                Plugin.logger.LogMessage(selfIdentity.GetSteamID());
                //CSteamID nacu = (CSteamID)76561198140779563;
                //SteamNetworkingIdentity nacku = new();
                //nacku.SetSteamID(nacu);
                //CSteamID ridg = (CSteamID)76561198357253101;
                //SteamNetworkingIdentity ridgg = new();
                //ridgg.SetSteamID(ridg);
                //
                //if (selfIdentity.GetSteamID() == nacu)
                //{
                //    Plugin.logger.LogMessage("Init send message from nacu!");
                //
                //    try
                //    {
                //        InnerWorkings.SendMessage("hi i am nacku and im yes", ridgg);
                //        InnerWorkings.SendMessage("another one!", ridgg);
                //    }
                //    catch (Exception e)
                //    {
                //        Plugin.logger.LogError("FAILED TO SEND MESSAGE AS NACU " + e);
                //    }
                //
                //    //CreateLobby();
                //}
                //else
                //{
                //    Plugin.logger.LogMessage("Init send message from ridg!");
                //    try
                //    {
                //        InnerWorkings.SendMessage("hi i am righ and im aweosme", nacku);
                //        InnerWorkings.SendMessage("another one!", nacku);
                //    }
                //    catch (Exception e)
                //    {
                //        Plugin.logger.LogError("FAILED TO SEND MESSAGE AS RIDG " + e);
                //    }
                //
                //    //LookAndJoinFirstLobby();
                //}

                //SteamNetworkingUtils.SteamNetworkingIdentity_ParseString(out var idelti, bluh);
            }
        }

        public static void CreateLobby()
        {
            SteamAPICall_t call = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 6);
            lobbyCreated.Set(call, OnLobbyCreated);
        }

        public static void LookAndJoinFirstLobby()
        {
            Plugin.logger.LogMessage("Trying to join lobby!");
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
            SteamAPICall_t call = SteamMatchmaking.RequestLobbyList();
            lobbyMatchList.Set(call, OnLobbyMatchList);
        }

        public static void LeaveLobby()
        {
            SteamMatchmaking.LeaveLobby(CurrentLobby);
            Plugin.logger.LogMessage("Left lobby " + CurrentLobby);
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
            Plugin.logger.LogMessage("Lobby created with ID " + result.m_ulSteamIDLobby + "! Setting lobby data");
            CSteamID lobbyID = (CSteamID)result.m_ulSteamIDLobby;
            SteamMatchmaking.SetLobbyData(lobbyID, "name", "Loblob");
            SteamMatchmaking.SetLobbyData(lobbyID, "host", SteamFriends.GetPersonaName());
            SteamMatchmaking.SetLobbyData(lobbyID, "testdata", "random thing beeboobaboo");
            SteamMatchmaking.SetLobbyJoinable(lobbyID, true);
            CurrentLobby = lobbyID;
        }

        public static void OnLobbyEntered(LobbyEnter_t callback, bool bIOFailure)
        {
            if (bIOFailure) { Plugin.logger.LogError("OnLobbyEntered bIOfailure"); return; }
            if (callback.m_EChatRoomEnterResponse != 1)
            {
                Plugin.logger.LogError("Failed to enter lobby " + callback.m_ulSteamIDLobby + "! " + callback.m_EChatRoomEnterResponse);
                return;
            }
            CSteamID lobbyID = (CSteamID)callback.m_ulSteamIDLobby;
            Plugin.logger.LogMessage("Entered lobby " + callback.m_ulSteamIDLobby + "! ");
            Plugin.logger.LogMessage($"Name : {SteamMatchmaking.GetLobbyData(lobbyID, "name")}\nHost : {SteamMatchmaking.GetLobbyData(lobbyID, "host")}\nTest : {SteamMatchmaking.GetLobbyData(lobbyID, "testdata")}");
        
            int members = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
            for (int i = 0; i < members; i++)
            {
                SteamNetworkingIdentity member = new SteamNetworkingIdentity();
                member.SetSteamID(SteamMatchmaking.GetLobbyMemberByIndex(lobbyID, i));
                InnerWorkings.SendMessage($"Hello im {SteamFriends.GetPersonaName()} and i joined loby!", member);
            }
            CurrentLobby = lobbyID;
        }

        public static void OnLobbyUpdate(LobbyChatUpdate_t callback)
        {
            string text = "";
            switch (callback.m_rgfChatMemberStateChange)
            {
                case 0x0001:
                    SteamNetworkingIdentity newMember = new SteamNetworkingIdentity();
                    newMember.SetSteamID((CSteamID)callback.m_ulSteamIDUserChanged);
                    LobbyMembers.Add(newMember);
                    text = "entered";
                    break;
                case 0x0002:
                    LobbyMembers.RemoveAll(x => x.GetSteamID64() == callback.m_ulSteamIDUserChanged);
                    text = "left";
                    break;
                case 0x0004:
                    LobbyMembers.RemoveAll(x => x.GetSteamID64() == callback.m_ulSteamIDUserChanged);
                    text = "disconnected from";
                    break;
                case 0x0008:
                    LobbyMembers.RemoveAll(x => x.GetSteamID64() == callback.m_ulSteamIDUserChanged);
                    text = "kicked from";
                    break;
                case 0x0010:
                    LobbyMembers.RemoveAll(x => x.GetSteamID64() == callback.m_ulSteamIDUserChanged);
                    text = "banned from";
                    break;
            }
            Plugin.logger.LogMessage($"User {callback.m_ulSteamIDUserChanged} {text} {callback.m_ulSteamIDLobby}");

            Plugin.logger.LogMessage("Current lobby members");
            foreach (var member in LobbyMembers)
            {
                Plugin.logger.LogMessage(member.GetSteamID64());
            }
        }

        public static void OnLobbyMatchList(LobbyMatchList_t result, bool bIOFailure)
        {
            if (bIOFailure) { Plugin.logger.LogError("OnLobbyMatchList bIOfailure"); return; }
            if (result.m_nLobbiesMatching < 1)
            {
                Plugin.logger.LogError("FOUND ZERO LOBBIES!!!");
                return;
            }
            Plugin.logger.LogMessage("Found " + result.m_nLobbiesMatching + " lobbies.");

            for (int i = 0; i < result.m_nLobbiesMatching; i++)
            {
                var lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                Plugin.logger.LogMessage("Found and joining lobby with ID " + lobbyID);
                var call = SteamMatchmaking.JoinLobby(lobbyID);
                lobbyEntered.Set(call, OnLobbyEntered);
                break;
            }
        }
    }
}
