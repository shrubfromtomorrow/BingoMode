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
        public Challenge[,] challengeGrid; // The challenges will be treated as coordinates on a grid for convenience
        public int size;

        public BingoBoard()
        {
            size = 5;
            GenerateBoard(size);
        }

        public void GenerateBoard(int size)
        {
            challengeGrid = new Challenge[size, size];
            FillGrid();
        }

        public void FillGrid()
        {
            for (int i = 0; i < challengeGrid.GetLength(0); i++)
            {
                for (int j = 0; j < challengeGrid.GetLength(1); j++)
                {
                    challengeGrid[i, j] = ChallengeOrganizer.RandomChallenge(false);
                }
            }
            UpdateChallenges();
        }

        public void UpdateChallenges()
        {
            foreach (Challenge c in AllChallengeList())
            {
                c.UpdateDescription();
            }
        }

        public List<Challenge> AllChallengeList()
        {
            List<Challenge> chacha = new();

            for (int i = 0; i < challengeGrid.GetLength(0); i++)
            {
                for (int j = 0; j < challengeGrid.GetLength(1); j++)
                {
                    chacha.Add(challengeGrid[i, j]);
                }
            }

            return chacha;
        }

        public void SetChallenge(int x, int y, Challenge newChallenge)
        {
            try
            {
                challengeGrid[x, y] = newChallenge;
            }
            catch
            {
                Plugin.logger.LogError("Invalid bingo board coordinates :(");
            }
        }

        public Challenge GetChallenge(int x, int y)
        {
            if (x < challengeGrid.GetLength(0) && y < challengeGrid.GetLength(1)) return challengeGrid[x, y];
            return null;
        }

        public class BingoChallenge
        {
            public BingoBoard board;
            public string text;

            public BingoChallenge(BingoBoard board)
            {
                this.board = board;
                text = "test";
            }
        }
    }
}
