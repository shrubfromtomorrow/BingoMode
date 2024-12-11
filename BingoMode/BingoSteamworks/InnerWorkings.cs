using BingoMode.Challenges;
using Expedition;
using RWCustom;
using Steamworks;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace BingoMode.BingoSteamworks
{
    internal class InnerWorkings
    {
        public static void SendMessage(string data, SteamNetworkingIdentity receiver, bool reliable = true)
        {
            if (receiver.GetSteamID() == SteamTest.selfIdentity.GetSteamID()) return;
            IntPtr ptr = Marshal.StringToHGlobalAuto(data);
            Plugin.logger.LogMessage("SENDING: " + Marshal.PtrToStringAuto(ptr) + " " + (uint)(data.Length * sizeof(char)));

            if (SteamNetworkingMessages.SendMessageToUser(ref receiver, ptr, (uint)(data.Length * sizeof(char)), reliable ? 40 : 32, 0) != EResult.k_EResultOK)
            {
                Plugin.logger.LogMessage("FAILED TO SEND MESSAGE \"" + data + "\" TO USER " + receiver.GetSteamID());
            }

            Marshal.FreeHGlobal(ptr);
        }

        // Data format: "xdata1;data2;..dataN"
        // x - type of data we want to interpret
        // the rest - the actual data we want, separated with semicolons if needed
        public static void MessageReceived(string message)
        {
            char type = message[0];
            message = message.Substring(1);
            string[] data = message.Split(';');
            switch (type)
            {
                // Complete a challenge on the bingo board, based on given int coordinates
                case '#':
                    if (data.Length != 4)
                    {
                        Plugin.logger.LogError("INVALID LENGTH OF REQUESTED MESSAGE: " + message);
                        break;
                    }

                    int x = int.Parse(data[0], System.Globalization.NumberStyles.Any);
                    int y = int.Parse(data[1], System.Globalization.NumberStyles.Any);
                    int teamCredit = int.Parse(data[2], System.Globalization.NumberStyles.Any);
                    ulong playerCredit = ulong.Parse(data[3], System.Globalization.NumberStyles.Any);

                    if (x != -1 && y != -1)
                    {
                        Plugin.logger.LogMessage($"Completing online challenge at {x}, {y}");
                        if (SteamTest.team != 8 &&
                            BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer) &&
                            BingoData.BingoSaves[ExpeditionData.slugcatPlayer].lockout && 
                            teamCredit != SteamTest.team)
                        {
                            (BingoHooks.GlobalBoard.challengeGrid[x, y] as BingoChallenge).OnChallengeLockedOut(teamCredit);
                        }
                        else (BingoHooks.GlobalBoard.challengeGrid[x, y] as BingoChallenge).OnChallengeCompleted(teamCredit);

                        SteamFinal.BroadcastCurrentBoardState();
                        break;
                    }
                    else
                    {
                        Plugin.logger.LogError("COULDNT: " + message);
                        break;
                    }

                // Fail a challenge on the bingo board, based on given int coordinates
                case '^':
                    if (data.Length != 4)
                    {
                        Plugin.logger.LogError("INVALID LENGTH OF REQUESTED MESSAGE: " + message);
                        break;
                    }

                    int xx = int.Parse(data[0], System.Globalization.NumberStyles.Any);
                    int yy = int.Parse(data[1], System.Globalization.NumberStyles.Any);
                    int teamCredit2 = int.Parse(data[2], System.Globalization.NumberStyles.Any);
                    ulong playerCredit2 = ulong.Parse(data[3], System.Globalization.NumberStyles.Any);
                    if (xx != -1 && yy != -1)
                    {
                        Plugin.logger.LogMessage($"Failing online challenge at {xx}, {yy}");
                        //(BingoHooks.GlobalBoard.challengeGrid[xx, yy] as BingoChallenge).TeamsCompleted[teamCredit2] = false;
                        //(BingoHooks.GlobalBoard.challengeGrid[xx, yy] as BingoChallenge).completeCredit = playerCredit2;
                        (BingoHooks.GlobalBoard.challengeGrid[xx, yy] as BingoChallenge).OnChallengeFailed(teamCredit2);
                        //(BingoHooks.GlobalBoard.challengeGrid[xx, yy] as BingoChallenge).completeCredit = default;

                        SteamFinal.BroadcastCurrentBoardState();

                        break;
                    }
                    else
                    {
                        Plugin.logger.LogError("COULDNT PARSE INTEGERS OF REQUESTED MESSAGE: " + message);
                        break;
                    }

                // Change team
                case '%':
                    int t = int.Parse(data[0], System.Globalization.NumberStyles.Any);

                    SteamTest.team = t;
                    SteamMatchmaking.SetLobbyMemberData(SteamTest.CurrentLobby, "playerTeam", data[0]);

                    if (BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page33) && page33.inLobby)
                    {
                        page33.ResetPlayerLobby();
                    }
                    break;

                // Kick behavior
                case '@':
                    if (SteamTest.CurrentLobby == default) break;

                    if (BingoData.globalMenu != null && BingoHooks.bingoPage.TryGetValue(BingoData.globalMenu, out var page6) && page6.inLobby)
                    {
                        page6.multiButton.buttonBehav.greyedOut = false;
                        page6.Singal(page6.multiButton, "LEAVE_LOBBY");
                        Custom.rainWorld.processManager.ShowDialog(new InfoDialog(Custom.rainWorld.processManager, "You've been kicked from the lobby."));
                    }
                    break;

                // Exit to menu
                case 'e':
                    if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game)
                    {
                        if (game.manager.musicPlayer != null)
                        {
                            game.manager.musicPlayer.DeathEvent();
                        }
                        game.ExitGame(false, false);
                        game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                    }
                    break;

                // Finish game request from host
                case 'x':
                    //ulong hostID = ulong.Parse(message, System.Globalization.NumberStyles.Any);
                    //SteamNetworkingIdentity hostIdentity = new SteamNetworkingIdentity();
                    //hostIdentity.SetSteamID64(hostID);
                    //SendMessage("f;" + SteamTest.selfIdentity.GetSteamID64(), hostIdentity);
                    if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game2)
                    {
                        if (game2.manager.musicPlayer != null)
                        {
                            game2.manager.musicPlayer.DeathEvent();
                        }
                        game2.ExitGame(false, false);
                    }
                    Custom.rainWorld.processManager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                    Custom.rainWorld.processManager.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer); 
                    BingoData.FinishBingo();
                    break;

                //case 'f':
                //    ulong playerID = ulong.Parse(message, System.Globalization.NumberStyles.Any);
                //    SteamNetworkingIdentity playerIdentity = new SteamNetworkingIdentity();
                //    playerIdentity.SetSteamID64(playerID);
                //    break;

                // Receive new bingo state
                case 'B':
                    BingoHooks.GlobalBoard.InterpretBingoState(message);
                    break;

                // Receive upkeep request
                case 'U':
                    if (BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer) && BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID.GetSteamID64() != default && BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID.GetSteamID64() != SteamTest.selfIdentity.GetSteamID64())
                    {
                        SendMessage("C" + SteamTest.selfIdentity.GetSteamID64(), BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID);
                        SteamFinal.ReceivedHostUpKeep = true;
                    }
                    break;

                // Confirm upkeep
                case 'C':
                    if (SteamFinal.TryToReconnect)
                    {
                        if (data.Length == 2 && BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer) && BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID.GetSteamID64().ToString() == data[0])
                        {
                            Plugin.logger.LogMessage("Reconnected to host " + data[0] + "!");
                            SteamFinal.ReceivedHostUpKeep = true;
                            SteamFinal.HostUpkeep = SteamFinal.MaxHostUpKeepTime;
                            Plugin.logger.LogMessage($"Got new bingo board state!");
                            BingoHooks.GlobalBoard.InterpretBingoState(data[1]);
                        } 
                        else
                        {
                            Plugin.logger.LogError("INVALID LENGTH OF DATA IN C" + message);
                        }
                        break;
                    }
                    string g = message;
                    if (data.Length == 2) g = data[0];
                    ulong playerID = ulong.Parse(g, System.Globalization.NumberStyles.Any);
                    if (SteamFinal.ReceivedPlayerUpKeep.ContainsKey(playerID)) SteamFinal.ReceivedPlayerUpKeep[playerID] = true;

                    if (BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer) && 
                        BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID.GetSteamID64().ToString() == data[0] &&
                        BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID.GetSteamID64() != SteamTest.selfIdentity.GetSteamID64())
                    {
                        SteamFinal.ReceivedHostUpKeep = true;
                    }
                    break;

                // Host upkeep request
                case 'H':
                    ulong requesterID = ulong.Parse(message, System.Globalization.NumberStyles.Any);
                    if (BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer) && BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID.GetSteamID64() != default && BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID.GetSteamID64() == SteamTest.selfIdentity.GetSteamID64())
                    {
                        SteamNetworkingIdentity requesterIdentity = new SteamNetworkingIdentity();
                        requesterIdentity.SetSteamID64(requesterID);
                        SendMessage("C" + SteamTest.selfIdentity.GetSteamID64() + ";" + BingoHooks.GlobalBoard.GetBingoState().Replace('3', '1'), requesterIdentity);
                        var playerwhitelist = SteamFinal.PlayersFromString(BingoData.BingoSaves[ExpeditionData.slugcatPlayer].playerWhiteList);
                        Plugin.logger.LogMessage("The guy who contacted me: " + requesterID);
                        foreach (var gruh in playerwhitelist)
                        {
                            Plugin.logger.LogMessage("Player white list: " + gruh.GetSteamID64());
                        }
                        foreach (var gruh in SteamFinal.ConnectedPlayers)
                        {
                            Plugin.logger.LogMessage("Connected player: " + gruh.GetSteamID64());
                        }
                        if (!SteamFinal.ConnectedPlayers.Any(x => x.GetSteamID64() == requesterID) && playerwhitelist.Any(x => x.GetSteamID64() == requesterID))
                        {
                            Plugin.logger.LogMessage($"Adding player {requesterID} back to the game!");
                            SteamFinal.ConnectedPlayers.Add(requesterIdentity);
                        }
                        SteamFinal.ReceivedPlayerUpKeep[requesterID] = true;
                    }
                    break;

                // Leave lobby
                case 'L':
                    SteamTest.LeaveLobby();
                    break;

                default:
                    Plugin.logger.LogWarning("INVALID MESSAGE: " + message);
                    break;
            }
        }
    }
}
