using System;
using System.Collections.Generic;
using BingoMode.BingoSteamworks;
using Expedition;

namespace BingoMode.BingoChallenges
{
    using System.Linq;
    using System.Text.RegularExpressions;
    using BingoMenu;
    using IL.Watcher;

    public abstract class BingoChallenge : Challenge
    {
        public abstract void AddHooks();
        public abstract void RemoveHooks();
        public abstract List<object> Settings();
        public bool[] TeamsFailed = new bool[9];
        public bool[] TeamsCompleted = new bool[9];
        public ulong completeCredit = 0;
        public virtual Phrase ConstructPhrase() => null;
        public event Action ValueChanged;
        public event Action<int> ChallengeCompleted;
        public event Action<int> ChallengeDepleted;
        public event Action<int> ChallengeFailed;
        public event Action<int> ChallengeAlmostComplete;
        public event Action<int> ChallengeLockedOut;

        public virtual bool ReverseChallenge() => false;
        public virtual bool RequireSave() => true;
        public virtual bool SaveOnDeath() => false;

        public string TeamsToString()
        {
            char[] data = "000000000".ToCharArray();
            for (int t = 0; t < TeamsCompleted.Length; t++)
            {
                if (TeamsFailed[t] == true)
                {
                    data[t] = '2';
                    continue;
                }
                if (TeamsCompleted[t] == true)
                {
                    data[t] = '1';
                    continue;
                }
            }
            return new string(data);
        }

        public void TeamsFromString(string data, int ourTeam)
        {
            if (TeamsCompleted.Length != data.Length) return;
            for (int i = 0; i < data.Length; i++)
            {
                TeamsFailed[i] = data[i] == '2';
                TeamsCompleted[i] = data[i] == '1';
                if (i == ourTeam)
                {
                    if (TeamsCompleted[i]) completed = true;
                }
                else if (!ReverseChallenge() && TeamsCompleted[i] && BingoData.IsCurrentSaveLockout())
                {
                    hidden = true;
                }
            }
        }

        public override void CompleteChallenge()
        {
            //
            // Singleplayer
            if (SteamFinal.GetHost().GetSteamID64() == default)
            {
                if (RequireSave() && !revealed)
                {
                    revealed = true;
                    //Plugin.logger.LogInfo("Singleplayer, challenge completed but not locked in: " + this.ToString());
                    
                    ChallengeAlmostComplete?.Invoke(SteamTest.team);
                    return;
                }

                //Plugin.logger.LogInfo("Singleplayer, challenge locked in: " + this.ToString());
                OnChallengeCompleted(SteamTest.team);

                //if (ExpeditionGame.activeUnlocks.Contains("unl-passage"))
                //{
                //    ExpeditionData.earnedPassages++;
                //}

                //CheckWinLose();
                return;
            }

            // Multiplayer

            if (hidden) return; // Hidden means locked out here in bingo

            // If is host
            if (SteamFinal.GetHost().GetSteamID64() == SteamTest.selfIdentity.GetSteamID64())
            {
                if (RequireSave() && !revealed)
                {
                    revealed = true;
                    //Plugin.logger.LogInfo("Multiplayer HOST, challenge completed but not locked in: " + this.ToString());

                    ChallengeAlmostComplete?.Invoke(SteamTest.team);
                    return;
                }

                //Plugin.logger.LogInfo("Multiplayer HOST, challenge locked in: " + this.ToString());
                OnChallengeCompleted(SteamTest.team);

                //if (ExpeditionGame.activeUnlocks.Contains("unl-passage"))
                //{
                //    ExpeditionData.earnedPassages++;
                //}

                SteamFinal.BroadcastCurrentBoardState();
                //CheckWinLose();

                return;
            }
            else // If regular player
            {
                if (RequireSave() && !revealed)
                {
                    revealed = true;
                    //Plugin.logger.LogInfo("Multiplayer PLAYER, challenge completed but not locked in: " + this.ToString());

                    ChallengeAlmostComplete?.Invoke(SteamTest.team);
                    return;
                }

                //Plugin.logger.LogInfo("Multiplayer PLAYER, challenge locked in: " + this.ToString());
                SteamFinal.ChallengeStateChangeToHost(this, false);
                return;
            }
            /*
            if (SteamTest.LobbyMembers.Count > 0 && completeCredit != default)
            {
                
                goto compleple;
            }

            if (RequireSave() && !revealed) // I forgot what this does (i remembered)
            {
                revealed = true;
                
                ChallengeAlmostComplete?.Invoke();
                return;
            }

            //
            //foreach (var gg in SteamTest.LobbyMembers)
            //{
            //    
            //}
            if (SteamTest.LobbyMembers.Count > 0)
            {
                SteamTest.BroadcastCompletedChallenge(this);
            }
            TeamsCompleted[SteamTest.team] = true;
        compleple:
            if (TeamsCompleted[SteamTest.team]) completed = true;
            UpdateDescription();

            //if (ExpeditionGame.activeUnlocks.Contains("unl-passage"))
            //{
            //    ExpeditionData.earnedPassages++;
            //}

            // Expedition.Expedition.coreFile.Save(false); // Idk when the saving should happen

            //ChallengeCompleted?.Invoke(SteamTest.team);
            //CheckWinLose();
            */
        }

        public void OnChallengeCompleted(int team)
        {
            bool lastCompleted = TeamsCompleted[team];

            TeamsCompleted[team] = true;
            if (TeamsCompleted[SteamTest.team]) completed = true;

            UpdateDescription();

            if (!lastCompleted)
            {
                ChallengeCompleted?.Invoke(team);
                if (team == SteamTest.team)
                {
                    for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                    {
                        if (ExpeditionData.challengeList[j] is BingoHellChallenge c && !ReverseChallenge())
                        {
                            c.GetChallenge(this);
                        }
                    }
                }
            }
            if (team == SteamTest.team && this is BingoUnlockChallenge uch && BingoData.challengeTokens.Contains(uch.unlock.Value))
            {
                BingoData.challengeTokens.Remove(uch.unlock.Value);
            }
            if (team == SteamTest.team && this is BingoBroadcastChallenge brd && BingoData.challengeTokens.Contains(brd.chatlog.Value))
            {
                BingoData.challengeTokens.Remove(brd.chatlog.Value);
            }
            BingoHooks.GlobalBoard.GetCompletableChallenges(SteamTest.team);
            BingoSaveFile.Save();
        }

        public void FailChallenge(int team)
        {
            if (SteamFinal.GetHost().GetSteamID64() == default)
            {
                OnChallengeFailed(SteamTest.team);
                return;
            }

            // If is host
            if (SteamFinal.GetHost().GetSteamID64() == SteamTest.selfIdentity.GetSteamID64())
            {
                OnChallengeFailed(SteamTest.team);

                SteamFinal.BroadcastCurrentBoardState();
                return;
            }
            else // If regular player
            {
                SteamFinal.ChallengeStateChangeToHost(this, true);
                return;
            }
        }

        public void OnChallengeFailed(int team)
        {
            if (team == SteamTest.team)
            {
                completed = false;
            }
            TeamsFailed[team] = true;
            TeamsCompleted[team] = false;

            UpdateDescription();

            ChallengeFailed?.Invoke(team);
            BingoHooks.GlobalBoard.GetCompletableChallenges(SteamTest.team);
            BingoSaveFile.Save();
        }

        public void OnChallengeLockedOut(int team)
        {
            bool lastHidden = hidden;
            if (team == SteamTest.team || TeamsCompleted[SteamTest.team] || hidden) return;
            hidden = true;
            TeamsCompleted[team] = true;
            if (!lastHidden) ChallengeLockedOut?.Invoke(team);
            BingoHooks.GlobalBoard.GetCompletableChallenges(SteamTest.team);
            BingoSaveFile.Save();
        }

        public void OnChallengeDepleted(int team)
        {
            bool lastCompleted = TeamsCompleted[team];
            TeamsCompleted[team] = false;
            if (team == SteamTest.team) completed = false;
            if (team == SteamTest.team && this is BingoUnlockChallenge uch && !BingoData.challengeTokens.Contains(uch.unlock.Value))
            {
                BingoData.challengeTokens.Add(uch.unlock.Value);
            }
            if (team == SteamTest.team && this is BingoBroadcastChallenge brd && !BingoData.challengeTokens.Contains(brd.chatlog.Value))
            {
                BingoData.challengeTokens.Add(brd.chatlog.Value);
            }

            if (hidden && team != SteamTest.team) hidden = false;
            if (lastCompleted)
            {
                ChallengeDepleted?.Invoke(team);
            }
            UpdateDescription();
            BingoHooks.GlobalBoard.GetCompletableChallenges(SteamTest.team);
            BingoSaveFile.Save();
        }

        public void ChangeValue()
        {
            ValueChanged?.Invoke();
        }

        public override void UpdateDescription()
        {
        }

        public static void UpdateHellChallengeOnDeath()
        {
            string board = Expedition.Expedition.coreFile.ToString();
            string[] array = Regex.Split(board, "<expC>");
            bool flag = false;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == "[END CHALLENGES]")
                {
                    flag = false;
                }

                if (flag)
                {
                    string[] array10 = Regex.Split(array[i], "#");
                    if (array10.Length < 2) continue;

                    SlugcatStats.Name name = new SlugcatStats.Name(array10[0], false);
                    if (name == ExpeditionData.slugcatPlayer)
                    {
                        string[] array11 = Regex.Split(array10[1], "~");
                        if (array11.Length < 2) continue;

                        string type = array11[0];
                        string text2 = array11[1];

                        if (type == "BingoHellChallenge")
                        {
                            for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
                            {
                                if (ExpeditionData.challengeList[j] is BingoHellChallenge c)
                                {
                                    array10[1] = $"BingoHellChallenge~0><System.Int32|{c.amount}|Amount|0|NULL><{(c.completed ? '1' : '2')}><{(c.revealed ? '1' : '2')}";
                                    array[i] = array10[0] + "#" + array10[1];
                                }
                            }
                        }
                    }
                }

                if (array[i] == "[CHALLENGES]")
                {
                    flag = true;
                }
            }

            string newBoard = string.Join("<expC>", array);
            Plugin.logger.LogInfo(board);
            Plugin.logger.LogInfo(newBoard);

            Expedition.Expedition.coreFile.FromString(newBoard);
        }
    }
}
