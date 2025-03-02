using BingoMode.BingoSteamworks;
using Expedition;
using System;
using System.Collections.Generic;

namespace BingoMode.BingoChallenges
{
    using BingoMenu;

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
                else if (!ReverseChallenge() && TeamsCompleted[i] && BingoData.BingoSaves.TryGetValue(ExpeditionData.slugcatPlayer, out var saveData) && saveData.gamemode == BingoData.BingoGameMode.Lockout)
                {
                    hidden = true;
                }
            }
        }

        public override void CompleteChallenge()
        {
            //Plugin.logger.LogWarning(Environment.StackTrace);
            // Singleplayer
            if (SteamFinal.GetHost().GetSteamID64() == default)
            {
                if (RequireSave() && !revealed)
                {
                    revealed = true;
                    Plugin.logger.LogMessage($"Challenge {this} requires saving to complete!");
                    ChallengeAlmostComplete?.Invoke(SteamTest.team);
                    return;
                }

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
                    Plugin.logger.LogMessage($"Challenge {this} requires saving to complete!");
                    ChallengeAlmostComplete?.Invoke(SteamTest.team);
                    return;
                }

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
                    Plugin.logger.LogMessage($"Challenge {this} requires saving to complete!");
                    ChallengeAlmostComplete?.Invoke(SteamTest.team);
                    return;
                }

                SteamFinal.ChallengeStateChangeToHost(this, false);
                return;
            }
            /*
            if (SteamTest.LobbyMembers.Count > 0 && completeCredit != default)
            {
                Plugin.logger.LogMessage("complete credit isnt null " + completeCredit);
                goto compleple;
            }

            if (RequireSave() && !revealed) // I forgot what this does (i remembered)
            {
                revealed = true;
                Plugin.logger.LogMessage($"Challenge {this} requires saving to complete!");
                ChallengeAlmostComplete?.Invoke();
                return;
            }

            //Plugin.logger.LogMessage($"Current lobby member count in the thing: {SteamTest.LobbyMembers.Count}");
            //foreach (var gg in SteamTest.LobbyMembers)
            //{
            //    Plugin.logger.LogMessage($"- {gg.GetSteamID64()}");
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
            Plugin.logger.LogMessage($"Completing challenge for {BingoPage.TeamName(team)}: {this}");
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
                            c.GetChallenge();
                        }
                    }
                }
            }
            if (team == SteamTest.team && this is BingoUnlockChallenge uch && BingoData.challengeTokens.Contains(uch.unlock.Value)) BingoData.challengeTokens.Remove(uch.unlock.Value);

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
            Plugin.logger.LogMessage($"Failing challenge for {BingoPage.TeamName(team)}: {this}");

            if (team == SteamTest.team)
            {
                completed = false;
            }
            TeamsFailed[team] = true;
            TeamsCompleted[team] = false;

            UpdateDescription();

            ChallengeFailed?.Invoke(team);
            BingoSaveFile.Save();
        }

        public void OnChallengeLockedOut(int team)
        {
            bool lastHidden = hidden;
            if (team == SteamTest.team || TeamsCompleted[SteamTest.team] || hidden) return;
            hidden = true;
            TeamsCompleted[team] = true;
            if (!lastHidden) ChallengeLockedOut?.Invoke(team);
            BingoSaveFile.Save();
        }

        public void OnChallengeDepleted(int team)
        {
            bool lastCompleted = TeamsCompleted[team];
            TeamsCompleted[team] = false;
            if (team == SteamTest.team) completed = false;
            if (this is BingoUnlockChallenge uch && !BingoData.challengeTokens.Contains(uch.unlock.Value)) BingoData.challengeTokens.Add(uch.unlock.Value);
            if (hidden && team != SteamTest.team) hidden = false;
            if (lastCompleted)
            {
                ChallengeDepleted?.Invoke(team);
            }
            UpdateDescription();
            BingoSaveFile.Save();
        }

        public void ChangeValue()
        {
            ValueChanged?.Invoke();
        }

        public override void UpdateDescription()
        {
        }
    }
}
