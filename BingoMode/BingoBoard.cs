using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Expedition;
using RWCustom;
using Menu;
using Menu.Remix;
using UnityEngine;

namespace BingoMode
{
    public class BingoBoard
    {
        public ExpeditionCoreFile core;
        public Challenge[,] challengeGrid; // The challenges will be treated as coordinates on a grid for convenience
        public List<IntVector2> currentWinLine; // A list of grid coordinates
        public int size;

        public BingoBoard()
        {
            size = 4;
            GenerateBoard(size);
            currentWinLine = new();
        }

        public void GenerateBoard(int size)
        {
            challengeGrid = new Challenge[size, size];
            ExpeditionData.ClearActiveChallengeList();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    challengeGrid[i, j] = ChallengeOrganizer.RandomChallenge(false);
                    ExpeditionData.challengeList.Add(challengeGrid[i, j]);
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

        public bool CheckWin()
        {
            bool won = false;

            // Vertical lines
            for (int i = 0; i < size; i++)
            {
                bool line = true;
                for (int j = 0; j < size; j++)
                {
                    line &= challengeGrid[i, j].completed;
                    if (line) currentWinLine.Add(new IntVector2(i, j));
                }
                won = line;
                if (won)
                {
                    Plugin.logger.LogMessage("Vertical win");
                    break;
                }
                else currentWinLine.Clear();
            }

            // Horizontal lines
            if (!won)
            {
                for (int i = 0; i < size; i++)
                {
                    bool line = true;
                    for (int j = 0; j < size; j++)
                    {
                        line &= challengeGrid[j, i].completed;
                        if (line) currentWinLine.Add(new IntVector2(j, i));
                    }
                    won = line;
                    if (won)
                    {
                        Plugin.logger.LogMessage("Horizontal win");
                        break;
                    }
                    else currentWinLine.Clear();
                }
            }

            // Diagonal line 1
            if (!won)
            {
                bool line = true;
                for (int i = 0; i < size; i++)
                {
                    line &= challengeGrid[i, i].completed;
                    if (line) currentWinLine.Add(new IntVector2(i, i));
                }
                won = line;
                if (won)
                {
                    Plugin.logger.LogMessage("Diagonal 1 win");
                }
                else currentWinLine.Clear();
            }

            // Diagonal line 2
            if (!won)
            {
                bool line = true;
                for (int i = 0; i < size; i++)
                {
                    line &= challengeGrid[size - 1 - i, i].completed;
                    if (line) currentWinLine.Add(new IntVector2(size - 1 - i, i));
                }
                won = line;
                if (won)
                {
                    Plugin.logger.LogMessage("Diagnoal 2 win");
                }
                else currentWinLine.Clear();
            }

            return won;
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
