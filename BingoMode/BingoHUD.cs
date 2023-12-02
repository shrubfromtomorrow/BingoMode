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
using Steamworks;

namespace BingoMode
{
    using BingoSteamworks;

    public class BingoHUD : HudPart
    {
        public BingoBoard board;
        public Vector2 pos;
        public BingoInfo[,] grid;
        public Vector2 mousePosition;
        public Vector2 lastMousePosition;
        public bool mouseDown;
        public bool lastMouseDown;
        public bool show;

        public bool MousePressed => mouseDown && !lastMouseDown;

        public BingoHUD(HUD.HUD hud) : base(hud)
        {
            pos = new Vector2(20.2f, 725.2f);
            board = BingoHooks.GlobalBoard;
            show = true;

            GenerateBingoGrid();
        }

        public override void Update()
        {
            base.Update();

            lastMousePosition = mousePosition;
            mousePosition = Futile.mousePosition;

            lastMouseDown = mouseDown;
            mouseDown = (Input.GetMouseButton(0));

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    grid[i, j].Update();
                }
            }
        }

        public void GenerateBingoGrid()
        {
            grid = new BingoInfo[board.size, board.size];

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    float size = 400f / board.size;
                    float topLeft = -size * board.size / 2f;
                    Vector2 center = new(hud.rainWorld.screenSize.x * 0.175f, hud.rainWorld.screenSize.y * 0.7f);
                    grid[i, j] = new BingoInfo(hud, this,
                        center + new Vector2(topLeft + i * size + (i * size * 0.2f) + size / 2f, -topLeft - j * size - (j * size * 0.2f) - size / 2f), size, hud.fContainers[1], board.challengeGrid[i, j], i, j);
                }
            }
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);

            if (hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player) // Use later when fading the hud
            {
                Player p = hud.owner as Player;

                // Display only if map pressed
                if (p.input[0].mp) show = true;
                else show = false;
            }

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    grid[i, j].show = show;
                    grid[i, j].Draw();
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
            public Challenge challenge;
            public BingoHUD owner;
            public bool show;
            public int x;
            public int y;

            public bool MouseOver
            {
                get
                {
                    return owner.mousePosition.x > pos.x - size / 2f && owner.mousePosition.y > pos.y - size / 2f 
                        && owner.mousePosition.x < pos.x + size / 2f && owner.mousePosition.y < pos.y + size / 2f;
                }
            }

            public BingoInfo(HUD.HUD hud, BingoHUD owner, Vector2 pos, float size, FContainer container, Challenge challenge, int x, int y)
            {
                this.hud = hud;
                this.pos = pos;
                this.size = size;
                this.challenge = challenge;
                this.owner = owner;
                this.x = x;
                this.y = y;

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
                label = new FLabel(Custom.GetFont(), challenge.ChallengeName())
                {
                    x = pos.x,
                    y = pos.y,
                    anchorX = 0.5f,
                    anchorY = 0.5f
                };
                container.AddChild(sprite);
                container.AddChild(label);
            }

            public void Draw() // Add fading later
            {
                if (!show)
                {
                    sprite.alpha = 0f;
                    label.alpha = 0f;
                }
                else
                {
                    sprite.alpha = 0.1f;
                    label.alpha = 1f;
                }
            }

            public void Update()
            {
                label.text = SplitString(challenge.description);//challenge.completed ? "YES" : "NO";

                if (show && MouseOver && owner.mouseDown)
                {
                    sprite.color = Color.blue;
                    if (owner.MousePressed)
                    {
                        challenge.completed = !challenge.completed;
                        //CSteamID id = (CSteamID)76561198140779563;
                        //SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
                        //identity.SetSteamID(id);
                        //InnerWorkings.SendMessage($"#{x - 1};{y}", identity);
                    }
                }
                else if (MouseOver || challenge.completed) sprite.color = Color.red;
                else sprite.color = Color.grey;
            }

            public string SplitString(string s)
            {
                string modified = "";
                int limit = 0;
                foreach (var c in s)
                {
                    limit += 6;
                    if (limit > size * 0.8f)
                    {
                        modified += "\n";
                        limit = 0;
                    }
                    modified += c;
                }
                return modified;
            }
        }
    }
}
