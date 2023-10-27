using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Expedition;
using HUD;
using Menu;
using Menu.Remix;
using UnityEngine;
using RWCustom;

namespace BingoMode
{
    public class BingoHUD : HudPart
    {
        public BingoBoard board;
        public Vector2 pos;
        public BingoInfo[,] grid;

        public BingoHUD(HUD.HUD hud) : base(hud)
        {
            pos = new Vector2(20.2f, 725.2f);
            board = BingoHooks.GlobalBoard;

            GenerateBingoGrid();
        }

        public void GenerateBingoGrid()
        {
            grid = new BingoInfo[board.size, board.size];

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    float size = 250f / board.size;
                    float topLeft = -size * board.size / 2f;
                    Vector2 center = new(hud.rainWorld.screenSize.x * 0.1f, hud.rainWorld.screenSize.y * 0.823f);
                    grid[i, j] = new BingoInfo(hud,
                        center + new Vector2(topLeft + i * size + size / 2f, -topLeft - j * size - size / 2f), size, hud.fContainers[1]);
                }
            }
        }

        public class BingoInfo
        {
            public HUD.HUD hud;
            public Vector2 pos;
            public FSprite sprite;
            public FLabel label;
            public float size;

            public BingoInfo(HUD.HUD hud, Vector2 pos, float size, FContainer container)
            {
                this.hud = hud;
                this.pos = pos;
                this.size = size;

                sprite = new FSprite("pixel")
                {
                    scale = size,
                    alpha = 0.1f,
                    color = Color.grey,
                    x = pos.x, 
                    y = pos.y,
                    anchorX = 0.5f,
                    anchorY = 0.5f
                };
                label = new FLabel(Custom.GetFont(), "HELO")
                {
                    x = pos.x,
                    y = pos.y,
                    anchorX = 0.5f,
                    anchorY = 0.5f
                };
                container.AddChild(sprite);
                container.AddChild(label);
            }
        }
    }
}
