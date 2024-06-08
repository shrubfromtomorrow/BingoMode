using BingoMode.Challenges;
using Expedition;
using RWCustom;
using Steamworks;
using System;
using System.Runtime.InteropServices;

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

        public static void ConfirmMessage(int m, SteamNetworkingIdentity receiver)
        {
            SendMessage("c" + m, receiver);
        }

        // Data format: "xdata1;data2;..dataN"
        // x - type of data we want to interpret
        // the rest - the actual data we want, separated with semicolons if needed
        public static void MessageReceived(string message)
        {
            char type = message[0];
            message = message.Substring(1);
            Plugin.logger.LogMessage("MESSAGE TYPE IS " + type + " " + (type == '!'));
            string[] data = message.Split(';');
            switch (type)
            {
                // Complete a challenge on the bingo board, based on given int coordinates
                case '#':
                    //if (data.Length != 4)
                    //{
                    //    Plugin.logger.LogError("INVALID LENGTH OF REQUESTED MESSAGE: " + message);
                    //    break;
                    //}

                    int x = int.Parse(data[0], System.Globalization.NumberStyles.Any);
                    int y = int.Parse(data[1], System.Globalization.NumberStyles.Any);
                    int teamCredit = int.Parse(data[2], System.Globalization.NumberStyles.Any);
                    ulong playerCredit = ulong.Parse(data[3], System.Globalization.NumberStyles.Any);

                    if (x != -1 && y != -1)
                    {
                        Plugin.logger.LogMessage($"Completing online challenge at {x}, {y}");
                        (BingoHooks.GlobalBoard.challengeGrid[x, y] as BingoChallenge).OnChallengeCompleted(teamCredit);

                        SteamFinal.BroadcastCurrentBoardState();

                        //(BingoHooks.GlobalBoard.challengeGrid[x, y] as BingoChallenge).completeCredit = playerCredit;
                        //if (teamCredit != SteamTest.team)
                        //{
                        //    if (BingoData.globalSettings.lockout) (BingoHooks.GlobalBoard.challengeGrid[x, y] as BingoChallenge).LockoutChallenge();
                        //    else BingoHooks.GlobalBoard.challengeGrid[x, y].CompleteChallenge();
                        //}
                        //else
                        //{
                        //    BingoHooks.GlobalBoard.challengeGrid[x, y].CompleteChallenge();
                        //}
                        //(BingoHooks.GlobalBoard.challengeGrid[x, y] as BingoChallenge).completeCredit = default;
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
                    SteamMatchmaking.SetLobbyMemberData(SteamTest.CurrentLobby, "playerTeam", t.ToString());

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
                        game.ExitGame(false, true);
                        game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                    }
                    break;

                // End game ! !!! ! ! ! !!
                case 'x':
                    Custom.rainWorld.processManager.ShowDialog(new InfoDialog(Custom.rainWorld.processManager, "The host quit the game."));
                    break;

                // Receive new bingo state
                case 'B':
                    BingoHooks.GlobalBoard.InterpretBingoState(message);
                    break;

                // Receive upkeep request
                case 'U':
                    if (BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer) && BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID.GetSteamID64() != default)
                    {
                        SendMessage("C" + SteamTest.selfIdentity.GetSteamID64(), BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID);
                        SteamFinal.ReceivedHostUpKeep = true;
                    }
                    break;

                // Confirm upkeep
                case 'C':
                    ulong playerID = ulong.Parse(message, System.Globalization.NumberStyles.Any);
                    if (SteamFinal.ReceivedPlayerUpKeep.ContainsKey(playerID)) SteamFinal.ReceivedPlayerUpKeep[playerID] = true;
                    break;

                default:
                    Plugin.logger.LogError("INVALID MESSAGE: " + message);
                    break;
            }
        }
    }
}
