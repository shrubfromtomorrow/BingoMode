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
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace BingoMode
{
    public class BingoBoard
    {
        public ExpeditionCoreFile core;
        public Challenge[,] challengeGrid; // The challenges will be treated as coordinates on a grid for convenience
        public List<IntVector2> currentWinLine; // A list of grid coordinates
        public int size;
        public List<Challenge> recreateList;

        public BingoBoard()
        {
            size = 5;
            currentWinLine = [];
            recreateList = [];
        }

        public void GenerateBoard(int size, bool changeSize = false)
        {
            Plugin.logger.LogMessage("Generating bored");
            Challenge[,] ghostGrid = new Challenge[size, size];
            BingoData.FillPossibleTokens(ExpeditionData.slugcatPlayer);
            ExpeditionData.ClearActiveChallengeList();
            if (changeSize)
            { 
                ghostGrid = challengeGrid;
            }
            challengeGrid = new Challenge[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    if (changeSize)
                    {
                        if (!(i + 1 > ghostGrid.GetLength(0) || j + 1 > ghostGrid.GetLength(1)) && ghostGrid[i, j] != null)
                        {
                            challengeGrid[i, j] = ghostGrid[i, j];
                            if (!ExpeditionData.challengeList.Contains(challengeGrid[i, j])) ExpeditionData.challengeList.Add(challengeGrid[i, j]);
                            continue;
                        }
                    }
                    if (challengeGrid[i, j] != null)
                    {
                        continue;
                    }
                    challengeGrid[i, j] = RandomBingoChallenge();
                }
            }
            Plugin.logger.LogMessage("Current challenge list");
            foreach (var gruh in ExpeditionData.challengeList)
            {
                Plugin.logger.LogMessage(ExpeditionData.challengeList.IndexOf(gruh) + " - " + gruh);
            }
            UpdateChallenges();
        }

        public void UpdateChallenges()
        {
            foreach (Challenge c in ExpeditionData.challengeList)
            {
                c.UpdateDescription();
            }
        }

        public bool CheckWin(bool bias = false)
        {
            bool won = false;

            // Vertical lines
            for (int i = 0; i < size; i++)
            {
                bool line = true;
                for (int j = 0; j < size; j++)
                {
                    var ch = challengeGrid[i, j];
                    line &= bias ? !(ch as IBingoChallenge).Failed && !ch.hidden : ch.completed && !ch.hidden;
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
                        var ch = challengeGrid[j, i];
                        line &= bias ? !(ch as IBingoChallenge).Failed && !ch.hidden : ch.completed && !ch.hidden;
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
                    var ch = challengeGrid[i, i];
                    line &= bias ? !(ch as IBingoChallenge).Failed && !ch.hidden : ch.completed && !ch.hidden;
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
                    var ch = challengeGrid[size - 1 - i, i];
                    line &= bias ? !(ch as IBingoChallenge).Failed && !ch.hidden : ch.completed && !ch.hidden;
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

        public Challenge RandomBingoChallenge(Challenge type = null, bool ignore = false, bool add = true)
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
            if (add) ExpeditionData.challengeList.Add(ch);
            return ch;
        }

        public void RecreateFromList()
        {
            if (recreateList != null && recreateList.Count == size * size)
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
                        Plugin.logger.LogMessage($"Recreated {recreateList[next]} at: {i}, {j}. Challenge - {challengeGrid[i, j]} with index {(challengeGrid[i, j] as IBingoChallenge).Index}");
                        next++;
                    }
                }
                recreateList = [];
                Plugin.logger.LogMessage("Recreated list from thinj yipe");
            }
            //else GenerateBoard(size);
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

        public override string ToString()
        {
            string text = string.Join("bChG", ExpeditionData.challengeList);
            Plugin.logger.LogMessage("Bingo board to string:\n" + text);
            return text;
        }
        
        public void FromString(string text)
        {
            Plugin.logger.LogMessage("Bingo board from string:\n" + text);
            ExpeditionData.allChallengeLists[ExpeditionData.slugcatPlayer].Clear();
            string[] challenges = Regex.Split(text, "bChG");
            int size = Mathf.FloorToInt(challenges.Length / 2f);
            int next = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    try
                    {
                        string[] array10 = Regex.Split(challenges[i], "#");
                        string[] array11 = Regex.Split(array10[1], "~");
                        string type = array11[0];
                        string text2 = array11[1];
                        Challenge challenge = (Challenge)Activator.CreateInstance(ChallengeOrganizer.availableChallengeTypes.Find((Challenge c) => c.GetType().Name == type).GetType());
                        challenge.FromString(text2);
                        ExpLog.Log(challenge.description);
                        if (!ExpeditionData.allChallengeLists.ContainsKey(ExpeditionData.slugcatPlayer))
                        {
                            ExpeditionData.allChallengeLists.Add(ExpeditionData.slugcatPlayer, new List<Challenge>());
                        }
                        ExpeditionData.allChallengeLists[ExpeditionData.slugcatPlayer].Add(challenge);
                        challengeGrid[i, j] = challenge;
                    }
                    catch (Exception ex)
                    {
                        Plugin.logger.LogError("ERROR: Problem recreating challenge type with reflection in bingoboard.fromstring: " + ex.Message);
                    }
                    next++;
                }
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
