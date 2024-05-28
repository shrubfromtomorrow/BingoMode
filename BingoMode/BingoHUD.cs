using BingoMode.Challenges;
using Expedition;
using HUD;
using RWCustom; 
using UnityEngine;
using System.Collections.Generic;

namespace BingoMode
{
    public class BingoHUD : HudPart
    {
        public BingoBoard board;
        public Vector2 pos;
        public BingoInfo[,] grid;
        public Vector2 mousePosition;
        public Vector2 lastMousePosition;
        public bool mouseDown;
        public bool lastMouseDown;
        public bool toggled;
        public float alpha;
        public float lastAlpha;

        public bool MousePressed => mouseDown && !lastMouseDown;

        public BingoHUD(HUD.HUD hud) : base(hud)
        {
            pos = new Vector2(20.2f, 725.2f);
            board = BingoHooks.GlobalBoard;
            toggled = false;

            GenerateBingoGrid();
        }

        public override void Update()
        {
            base.Update();

            lastMousePosition = mousePosition;
            mousePosition = Futile.mousePosition;

            lastMouseDown = mouseDown;
            mouseDown = Input.GetMouseButton(0);

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    grid[i, j].Update();
                }
            }

            if (hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player) // Use later when fading the hud
            {
                Player p = hud.owner as Player;

                // Display only if map pressed
                if (p.input[0].mp && !p.input[1].mp) toggled = !toggled;
            }
        }

        public void GenerateBingoGrid()
        {
            grid = new BingoInfo[board.size, board.size];

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    float size = 420f / board.size;
                    float topLeft = -size * board.size / 2f;
                    Vector2 center = new(hud.rainWorld.screenSize.x * 0.16f, hud.rainWorld.screenSize.y * 0.715f);
                    grid[i, j] = new BingoInfo(hud, this,
                        center + new Vector2(topLeft + i * size + (i * size * 0.075f) + size / 2f, -topLeft - j * size - (j * size * 0.075f) - size / 2f), size, hud.fContainers[1], board.challengeGrid[i, j], i, j);
                }
            }
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);

            alpha = Mathf.Clamp01(alpha + 0.1f * (toggled ? 1f : -1f));

            float alfa = Mathf.Lerp(lastAlpha, alpha, timeStacker);
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    grid[i, j].alpha = Custom.LerpCircEaseOut(0f, 1f, alfa);
                    grid[i, j].Draw();
                }
            }

            lastAlpha = alpha;
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
            public float alpha;
            public int x;
            public int y;
            public Phrase phrase;
            public FContainer container;
            public FSprite[] teamCompletes;
            public FSprite[] border;
            public TriangleMesh[] teamColors;
            public Vector2[] corners;
            bool showBG;

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
                this.container = container;
                alpha = 1f;
                (challenge as BingoChallenge).DescriptionUpdated += UpdateText;
                (challenge as BingoChallenge).ChallengeCompleted += UpdateTeamColors;
                showBG = true;

                sprite = new FSprite("pixel")
                {
                    scale = size,
                    color = new Color(0f, 0f, 0f),
                    alpha = 0.75f,
                    x = pos.x, 
                    y = pos.y,
                    anchorX = 0.5f,
                    anchorY = 0.5f
                };
                label = new FLabel(Custom.GetFont(), "")
                {
                    x = pos.x,
                    y = pos.y,
                    anchorX = 0.5f,
                    anchorY = 0.5f
                };
                container.AddChild(sprite);

                float scaleFac = (size / 84f);
                border = new FSprite[4];
                for (int i = 0; i < border.Length; i++)
                {
                    border[i] = new FSprite("pixel")
                    {
                        scaleX = (i < 2) ? size : 2f,
                        anchorX = (i < 2) ? 0f : 0.5f,
                        scaleY = (i < 2) ? 2f : size,
                        anchorY = (i < 2) ? 0.5f : 0f
                    };
                    container.AddChild(border[i]);
                }
                corners = new Vector2[4];
                corners[0] = pos + new Vector2(-size / 2f, -size / 2f);
                corners[1] = pos + new Vector2(-size / 2f, size / 2f);
                corners[2] = pos + new Vector2(size / 2f, -size / 2f);
                corners[3] = pos + new Vector2(size / 2f, size / 2f);
                border[0].SetPosition(corners[0]);
                border[1].SetPosition(corners[1]);
                border[2].SetPosition(corners[0]);
                border[3].SetPosition(corners[2]);

                teamColors = new TriangleMesh[8];
                TriangleMesh.Triangle[] tris = [
                    new(0, 1, 2),
                    new(1, 2, 3)
                    ];
                for (int i = 0; i < teamColors.Length; i++)
                {
                    teamColors[i] = new TriangleMesh("Futile_White", tris, false)
                    {
                        color = BingoPage.TEAM_COLOR[i],
                    };
                    container.AddChild(teamColors[i]);
                }

                //teamCompletes = new FSprite[8];
                //for (int i = 0; i < teamCompletes.Length; i++)
                //{
                //    teamCompletes[i] = new FSprite("Kill_Slugcat")
                //    {
                //        color = BingoPage.TEAM_COLOR[i],
                //        scale = 0.8f * scaleFac
                //    };
                //    container.AddChild(teamCompletes[i]);
                //}
                container.AddChild(label);
                UpdateText();
                UpdateTeamColors();
            }

            public void UpdateTeamColors()
            {
                bool[] virtualTeams = new bool[8];
                bool g = false;
                for (int i = 0; i < virtualTeams.Length; i++)
                {
                    if (Random.value < 0.1f) { virtualTeams[i] = true; g = true; }
                }
                if (g) showBG = false;

                List<TriangleMesh> visible = [];
                for (int i = 0; i < teamColors.Length; i++)
                {
                    //teamColors[i].isVisible = virtualTeams[i];//(challenge as BingoChallenge).TeamsCompleted[i];
                    teamColors[i].isVisible = false;
                    if (virtualTeams[i]) visible.Add(teamColors[i]);
                }

                float dist = size / visible.Count;
                float halfStep = dist * 0.3f;
                for (int i = 0; i < visible.Count; i++)
                {
                    visible[i].isVisible = true;

                    int isFirst = i == 0 ? 0 : 1;
                    int isLast = i == visible.Count - 1 ? 0 : 1;
                    visible[i].MoveVertice(0, corners[0] + new Vector2(dist * i - halfStep * isFirst, 0f));
                    visible[i].MoveVertice(1, corners[1] + new Vector2(dist * i + halfStep * isFirst, 0f));

                    visible[i].MoveVertice(2, corners[0] + new Vector2(dist * (i + 1) - halfStep * isLast, 0f));
                    visible[i].MoveVertice(3, corners[1] + new Vector2(dist * (i + 1) + halfStep * isLast, 0f));
                }
            }

            public void Draw() // Add fading later
            {
                // Phrase biz
                sprite.alpha = showBG ? Mathf.Lerp(0f, 0.66f, alpha) : 0f;
                foreach (var g in border) g.alpha = alpha;
                label.alpha = alpha;

                //for (int i = 0; i < teamCompletes.Length; i++)
                //{
                //    Vector2 teamPos = pos + new Vector2(-31.5f + 10.5f * i, -31.5f);
                //    teamCompletes[i].SetPosition(teamPos);
                //    teamCompletes[i].alpha = alpha;
                //}

                for (int i = 0; i < teamColors.Length; i++)
                {
                    teamColors[i].alpha = Mathf.Lerp(0f, 0.4f, alpha);
                }

                if (phrase != null)
                {
                    phrase.SetAlpha(alpha);
                    phrase.centerPos = pos;
                    phrase.Draw();
                    phrase.scale = size / 84f;
                }
            }

            public void Update()
            {
                if (alpha > 0f && MouseOver && owner.mouseDown)
                {
                    //sprite.color = Color.blue;
                    if (owner.MousePressed)
                    {
                        challenge.CompleteChallenge();
                    }
                }
                //else if (challenge.hidden) sprite.color = new Color(0.01f, 0.01f, 0.01f);
                //else if (MouseOver || challenge.completed) sprite.color = Color.red;
                //else sprite.color = Color.grey;
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

            public void UpdateText()
            {
                if (phrase != null)
                {
                    phrase.ClearAll();
                }
                phrase = (challenge as BingoChallenge).ConstructPhrase();
                if (phrase != null)
                {
                    phrase.AddAll(container);
                    phrase.centerPos = pos;
                    phrase.Draw();
                    phrase.scale = size / 84f;
                }
                label.text = phrase == null ? SplitString(challenge.description) : "";
            }
        }
    }
}
