using System;
using HUD;
using Expedition;
using System.Collections.Generic;
using Steamworks;
using BingoMode.BingoSteamworks;

namespace BingoMode.Challenges
{
    public abstract class BingoChallenge : Challenge
    {
        public abstract void AddHooks();
        public abstract void RemoveHooks();
        public abstract List<object> Settings();
        public bool RequireSave = true;
        public bool Failed;
        public bool[] TeamsCompleted = new bool[8];
        public ulong completeCredit = 0;
        public virtual Phrase ConstructPhrase() => null;
        public event Action DescriptionUpdated;
        public event Action ChallengeCompleted;

        public override void UpdateDescription()
        {
            base.UpdateDescription();
            DescriptionUpdated?.Invoke();
        }

        public override void CompleteChallenge()
        {
            if (completed) return;
            if (hidden) return; // Hidden means locked out here in bingo

            if (SteamTest.LobbyMembers.Count > 0 && completeCredit != default)
            {
                goto compleple;
            }

            if (RequireSave && !revealed) // I forgot what this does (i remembered)
            {
                revealed = true;
                return;
            }
            
            if (SteamTest.LobbyMembers.Count > 0)
            {
                SteamTest.BroadcastCompletedChallenge(this);
                TeamsCompleted[SteamTest.team] = true;
            }
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
            if (this.game != null && this.game.cameras != null && this.game.cameras[0].hud != null)
            {
                this.UpdateDescription();
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
        }
    }
}
