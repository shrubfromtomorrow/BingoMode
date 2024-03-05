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
using BingoMode.Challenges;

namespace BingoMode
{
    public class BingoBoard
    {
        public ExpeditionCoreFile core;
        public Challenge[,] challengeGrid; // The challenges will be treated as coordinates on a grid for convenience
        //public Challenge[,] ghostGrid;
        public List<IntVector2> currentWinLine; // A list of grid coordinates
        public int size;
        public int lastSize;
        public List<Challenge> recreateList;

        public BingoBoard()
        {
            size = 5;
            lastSize = 5;
            currentWinLine = [];
            recreateList = [];
        }

        public void GenerateBoard(int size, bool changeSize = false)
        {
            Plugin.logger.LogMessage("Generating bored");
            if (!changeSize)
            {
                BingoData.FillPossibleTokens(ExpeditionData.slugcatPlayer);
                challengeGrid = new Challenge[size, size];
                ExpeditionData.ClearActiveChallengeList();
            }
            int dex = 0;
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
                    challengeGrid[i, j] = RandomBingoChallenge();
                    //(challengeGrid[i, j] as IBingoChallenge).Index = dex;
                    //ghostGrid[i, j] = challengeGrid[i, j];
                    dex++;
                }
            }
            UpdateChallenges();
            lastSize = size;
        }

        public void UpdateChallenges()
        {
            foreach (Challenge c in ExpeditionData.challengeList)
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

        resette:
            Challenge ch = list[UnityEngine.Random.Range(0, list.Count)];
            if (!ch.ValidForThisSlugcat(ExpeditionData.slugcatPlayer))
            {
                list.Remove(ch);
                goto resette;
            }
            ch = ch.Generate();

            if (ExpeditionData.challengeList.Count > 0 && type == null && !ignore)
            {
                for (int i = 0; i < ExpeditionData.challengeList.Count; i++)
                {
                    if (!ExpeditionData.challengeList[i].Duplicable(ch))
                    {
                        list.Remove(ch);
                        ch = null;
                        goto resette;
                    }
                }
            }

            if (ch == null) goto resette;
            ExpeditionData.challengeList.Add(ch);
            return ExpeditionData.challengeList.Last();
        }

        public void RecreateFromList()
        {
            if (recreateList != null && recreateList.Count > 0)
            {
                int next = 0;
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        //if (recreateList.Count < next + 1)
                        //{
                        //    challengeGrid[i, j] = RandomBingoChallenge();
                        //}
                        //else 
                        challengeGrid[i, j] = recreateList[next];
                        //(challengeGrid[i, j] as IBingoChallenge).Index = next;
                        //Plugin.logger.LogMessage($"Recreated {recreateList[next]} at: {i}, {j}. Challenge - {challengeGrid[i, j]} with index {(challengeGrid[i, j] as IBingoChallenge).Index}");
                        next++;
                    }
                }
                recreateList = [];
                Plugin.logger.LogMessage("Recreated list from thinj yipe");
            }
        }

        public void SetChallenge(int x, int y, Challenge newChallenge, int index)
        {
            try
            {
                int g1 = index == -1 ? ExpeditionData.challengeList.IndexOf(challengeGrid[x, y]) : index;
                //(newChallenge as IBingoChallenge).Index = g1;
                ExpeditionData.challengeList.Remove(challengeGrid[x, y]);
                challengeGrid[x, y] = newChallenge;
                ExpeditionData.challengeList.Insert(g1, challengeGrid[x, y]);
                UpdateChallenges();
            }
            catch (Exception e)
            {
                Plugin.logger.LogError("Invalid bingo board coordinates or challenge null :( " + e);
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
