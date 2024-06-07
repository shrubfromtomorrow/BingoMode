using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using Expedition;
using Steamworks;
using UnityEngine;

namespace BingoMode.BingoSteamworks
{
    public class SteamFinal
    {
        public static List<SteamNetworkingIdentity> ConnectedPlayers = [];
        public static Dictionary<ulong, bool> ReceivedPlayerUpKeep = [];
        public static int HostUpkeep;

        public const int PlayerUpkeepTime = 7200;
        public const int MaxPlayerUpKeepTime = 4800;
        public const int MaxHostUpKeepTime = 6000;
        public static int SendUpKeepCounter = PlayerUpkeepTime;

        //public enum UpKeepState
        //{
        //    Idle,
        //    Sent,
        //    Received
        //}

        public static void ReceiveMessagesUpdate()
        {
            SendUpKeepCounter--;
            if (SendUpKeepCounter <= 0)
            {
                List<ulong> ToRemove = [];
                foreach (var kvp in ReceivedPlayerUpKeep)
                {
                    if (ReceivedPlayerUpKeep[kvp.Key])
                    {
                        RequestPlayerUpKeep(kvp.Key);
                        continue;
                    }

                    // Player didnt send their upkeep, so theyre considered dead to the host
                    ConnectedPlayers.RemoveAll(x => x.GetSteamID64() == kvp.Key);
                    ToRemove.Add(kvp.Key);
                    //switch (PlayerUpKeeps[kvp.Key])
                    //{
                    //    case UpKeepState.Idle:
                    //        RequestPlayerUpKeep(kvp.Key);
                    //        break;
                    //    case UpKeepState.Sent:
                    //    case UpKeepState.Received:
                    //        RequestPlayerUpKeep(kvp.Key);
                    //        break;
                    //}
                }

                if (ToRemove.Count > 0)
                {
                    foreach (var r in ToRemove)
                    {
                        ReceivedPlayerUpKeep.Remove(r);
                    }
                }

                SendUpKeepCounter = PlayerUpkeepTime;
            }

            // How the fuck does this work
            IntPtr[] messges = new IntPtr[16];
            int messages = SteamNetworkingMessages.ReceiveMessagesOnChannel(0, messges, messges.Length);
            if (messages > 0)
            {
                for (int i = 0; i < messages; i++)
                {
                    if (!BingoData.MultiplayerGame && !BingoData.BingoMode) continue;

                    SteamNetworkingMessage_t netMessage = Marshal.PtrToStructure<SteamNetworkingMessage_t>(messges[i]);

                    Plugin.logger.LogMessage("RECEIVED MESSAG???");

                    byte[] data = new byte[netMessage.m_cbSize];
                    Marshal.Copy(netMessage.m_pData, data, 0, data.Length);
                    char[] chars = new char[data.Length / sizeof(char)];
                    Buffer.BlockCopy(data, 0, chars, 0, data.Length);
                    string message = new string(chars, 0, chars.Length);
                    Plugin.logger.LogMessage(message);
                    InnerWorkings.MessageReceived(message);
                }
            }
        }

        public static void RequestPlayerUpKeep(ulong playerID)
        {
            SteamNetworkingIdentity playerIdentity = new();
            playerIdentity.SetSteamID64(playerID);
            InnerWorkings.SendMessage("U", playerIdentity);
            ReceivedPlayerUpKeep[playerID] = false;
        }

        //public static void UpdatePlayerUpKeep(ulong playerID)
        //{
        //    PlayerUpkeeps[playerID] = PlayerUpkeepTime;
        //}

        public static bool IsSaveMultiplayer(BingoData.BingoSaveData saveData)
        {
            Plugin.logger.LogMessage($"TEST BINGOSAVES HOST ID: {saveData.hostID.GetSteamID64()} is default? {saveData.hostID.GetSteamID64() == default}");
            return saveData.hostID.GetSteamID64() != default;
        }

        public static SteamNetworkingIdentity GetHost()
        {
            return BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID;
        }

        public static List<SteamNetworkingIdentity> PlayersFromString(string text)
        {
            List<SteamNetworkingIdentity> players = [];
            if (text == "") return players;
            foreach (string player in Regex.Split(text, "bPlR"))
            {
                SteamNetworkingIdentity playerIdentity = new();
                playerIdentity.SetSteamID64(ulong.Parse(player, NumberStyles.Any));
            }
            return players;
        }

        public static void ChallengeStateChangeToHost(Challenge ch, bool failed)
        {
            Plugin.logger.LogMessage("CHALLENGE STATE CHANGE TO HOST. Failed? " + failed + " - " + ch);
            int x = -1;
            int y = -1;
            for (int i = 0; i < BingoHooks.GlobalBoard.challengeGrid.GetLength(0); i++)
            {
                bool b = false;
                for (int j = 0; j < BingoHooks.GlobalBoard.challengeGrid.GetLength(1); j++)
                {
                    if (BingoHooks.GlobalBoard.challengeGrid[i, j] == ch)
                    {
                        x = i;
                        y = j;
                        b = true;
                        break;
                    }
                }
                if (b) break;
            }
            InnerWorkings.SendMessage($"#{x};{y};{SteamTest.team};{SteamTest.selfIdentity.GetSteamID64()}", GetHost());
        }

        public static void BroadcastCurrentBoardState()
        {
            foreach (var player in ConnectedPlayers)
            {
                InnerWorkings.SendMessage("B" + BingoHooks.GlobalBoard.GetBingoState(), player);
            }
        }
    }
}
