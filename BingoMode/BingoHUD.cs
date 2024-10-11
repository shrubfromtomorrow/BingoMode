using BingoMode.Challenges;
using Expedition;
using HUD;
using RWCustom; 
using UnityEngine;
using System.Collections.Generic;
using Menu.Remix.MixedUI;
using System.Linq;
using Menu;
using MoreSlugcats;

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
        public bool toggled;
        public float alpha;
        public float lastAlpha;
        public bool mapOpen;
        public bool lastMapOpen;
        const int animationLength = 20;
        public int animation = 0;
        public List<BingoInfo> queue;

        public BingoHUD(HUD.HUD hud) : base(hud)
        {
            pos = new Vector2(20.2f, 725.2f);
            board = BingoHooks.GlobalBoard;
            toggled = false;
            queue = [];
            GenerateBingoGrid();
            if (hud.owner.GetOwnerType() == HUD.HUD.OwnerType.SleepScreen)
            {
                AddRevealedToQueue();
            }
        }

        public void AddRevealedToQueue()
        {
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    if (grid[i, j].challenge is BingoChallenge g && g.RequireSave() && g.revealed)
                    {
                        g.CompleteChallenge();
                        g.revealed = false;
                        queue.Add(grid[i, j]);
                    }
                }
            }
            animation = 100;
        }

        public override void ClearSprites()
        {
            base.ClearSprites();

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    grid[i, j].Clear();
                }
            }
            queue.Clear();
        }

        public override void Update()
        {
            base.Update();

            lastMousePosition = mousePosition;
            mousePosition = Futile.mousePosition;

            lastMouseDown = mouseDown;
            mouseDown = Input.GetMouseButton(0);

            if (queue.Count != 0)
            {
                animation--;
                if (animation <= 0)
                {
                    Plugin.logger.LogFatal("should be animating " + queue[0].challenge);
                    queue[0].StartAnim();
                    queue.RemoveAt(0);
                    animation = animationLength;
                }
            }

            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    grid[i, j].Update();
                }
            }
            if (hud.owner.GetOwnerType() == HUD.HUD.OwnerType.Player)
            {
                Player p = hud.owner as Player;

                if (p.input[0].mp && !p.input[1].mp) 
                {
                    toggled = !toggled;
                    Cursor.visible = toggled;
                }
                return;
            }
            if (hud.owner.GetOwnerType() == HUD.HUD.OwnerType.SleepScreen || hud.owner.GetOwnerType() == HUD.HUD.OwnerType.DeathScreen)
            {
                lastMapOpen = mapOpen;
                mapOpen = hud.owner.MapInput.mp;
                //SleepAndDeathScreen sleepScreen = hud.owner as SleepAndDeathScreen;

                if (mapOpen && !lastMapOpen) 
                {
                    toggled = !toggled;
                }
            }

            alpha = Mathf.Clamp01(alpha + 0.1f * (toggled ? 1f : -1f));
            lastAlpha = alpha;
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
                        center + new Vector2(topLeft + i * size + (i * size * 0.075f) + size / 2f, -topLeft - j * size - (j * size * 0.075f) - size / 2f), size, hud.owner is SleepAndDeathScreen s ? s.container : hud.fContainers[1], board.challengeGrid[i, j], i, j);
                }
            }
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);

            float alfa = Mathf.Lerp(lastAlpha, alpha, timeStacker);
            for (int i = 0; i < grid.GetLength(0); i++)
            {
                for (int j = 0; j < grid.GetLength(1); j++)
                {
                    float chooseAlpha = Mathf.Max(Mathf.Lerp(grid[i, j].lastOverwriteAlpha, grid[i, j].overwriteAlpha, timeStacker), alfa);
                    grid[i, j].alpha = Custom.LerpCircEaseOut(0f, 1f, chooseAlpha);
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
            public float alpha;
            public float overwriteAlpha;
            public float lastOverwriteAlpha;
            public int x;
            public int y;
            public Phrase phrase;
            public FContainer container;
            public FSprite[] border;
            public FSprite lockoutLock;
            public TriangleMesh[] teamColors;
            public Vector2[] corners;
            bool showBG;
            FSprite[] boxSprites;
            FLabel infoLabel;
            bool boxVisible;
            public bool lastMouseOver;
            public bool mouseOver;
            public float scale;
            public float goalScale;
            public float goalScaleSpeed;
            public List<TriangleMesh> visible;
            public int updateTextCounter;
            public int flashBorder;
            public float sinCounter;
            public bool doSin;

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
                (challenge as BingoChallenge).ValueChanged += UpdateText;
                (challenge as BingoChallenge).ChallengeCompleted += ChallengeCompleted;
                (challenge as BingoChallenge).ChallengeFailed += OnChallengeFailed;
                (challenge as BingoChallenge).ChallengeLockedOut += BingoInfo_ChallengeLockedOut;
                (challenge as BingoChallenge).ChallengeAlmostComplete += BingoInfo_ChallengeAlmostComplete;
                showBG = true;
                scale = 1f;
                goalScale = 1f;
                goalScaleSpeed = 1f;

                sprite = new FSprite("pixel")
                {
                    scale = size,
                    color = new Color(0.01f, 0.01f, 0.01f),
                    alpha = 0.7f,
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
                flashBorder = -1;

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

                container.AddChild(label);

                boxSprites = new FSprite[5];
                int width = 400;
                int height = 75;
                infoLabel = new FLabel(Custom.GetFont(), challenge.description.WrapText(false, width - 20f))
                {
                    anchorX = 0.5f,
                    anchorY = 0.5f,
                    alignment = FLabelAlignment.Center
                };
                container.AddChild(infoLabel);
                boxSprites[0] = new FSprite("pixel", true)
                {
                    anchorX = 0,
                    anchorY = 0,
                    scaleX = width,
                    scaleY = height,
                    color = new Color(0.01f, 0.01f, 0.01f, 0.9f)
                };
                boxSprites[1] = new FSprite("pixel", true)
                {
                    anchorX = 0,
                    anchorY = 0,
                    scaleX = width + 1,
                };
                boxSprites[2] = new FSprite("pixel", true)
                {
                    anchorX = 0,
                    anchorY = 0,
                    scaleX = width,
                };
                boxSprites[3] = new FSprite("pixel", true)
                {
                    anchorX = 0,
                    anchorY = 0,
                    scaleY = height,
                };
                boxSprites[4] = new FSprite("pixel", true)
                {
                    anchorX = 0,
                    anchorY = 0,
                    scaleY = height,
                };
                for (int i = 0; i < boxSprites.Length; i++)
                {
                    container.AddChild(boxSprites[i]);
                    if (i > 0)
                    {
                        boxSprites[i].scaleX += 1f;
                        boxSprites[i].scaleY += 1f;
                        boxSprites[i].color = Color.white;
                        boxSprites[i].shader = hud.rainWorld.Shaders["MenuText"];
                    }
                }
                UpdateText();
                UpdateTeamColors();
            }

            private void BingoInfo_ChallengeAlmostComplete(int team)
            {
                UpdateText();
                flashBorder = team;
                sinCounter = 0f;
            }

            private void BingoInfo_ChallengeLockedOut()
            {
                UpdateText();
            }

            public void OnChallengeFailed(int tea)
            {
                UpdateText();
            }

            public void ChallengeCompleted(int tea)
            {
                UpdateText();
                flashBorder = tea;
                sinCounter = 0f;
            }

            public void Clear()
            {
                (challenge as BingoChallenge).ValueChanged -= UpdateText;
                (challenge as BingoChallenge).ChallengeCompleted -= ChallengeCompleted;
                (challenge as BingoChallenge).ChallengeFailed -= OnChallengeFailed;
                (challenge as BingoChallenge).ChallengeLockedOut -= BingoInfo_ChallengeLockedOut;
                (challenge as BingoChallenge).ChallengeAlmostComplete -= BingoInfo_ChallengeAlmostComplete;
                sprite.RemoveFromContainer();
                label.RemoveFromContainer();
                foreach (var g in border)
                {
                    g.RemoveFromContainer();
                }
                foreach (var g in boxSprites)
                {
                    g.RemoveFromContainer();
                }
                foreach (var g in teamColors)
                {
                    g.RemoveFromContainer();
                }
                infoLabel.RemoveFromContainer();
            }

            public void UpdateTeamColors()
            {
                //Plugin.logger.LogMessage($"Updating team colors for " + challenge);
                visible = [];
                bool g = false;
                for (int i = 0; i < teamColors.Length; i++)
                {
                    teamColors[i].isVisible = false;
                    if ((challenge as BingoChallenge).TeamsCompleted[i])
                    {
                        //Plugin.logger.LogMessage("Making it visible!");
                        if (i == SteamTest.team)
                        {
                            sinCounter = 0.5f;
                            flashBorder = i;
                        }
                        visible.Add(teamColors[i]);
                        g = true;
                    }
                }
                showBG = !g;
            }

            public void Update()
            {
                lastMouseOver = mouseOver;
                mouseOver = owner.mousePosition.x > pos.x - size / 2f && owner.mousePosition.y > pos.y - size / 2f
                        && owner.mousePosition.x < pos.x + size / 2f && owner.mousePosition.y < pos.y + size / 2f;
                boxVisible = alpha > 0f && mouseOver;
                //else if (challenge.hidden) sprite.color = new Color(0.01f, 0.01f, 0.01f);
                //else if (MouseOver || challenge.completed) sprite.color = Color.red;
                //else sprite.color = Color.grey;
                if (mouseOver && lastMouseOver != mouseOver)
                {
                    for (int i = 0; i < boxSprites.Length; i++)
                    {
                        boxSprites[i].MoveToFront();
                    }
                    infoLabel.MoveToFront();
                }

                if (alpha > 0f && mouseOver && owner.mouseDown && !owner.lastMouseDown)
                {
                    challenge.CompleteChallenge();
                }

                bool doOverwriteAlpha = false;
                if (updateTextCounter > 0)
                {
                    if (updateTextCounter > 1) doOverwriteAlpha = true;
                    updateTextCounter -= 1;
                    if (updateTextCounter == 0)
                    {
                        Tick();
                    }
                }

                lastOverwriteAlpha = overwriteAlpha;
                if (doOverwriteAlpha)
                {
                    overwriteAlpha = Mathf.Min(overwriteAlpha + 0.04f, 1f);
                }
                else overwriteAlpha = Mathf.Max(overwriteAlpha - 0.02f, 0f);

                if (doSin)
                {
                    sinCounter += 0.016f;
                    if (sinCounter > 1f)
                    {
                        sinCounter -= 2f;
                    }
                }

                if (scale == 1.135f)
                {
                    goalScale = 1f;
                    goalScaleSpeed = 0.6f;
                }
                scale = Custom.LerpAndTick(scale, goalScale, goalScaleSpeed, 0.001f);
            }

            public void Draw()
            {
                // Phrase biz
                sprite.alpha = showBG ? Mathf.Lerp(0f, 0.85f, alpha) : 0f;
                foreach (var g in border) g.alpha = alpha;
                label.alpha = alpha;

                for (int i = 0; i < teamColors.Length; i++)
                {
                    teamColors[i].alpha = Mathf.Lerp(0f, 0.4f, alpha);
                }

                if (phrase != null)
                {
                    phrase.SetAlpha(alpha);
                    phrase.centerPos = pos;
                    phrase.scale = size / 84f * scale;
                    //phrase.applyScale = goalScale != 1f && scale != 1f;
                    phrase.Draw();
                }

                // Border
                for (int i = 0; i < border.Length; i++)
                {
                    border[i].scaleX = (i < 2) ? size * scale : 2f;
                    border[i].scaleY = (i < 2) ? 2f : size * scale;
                }
                sprite.scale = scale * size;
                corners[0] = pos + new Vector2(-size * scale / 2f, -size * scale / 2f);
                corners[1] = pos + new Vector2(-size * scale / 2f, size * scale / 2f);
                corners[2] = pos + new Vector2(size * scale / 2f, -size * scale / 2f);
                corners[3] = pos + new Vector2(size * scale / 2f, size * scale / 2f);
                border[0].SetPosition(corners[0]);
                border[1].SetPosition(corners[1]);
                border[2].SetPosition(corners[0]);
                border[3].SetPosition(corners[2]);

                if (flashBorder != -1)
                {
                    Color flashColor = Color.Lerp(Color.white, BingoPage.TEAM_COLOR[flashBorder], Mathf.Abs(Mathf.Sin(sinCounter * Mathf.PI)));
                    for (int i = 0; i < 4; i++)
                    {
                        border[i].color = flashColor;
                    }
                }

                // Colors
                float dist = size / visible.Count * scale;
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

                // Thinj and binj (box)
                for (int i = 0; i < boxSprites.Length; i++)
                {
                    boxSprites[i].isVisible = boxVisible;
                }
                infoLabel.isVisible = boxVisible;
                float yStep = boxSprites[3].scaleY / 2f;
                boxSprites[0].SetPosition(pos + new Vector2(size / 2f + 10f, -yStep));
                boxSprites[1].SetPosition(pos + new Vector2(size / 2f + 10f, yStep - 1));
                boxSprites[2].SetPosition(pos + new Vector2(size / 2f + 10f, -yStep));
                boxSprites[3].SetPosition(pos + new Vector2(size / 2f + 10f, -yStep));
                boxSprites[4].SetPosition(pos + new Vector2(size / 2f + 10f + boxSprites[0].scaleX, -yStep));
                infoLabel.SetPosition(pos + new Vector2(size / 2f + 10f + boxSprites[0].scaleX / 2f, 0));
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
                if (phrase == null)
                {
                    updateTextCounter = 1;
                    return;
                }
                //if (owner.queue.Count == 0) owner.animation = 0;
                owner.queue.Add(this);
            }

            public void StartAnim()
            {
                goalScale = 0.85f;
                goalScaleSpeed = 0.0125f;
                updateTextCounter = 50;
                Plugin.logger.LogFatal("anim machen");
            }

            public void Tick()
            {
                if (phrase != null)
                {
                    phrase.ClearAll();
                }
                phrase = challenge.hidden ? Phrase.LockPhrase() : (challenge as BingoChallenge).ConstructPhrase();
                if (phrase != null)
                {
                    phrase.AddAll(container);
                    phrase.centerPos = pos;
                    phrase.scale = size / 84f * scale;
                    phrase.Draw();
                }
                label.text = phrase == null ? SplitString(challenge.description) : "";
                infoLabel.text = challenge.description.WrapText(false, boxSprites[0].scaleX - 20f);
                if ((challenge as BingoChallenge).TeamsCompleted.Any(x => x == true))
                {
                    infoLabel.text += "\nCompleted by: ";
                    for (int i = 0; i < (challenge as BingoChallenge).TeamsCompleted.Length; i++)
                    {
                        if ((challenge as BingoChallenge).TeamsCompleted[i]) infoLabel.text += BingoPage.TeamName(i) + ", ";
                    }
                    infoLabel.text = infoLabel.text.Substring(0, infoLabel.text.Length - 2); // Trim the last ", "
                }
                if (overwriteAlpha > 0f)
                {
                    hud.PlaySound((challenge as BingoChallenge).TeamsCompleted[SteamTest.team] ? MMFEnums.MMFSoundID.Tock : MMFEnums.MMFSoundID.Tick);
                    goalScale = 1.135f;
                    goalScaleSpeed = 0.7f;
                    UpdateTeamColors();
                    doSin = challenge.revealed && !(challenge as BingoChallenge).TeamsCompleted[SteamTest.team];
                    sinCounter = 0.5f;
                }
            }
        }
    }
}
