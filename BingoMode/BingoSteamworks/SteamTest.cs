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

        //protected static Callback<GameOverlayActivated_t> bingoGameOverlayActivatedCallBack;
        protected static Callback<SteamNetworkingMessagesSessionRequest_t> bingoSessionRequest;

        public static void Apply()
        {
            if (SteamManager.Initialized) 
            {
                //bingoGameOverlayActivatedCallBack = Callback<GameOverlayActivated_t>.Create(CallbackTest);
                bingoSessionRequest = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnSessionRequest);

                SteamNetworkingSockets.GetIdentity(out selfIdentity);
                //Plugin.logger.LogMessage(identity.);
                //SteamNetworkingUtils.SteamNetworkingIdentity_ToString(ref identity, out string bluh, ;
                //selfIdentity.ToString(out string bluh);
                //SteamNetworkingUtils.SteamNetworkingIdentity_ParseString(out var idelti, bluh);
                //Plugin.logger.LogMessage(idelti.GetSteamID());

                // Ethanol
                CSteamID nacu = (CSteamID)76561198140779563;
                // Sending message to myself hihi
                if (SteamUser.GetSteamID() == nacu)
                {
                    //ReceivableIDs.Add(nacu);
                    Plugin.logger.LogMessage("Init send message from nacu!");
                    
                    try
                    {
                        InnerWorkings.SendMessage("hi helo i am", selfIdentity);
                        InnerWorkings.SendMessage("mr man!!!", selfIdentity);
                    }
                    catch (Exception e)
                    {
                        Plugin.logger.LogError("FAILED TO SEND MESSAGE AS NACU " + e);
                    }
                }
            }
        }

        public static void OnSessionRequest(SteamNetworkingMessagesSessionRequest_t callback)
        {
            Plugin.logger.LogMessage(callback.m_identityRemote.GetSteamID());
        }
    }
}
