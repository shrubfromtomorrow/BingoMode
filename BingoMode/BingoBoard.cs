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
    public class BingoBoard
    {
        public ExpeditionCoreFile core;
        public BingoChallenge[,] grid; // The challenges will be treated as coordinates on a grid

        public BingoBoard()
        {
            grid = new BingoChallenge[5, 5]; // 5 by 5
        }

        public class BingoChallenge
        {
            public BingoBoard board;
            string text;

            public BingoChallenge(BingoBoard board)
            {
                this.board = board;
                text = "test";
            }
        }
    }
}
