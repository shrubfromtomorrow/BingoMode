using System;
using HUD;
using Expedition;
using System.Collections.Generic;
using Steamworks;

namespace BingoMode.Challenges
{
    public abstract class BingoChallenge : Challenge
    {
        public abstract void AddHooks();
        public abstract void RemoveHooks();
        public abstract List<object> Settings();
        public bool RequireSave;
        public bool Failed;
        public bool[] TeamsCompleted = new bool[4];
        public CSteamID completeCredit;
    }
}
