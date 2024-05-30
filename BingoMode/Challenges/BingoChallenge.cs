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
        public bool RequireSave = true;
        public bool Failed;
        public bool[] TeamsCompleted = new bool[9];
        public ulong completeCredit = 0;
        public virtual Phrase ConstructPhrase() => null;
        public event Action DescriptionUpdated;
        public event Action ChallengeCompleted;
        public event Action ChallengeFailed;
        public event Action ChallengeAlmostComplete;
        public event Action ChallengeLockedOut;
        public bool ReverseChallenge;

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
            Failed = true;
            completed = false;
            TeamsCompleted[team] = false;
            if (SteamTest.LobbyMembers.Count > 0)
            {
                SteamTest.BroadcastFailedChallenge(this);
            }
            Expedition.Expedition.coreFile.Save(false);
            ChallengeFailed?.Invoke();
        }

        public void LockoutChallenge()
        {
            if (completed) return;
            hidden = true;
            ChallengeLockedOut?.Invoke();
            CheckWinLose();
        }

        public override void CompleteChallenge()
        {
            if (hidden) return; // Hidden means locked out here in bingo

            if (SteamTest.LobbyMembers.Count > 0 && completeCredit != default)
            {
                Plugin.logger.LogMessage("complete credit isnt null " + completeCredit);
                goto compleple;
            }

            if (RequireSave && !revealed) // I forgot what this does (i remembered)
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
            //int num = 0;
            //bool flag = true;
            //foreach (Challenge challenge in ExpeditionData.challengeList)
            //{
            //    if (!challenge.hidden && !challenge.completed)
            //    {
            //        flag = false;
            //    }
            //    else if (challenge.hidden && !challenge.revealed)
            //    {
            //        num++;
            //    }
            //}
            UpdateDescription();
            if (this.game != null && this.game.cameras != null && this.game.cameras[0].hud != null)
            {
                for (int i = 0; i < this.game.cameras[0].hud.parts.Count; i++)
                {
                    //if (this.game.cameras[0].hud.parts[i] is ExpeditionHUD)
                    //{
                    //    (this.game.cameras[0].hud.parts[i] as ExpeditionHUD).completeMode = true;
                    //    (this.game.cameras[0].hud.parts[i] as ExpeditionHUD).challengesToComplete++;
                    //    if (flag)
                    //    {
                    //        (this.game.cameras[0].hud.parts[i] as ExpeditionHUD).challengesToReveal = num;
                    //        (this.game.cameras[0].hud.parts[i] as ExpeditionHUD).revealMode = true;
                    //    }
                    //}
                }
            }
            //if (ExpeditionGame.activeUnlocks.Contains("unl-passage"))
            //{
            //    ExpeditionData.earnedPassages++;
            //}
            Expedition.Expedition.coreFile.Save(false);
            ChallengeCompleted?.Invoke();
            CheckWinLose();
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
                    game.manager.RequestMainProcessSwitch(BingoEnums.BingoWinScreen);
                    game.manager.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
                    if (BingoData.BingoSaves != null && BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer)) BingoData.BingoSaves.Remove(ExpeditionData.slugcatPlayer);
                    return;
                }
            }
        }
    }
}
