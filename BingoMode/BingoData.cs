using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Expedition;
using Menu;
using Menu.Remix;
using UnityEngine;

namespace BingoMode
{
    public static class BingoData
    {
        public static bool BingoMode = false;

        public static void InitializeBingo()
        {
            BingoMode = true;
        }

        public static void FinishBingo()
        {
            ExpeditionData.ClearActiveChallengeList();
        }
    }
}
