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
    public class BingoGrid : PositionedMenuObject
    {
        public BingoBoard board;
        public List<BingoButton> challengeButtons;
        public float maxSize;
        public int size;
        public Vector2 centerPos;

        public BingoGrid(Menu.Menu menu, MenuObject owner, Vector2 centerPos, float maxSize) : base(menu, owner, default)
        {
            board = BingoHooks.GlobalBoard;
            challengeButtons = [];
            size = board.size;
            this.maxSize = maxSize;
            this.centerPos = centerPos;

            GenerateBoardButtons();
        }

        public void GenerateBoardButtons()
        {
            for (int i = 0; i < board.challengeGrid.GetLength(0); i++)
            {
                for (int j = 0; j < board.challengeGrid.GetLength(1); j++)
                {
                    float butSize = maxSize / size;
                    float topLeft = -butSize * size / 2f;
                    BingoButton but = new BingoButton(menu, this,
                    centerPos - new Vector2(butSize / 2f, butSize / 2f) + new Vector2(topLeft + i * butSize + butSize / 2f, -topLeft - j * butSize - butSize / 2f - 50f), new Vector2(butSize, butSize), i + " " + j, i, j)
                    {
                        lastPos = pos
                    };
                    challengeButtons.Add(but);
                    subObjects.Add(but);
                    //Plugin.logger.LogMessage("Added new bimbo " + but + " at " + but.pos);
                }
            }
        }
    }
}
