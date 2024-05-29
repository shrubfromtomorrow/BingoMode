using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using RWCustom;
using Steamworks;
using BingoMode.Challenges;

namespace BingoMode.BingoSteamworks
{
    internal class InnerWorkings
    {
        public static void SendMessage(string data, SteamNetworkingIdentity receiver, bool reliable = true)
        {
            IntPtr ptr = Marshal.StringToHGlobalAuto(data);
            Plugin.logger.LogMessage("TEST: " + Marshal.PtrToStringAuto(ptr) + " " + (uint)(data.Length * sizeof(char)));

            if (SteamNetworkingMessages.SendMessageToUser(ref receiver, ptr, (uint)(data.Length * sizeof(char)), reliable ? 40 : 32, 0) != EResult.k_EResultOK)
            {
                Plugin.logger.LogMessage("FAILED TO SEND MESSAGE \"" + data + "\" TO USER " + receiver.GetSteamID());
            }

            Marshal.FreeHGlobal(ptr);
        }

        // Data format: "xdata1;data2;..dataN"
        // x - type of data we want to interpret
        // the rest - the actual data we want, separated with semicolons if needed
        public static bool MessageReceived(string message)
        {
            char type = message[0];
            //message = message.Substring(1);
            Plugin.logger.LogMessage("MESSAGE TYPE IS " + type + " " + (type == '!'));
            string[] data = message.Split(';');
            switch (type)
            {
                // Complete a challenge on the bingo board, based on given int coordinates
                case '#':
                    if (data.Length != 5)
                    {
                        Plugin.logger.LogError("INVALID LENGTH OF REQUESTED MESSAGE: " + message);
                        return false;
                    }

                    if (int.TryParse(data[1], out int x) && x != -1 && int.TryParse(data[2], out int y) && y != -1 && int.TryParse(data[3], out int teamCredit) && ulong.TryParse(data[4], out ulong playerCredit))
                    {
                        Plugin.logger.LogMessage($"Completing online challenge at {x}, {y}");
                        (BingoHooks.GlobalBoard.challengeGrid[x, y] as BingoChallenge).TeamsCompleted[teamCredit] = true;
                        (BingoHooks.GlobalBoard.challengeGrid[x, y] as BingoChallenge).completeCredit = playerCredit;
                        if (teamCredit != SteamTest.team)
                        {
                            if (BingoData.globalSettings.lockout) (BingoHooks.GlobalBoard.challengeGrid[x, y] as BingoChallenge).LockoutChallenge();
                            else BingoHooks.GlobalBoard.challengeGrid[x, y].CompleteChallenge();
                        }
                        else
                        {
                            BingoHooks.GlobalBoard.challengeGrid[x, y].CompleteChallenge();
                        }
                        (BingoHooks.GlobalBoard.challengeGrid[x, y] as BingoChallenge).completeCredit = default;
                        return true;
                    }
                    else
                    {
                        Plugin.logger.LogError("COULDNT PARSE INTEGERS OF REQUESTED MESSAGE: " + message);
                        return false;
                    }

                // Fail a challenge on the bingo board, based on given int coordinates
                case '^':
                    if (data.Length != 5)
                    {
                        Plugin.logger.LogError("INVALID LENGTH OF REQUESTED MESSAGE: " + message);
                        return false;
                    }

                    if (int.TryParse(data[1], out int xx) && xx != -1 && int.TryParse(data[2], out int yy) && yy != -1 && int.TryParse(data[3], out int teamCredit2) && ulong.TryParse(data[4], out ulong playerCredit2))
                    {
                        Plugin.logger.LogMessage($"Completing online challenge at {xx}, {yy}");
                        //(BingoHooks.GlobalBoard.challengeGrid[xx, yy] as BingoChallenge).TeamsCompleted[teamCredit2] = false;
                        (BingoHooks.GlobalBoard.challengeGrid[xx, yy] as BingoChallenge).completeCredit = playerCredit2;
                        (BingoHooks.GlobalBoard.challengeGrid[xx, yy] as BingoChallenge).FailChallenge(teamCredit2);
                        (BingoHooks.GlobalBoard.challengeGrid[xx, yy] as BingoChallenge).completeCredit = default;
                        //if (teamCredit2 != SteamTest.team)
                        //{
                        //    if (BingoData.globalSettings.lockout) (BingoHooks.GlobalBoard.challengeGrid[xx, yy] as BingoChallenge).LockoutChallenge();
                        //    else BingoHooks.GlobalBoard.challengeGrid[xx, yy].CompleteChallenge();
                        //}
                        //else
                        //{
                        //    BingoHooks.GlobalBoard.challengeGrid[xx, yy].CompleteChallenge();
                        //}

                        return true;
                    }
                    else
                    {
                        Plugin.logger.LogError("COULDNT PARSE INTEGERS OF REQUESTED MESSAGE: " + message);
                        return false;
                    }

                // Update board
                //case '*':
                //    string challenjes = SteamMatchmaking.GetLobbyData(SteamTest.CurrentLobby, "challenges");
                //    try
                //    {
                //        BingoHooks.GlobalBoard.FromString(challenjes);
                //        return true;
                //    }
                //    catch (Exception e)
                //    {
                //        Plugin.logger.LogError(e + "\nFAILED TO RECREATE BINGO BOARD FROM STRING FROM LOBBY: " + challenjes);
                //        SteamTest.LeaveLobby();
                //    }
                //    return false;

                // Begin game
                case '!':
                    if (data.Length != 2)
                    {
                        Plugin.logger.LogError("INVALID LENGTH OF REQUESTED MESSAGE: " + message);
                        return false;
                    }
                    if (SteamTest.selfIdentity.GetSteamID() == SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby) && BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page))
                    {
                        BingoData.BingoDen = data[1];
                        page.startGame.buttonBehav.greyedOut = false;
                        page.startGame.Clicked();
                        SteamTest.LeaveLobby();
                        return true;
                    }
                    return false;

                // Change team
                case '%':
                    if (data.Length != 2)
                    {
                        Plugin.logger.LogError("INVALID LENGTH OF REQUESTED MESSAGE: " + message);
                        return false;
                    }

                    if (int.TryParse(data[1], out int t))
                    {
                        SteamTest.team = t;
                        SteamMatchmaking.SetLobbyMemberData(SteamTest.CurrentLobby, "playerTeam", t.ToString());
                        return true;
                    }
                    else
                    {
                        Plugin.logger.LogError("COULDNT PARSE INTEGERS OF REQUESTED MESSAGE: " + message + " DATA: " + data[1]);
                        return false;
                    }

                // Kick behavior
                case '@':
                    if (SteamTest.CurrentLobby == default) return false;
                    SteamTest.LeaveLobby();
                    Custom.rainWorld.processManager.ShowDialog(new InfoDialog(Custom.rainWorld.processManager, "You've been kicked from the lobby."));
                    return true;

                // Force host burdens
                case 'b':
                    if (data.Length != 2)
                    {
                        Plugin.logger.LogError("INVALID LENGTH OF REQUESTED MESSAGE: " + message);
                        return false;
                    }
                    List<string> burjs = [];
                    for (int i = 1; i < data.Length; i++)
                    {
                        burjs.Add(data[1]);
                    }
                    SteamTest.FetchUnlocks(burjs, true);
                    return true;

                // Force host perks
                case 'p':
                    if (data.Length != 2)
                    {
                        Plugin.logger.LogError("INVALID LENGTH OF REQUESTED MESSAGE: " + message);
                        return false;
                    }
                    List<string> perj = [];
                    for (int i = 1; i < data.Length; i++)
                    {
                        perj.Add(data[1]);
                    }
                    SteamTest.FetchUnlocks(perj, false);
                    return true;
            }

            return false;
        }
    }
}
