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
        //public Challenge[,] ghostGrid;
        public List<IntVector2> currentWinLine; // A list of grid coordinates
        public List<Challenge> AllChallenges;
        public int size;
        public int lastSize;

        public BingoBoard()
        {
            size = 5;
            lastSize = 5;
            GenerateBoard(size);
            currentWinLine = [];
        }

        public void GenerateBoard(int size, bool changeSize = false)
        {
            Plugin.logger.LogMessage("Generating bored");
            if (!changeSize)
            {
                AllChallenges = [];
                BingoData.FillPossibleTokens(ExpeditionData.slugcatPlayer);
                challengeGrid = new Challenge[size, size];
                ExpeditionData.ClearActiveChallengeList();
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (challengeGrid[i, j] != null)
                    {
                        //if (changeSize)
                        //{
                        //    ghostGrid[i, j] = challengeGrid[i, j];
                        //}
                        continue;
                    }
                redo:
                    challengeGrid[i, j] = RandomBingoChallenge();
                    if (challengeGrid[i, j] == null) goto redo;
                    ExpeditionData.challengeList.Add(challengeGrid[i, j]);
                    //ghostGrid[i, j] = challengeGrid[i, j];
                }
            }
            UpdateChallenges();
            lastSize = size;
        }

        public void UpdateChallenges()
        {
            foreach (Challenge c in AllChallenges)
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

        public Challenge RandomBingoChallenge(Challenge type = null, bool ignore = false)
        {
            if (BingoData.availableBingoChallenges == null)
            {
                ChallengeOrganizer.SetupChallengeTypes();
            }

            List<Challenge> list = [];
            list.AddRange(BingoData.availableBingoChallenges);
            if (type != null) list.RemoveAll(x => x.GetType() != type.GetType());
            //for (int i = 0; i < BingoData.availableBingoChallenges.Count; i++)
            //{
            //    list.Add(BingoData.availableBingoChallenges[i]);
            //}

        resette:
            Challenge ch = list[UnityEngine.Random.Range(0, list.Count)];
            if (!ch.ValidForThisSlugcat(ExpeditionData.slugcatPlayer))
            {
                list.Remove(ch);
                goto resette;
            }
            ch = ch.Generate();

            if (AllChallenges.Count > 0 && type == null && !ignore)
            {
                for (int i = 0; i < AllChallenges.Count; i++)
                {
                    if (!AllChallenges[i].Duplicable(ch))
                    {
                        list.Remove(ch);
                        ch = null;
                        goto resette;
                    }
                }
            }

            //Plugin.logger.LogMessage("Assigned " + ch.ChallengeName());
            AllChallenges.Add(ch);
            return AllChallenges.Last();
        }

        //public List<Challenge> AllChallengeList()
        //{
        //    List<Challenge> chacha = [];
        //
        //    for (int i = 0; i < challengeGrid.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < challengeGrid.GetLength(1); j++)
        //        {
        //            chacha.Add(challengeGrid[i, j]);
        //        }
        //    }
        //
        //    return chacha;
        //}

        public void SetChallenge(int x, int y, Challenge newChallenge)
        {
            try
            {
                challengeGrid[x, y] = newChallenge;
                UpdateChallenges();
            }
            catch
            {
                Plugin.logger.LogError("Invalid bingo board coordinates or challenge null :(");
            }
        }

        public void CompleteChallengeAt(int x, int y)
        {
            challengeGrid[x, y].CompleteChallenge();
        }

        public Challenge GetChallenge(int x, int y)
        {
            if (x < challengeGrid.GetLength(0) && y < challengeGrid.GetLength(1)) return challengeGrid[x, y];
            return null;
        }
    }
}
