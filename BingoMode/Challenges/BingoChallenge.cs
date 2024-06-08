using System;
using HUD;
using Expedition;
using System.Collections.Generic;
using Steamworks;
using BingoMode.BingoSteamworks;
using System.Linq;

namespace BingoMode.Challenges
{
    public abstract class BingoChallenge : Challenge
    {
        public abstract void AddHooks();
        public abstract void RemoveHooks();
        public abstract List<object> Settings();
        //public bool RequireSave = true;
        public bool Failed;
        public bool[] TeamsCompleted = new bool[9];
        public ulong completeCredit = 0;
        public virtual Phrase ConstructPhrase() => null;
        public event Action DescriptionUpdated;
        public event Action<int> ChallengeCompleted;
        public event Action<int> ChallengeFailed;
        public event Action ChallengeAlmostComplete;
        public event Action ChallengeLockedOut;

        public virtual bool ReverseChallenge() => false;
        public virtual bool RequireSave() => true;

        public override void UpdateDescription()
        {
            base.UpdateDescription();
            DescriptionUpdated?.Invoke();
        }

        public string TeamsToString()
        {
            string data = "";
            foreach (bool t in TeamsCompleted) data += t ? "1" : 0;
            return data;
        }

        public void TeamsFromString(string data)
        {
            if (TeamsCompleted.Length != data.Length) return;
            for (int i = 0; i < data.Length; i++)
            {
                TeamsCompleted[i] = data[i] == '1';
            }
        }

        public void FailChallenge(int team)
        {
            if (team == SteamTest.team)
            {
                Failed = true;
                completed = false;
            }
            TeamsCompleted[team] = false;
            if (SteamTest.LobbyMembers.Count > 0 && completeCredit == default)
            {
                SteamTest.BroadcastFailedChallenge(this);
            }
            Expedition.Expedition.coreFile.Save(false);
            ChallengeFailed?.Invoke(team);
        }

        public void OnChallengeLockedOut()
        {
            if (completed) return;
            hidden = true;
            ChallengeLockedOut?.Invoke();
            CheckWinLose();
        }

        public override void CompleteChallenge()
        {
            // Singleplayer
            if (SteamFinal.GetHost().GetSteamID64() == default)
            {
                if (RequireSave() && !revealed)
                {
                    revealed = true;
                    Plugin.logger.LogMessage($"Challenge {this} requires saving to complete!");
                    ChallengeAlmostComplete?.Invoke();
                    return;
                }

                OnChallengeCompleted(SteamTest.team);

                //if (ExpeditionGame.activeUnlocks.Contains("unl-passage"))
                //{
                //    ExpeditionData.earnedPassages++;
                //}

                CheckWinLose();
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
                    ChallengeAlmostComplete?.Invoke();
                    return;
                }

                OnChallengeCompleted(SteamTest.team);

                //if (ExpeditionGame.activeUnlocks.Contains("unl-passage"))
                //{
                //    ExpeditionData.earnedPassages++;
                //}

                SteamFinal.BroadcastCurrentBoardState();
                CheckWinLose();

                return;
            }
            else // If regular player
            {
                if (RequireSave() && !revealed)
                {
                    revealed = true;
                    Plugin.logger.LogMessage($"Challenge {this} requires saving to complete!");
                    ChallengeAlmostComplete?.Invoke();
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
            TeamsCompleted[team] = true;
            if (TeamsCompleted[SteamTest.team]) completed = true;
            UpdateDescription();
            ChallengeCompleted?.Invoke(team);
            Expedition.Expedition.coreFile.Save(false);
            CheckWinLose();
        }

        public void OnChallengeFailed(int team) // failing challenges to do
        {
            ChallengeFailed?.Invoke(team);
        }

        public void CheckWinLose()
        {
            int teamsLost = 0;
            for (int t = 0; t < 8; t++)
            {
                if (!BingoHooks.GlobalBoard.CheckWin(SteamTest.team, true)) teamsLost++;
            }
            bool allChallengesDone = true;
            for (int i = 0; i < BingoHooks.GlobalBoard.size; i++)
            {
                if (!allChallengesDone) break;
                for (int j = 0; j < BingoHooks.GlobalBoard.size; j++)
                {
                    if (!(BingoHooks.GlobalBoard.challengeGrid[i, j] as BingoChallenge).TeamsCompleted.Any(x => x == true))
                    {
                        allChallengesDone = false;
                        break;
                    }
                }
            }
            if (teamsLost == BingoData.TeamsInBingo && allChallengesDone) // Noone can complete bingo anymore, game ending, stats on who got the most tiles
            {
                game.manager.RequestMainProcessSwitch(BingoEnums.BingoLoseScreen);
                game.manager.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
                if (BingoData.BingoSaves != null && BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer)) BingoData.BingoSaves.Remove(ExpeditionData.slugcatPlayer);
                return;
            }

            for (int t = 0; t < 8; t++)
            {
                if (BingoHooks.GlobalBoard.CheckWin(t, false))
                {
                    Plugin.logger.LogMessage($"Team {t} won!");
                    game.manager.RequestMainProcessSwitch(BingoEnums.BingoWinScreen);
                    game.manager.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
                    if (BingoData.BingoSaves != null && BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer)) BingoData.BingoSaves.Remove(ExpeditionData.slugcatPlayer);
                    return;
                }
            }
        }
    }
}
