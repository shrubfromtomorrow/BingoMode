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
        public static List<CSteamID> ReceivableIDs = new ();
        public static List<CSteamID> LobbyMembers = new ();
        public static List<CSteamID> TeamMembers = new ();

        public static SteamNetworkingIdentity selfIdentity;

        protected static Callback<SteamNetworkingMessagesSessionRequest_t> sessionRequest;
        //protected static Callback<LobbyCreated_t> lobbyCreated;
        protected static Callback<LobbyEnter_t> lobbyEntered;
        protected static Callback<LobbyChatUpdate_t> lobbyUpdate;

        public static CallResult<LobbyMatchList_t> lobbyMatchList;
        public static CallResult<LobbyCreated_t> lobbyCreated;

        public static void Apply()
        {
            if (SteamManager.Initialized) 
            {
                sessionRequest = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequested);
                //lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
                lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
                lobbyUpdate = Callback<LobbyChatUpdate_t>.Create(OnLobbyUpdate);

                SteamNetworkingSockets.GetIdentity(out selfIdentity);
                Plugin.logger.LogMessage(selfIdentity.GetSteamID());
                CSteamID nacu = (CSteamID)76561198140779563;
                SteamNetworkingIdentity nacku = new();
                nacku.SetSteamID(nacu);
                CSteamID ridg = (CSteamID)76561198357253101;
                SteamNetworkingIdentity ridgg = new();
                ridgg.SetSteamID(ridg);

                if (selfIdentity.GetSteamID() == nacu)
                {
                    Plugin.logger.LogMessage("Init send message from nacu!");

                    try
                    {
                        InnerWorkings.SendMessage("hi i am nacku and im yes", ridgg);
                        InnerWorkings.SendMessage("another one!", ridgg);
                    }
                    catch (Exception e)
                    {
                        Plugin.logger.LogError("FAILED TO SEND MESSAGE AS NACU " + e);
                    }

                    SteamAPICall_t call = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 6);
                    lobbyCreated.Set(call, OnLobbyCreated);
                }
                else
                {
                    Plugin.logger.LogMessage("Init send message from ridg!");
                    try
                    {
                        InnerWorkings.SendMessage("hi i am righ and im aweosme", nacku);
                        InnerWorkings.SendMessage("another one!", nacku);
                    }
                    catch (Exception e)
                    {
                        Plugin.logger.LogError("FAILED TO SEND MESSAGE AS RIDG " + e);
                    }

                    Plugin.logger.LogMessage("Trying to join lobby as ridg!");
                    SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterFar);
                    SteamAPICall_t call = SteamMatchmaking.RequestLobbyList();
                    lobbyMatchList.Set(call, OnLobbyMatchList);
                }


                //SteamNetworkingUtils.SteamNetworkingIdentity_ToString(ref identity, out string bluh, ;
                //selfIdentity.ToString(out string bluh);
                //SteamNetworkingUtils.SteamNetworkingIdentity_ParseString(out var idelti, bluh);
                //Plugin.logger.LogMessage(idelti.GetSteamID());

                // Sending message to myself hihi
                //if (SteamUser.GetSteamID() == nacu)
                //{
                //    //ReceivableIDs.Add(nacu);
                //    Plugin.logger.LogMessage("Init send message from nacu!");
                //    
                //    try
                //    {
                //        InnerWorkings.SendMessage("hi helo i am", selfIdentity);
                //        InnerWorkings.SendMessage("mr man!!!", selfIdentity);
                //    }
                //    catch (Exception e)
                //    {
                //        Plugin.logger.LogError("FAILED TO SEND MESSAGE AS NACU " + e);
                //    }
                //}
            }
        }

        public static void OnSessionRequested(SteamNetworkingMessagesSessionRequest_t callback)
        {
            SteamNetworkingMessages.AcceptSessionWithUser(ref callback.m_identityRemote);
            Plugin.logger.LogMessage("Accepted session with " + callback.m_identityRemote.GetSteamID64());
        }

        public static void OnLobbyCreated(LobbyCreated_t result, bool bIOFailure)
        {
            if (result.m_eResult != EResult.k_EResultOK)
            {
                Plugin.logger.LogError("Failed to create the lobby!!");
                return;
            }
            Plugin.logger.LogMessage("Lobby created with ID " + result.m_ulSteamIDLobby + "! Setting lobby data");
            CSteamID lobbyID = (CSteamID)result.m_ulSteamIDLobby;
            SteamMatchmaking.SetLobbyData(lobbyID, "name", "Epic and sick ass lobby yeah");
            SteamMatchmaking.SetLobbyData(lobbyID, "host", SteamFriends.GetPersonaName());
            SteamMatchmaking.SetLobbyData(lobbyID, "testdata", "random thing beeboobaboo");
            SteamMatchmaking.SetLobbyJoinable(lobbyID, true);
        }

        public static void OnLobbyEntered(LobbyEnter_t callback)
        {
            if (callback.m_EChatRoomEnterResponse != 1)
            {
                Plugin.logger.LogError("Failed to enter lobby " + callback.m_ulSteamIDLobby + "! " + callback.m_EChatRoomEnterResponse);
                return;
            }
            CSteamID lobbyID = (CSteamID)callback.m_ulSteamIDLobby;
            Plugin.logger.LogMessage("Entered lobby " + callback.m_ulSteamIDLobby + "! ");
            Plugin.logger.LogMessage($"Name : {SteamMatchmaking.GetLobbyData(lobbyID, "name")}\nHost : {SteamMatchmaking.GetLobbyData(lobbyID, "host")}\nTest : {SteamMatchmaking.GetLobbyData(lobbyID, "testdata")}");
        }

        public static void OnLobbyUpdate(LobbyChatUpdate_t callback)
        {
            string text = "";
            switch (callback.m_rgfChatMemberStateChange)
            {
                case 0x0001:
                    text = "entered";
                    break;
                case 0x0002:
                    text = "left";
                    break;
                case 0x0004:
                    text = "disconnected from";
                    break;
                case 0x0008:
                    text = "kicked from";
                    break;
                case 0x0010:
                    text = "banned from";
                    break;
            }
            Plugin.logger.LogMessage($"User {callback.m_ulSteamIDUserChanged} {text} {callback.m_ulSteamIDLobby}");
        }

        public static void OnLobbyMatchList(LobbyMatchList_t result, bool bIOFailure)
        {
            if (result.m_nLobbiesMatching < 1)
            {
                Plugin.logger.LogError("FOUND ZERO LOBBIES!!!");
            }

            for (int i = 0; i < result.m_nLobbiesMatching; i++)
            {
                var lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                Plugin.logger.LogMessage("Found and joining lobby with ID " + lobbyID);
                SteamMatchmaking.JoinLobby(lobbyID);
            }
        }
    }
}
