using Menu;
using UnityEngine;

namespace BingoMode.BingoMenu
{
    public class BingoGrid : PositionedMenuObject
    {
        public BingoBoard board;
        public BingoButton[,] challengeButtons;
        public float maxSize;
        public int size;
        public Vector2 centerPos;

        public BingoGrid(Menu.Menu menu, MenuObject owner, Vector2 centerPos, float maxSize) : base(menu, owner, default)
        {
            board = BingoHooks.GlobalBoard;
            size = board.size;
            this.maxSize = maxSize;
            this.centerPos = centerPos;

            GenerateBoardButtons();
        }

        public override void Singal(MenuObject sender, string message)
        {
            if (message == "SWITCH")
            {
                BingoButton chal = sender as BingoButton;
                BingoHooks.GlobalBoard.SwitchChals(chal.challenge, chal.x, chal.y);
                return;
            }

            base.Singal(sender, message);
        }

        public void Switch(bool off)
        {
            for (int j = 0; j < challengeButtons.GetLength(1); j++)
                for (int i = 0; i < challengeButtons.GetLength(0); i++)
                    challengeButtons[i, j].buttonBehav.greyedOut = off;
        }

        public void GenerateBoardButtons()
        {
            challengeButtons = new BingoButton[size, size];
            for (int j = 0; j < board.challengeGrid.GetLength(1); j++)
            {
                for (int i = 0; i < board.challengeGrid.GetLength(0); i++)
                {
                    float butSize = maxSize / size;
                    float topLeft = -butSize * size / 2f;
                    BingoButton but = new(
                            menu,
                            this,
                            centerPos - new Vector2(butSize / 2f, butSize / 2f) + new Vector2(topLeft + i * butSize + butSize / 2f, -topLeft - j * butSize - butSize / 2f - 50f),
                            new Vector2(butSize, butSize),
                            i + " " + j,
                            i,
                            j);
                    challengeButtons[i, j] = but;
                    subObjects.Add(but);
                }
            }
        }
    }
}
