using BingoMode.Challenges;
using Expedition;
using HUD;
using RWCustom; 
using UnityEngine;

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
                    float size = 400f / board.size;
                    float topLeft = -size * board.size / 2f;
                    Vector2 center = new(hud.rainWorld.screenSize.x * 0.1575f, hud.rainWorld.screenSize.y * 0.725f);
                    grid[i, j] = new BingoInfo(hud, this,
                        center + new Vector2(topLeft + i * size + (i * size * 0.05f) + size / 2f, -topLeft - j * size - (j * size * 0.05f) - size / 2f), size, hud.fContainers[1], board.challengeGrid[i, j], i, j);
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
                (challenge as BingoChallenge).DescriptionUpdated += UpdateText;

                sprite = new FSprite("pixel")
                {
                    scale = size,
                    alpha = 0f,
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
                UpdateText();
            }

            public void Draw() // Add fading later
            {
                // Phrase biz
                sprite.alpha = Mathf.Lerp(0f, 0.1f, alpha);

                if (phrase != null)
                {
                    phrase.SetAlpha(alpha);
                    phrase.centerPos = pos;// + new Vector2(size / 2f, size / 2f);
                    phrase.Draw();
                }
            }

            public void Update()
            {
                //label.text = SplitString(challenge.description);//challenge.completed ? "YES" : "NO";
                if (alpha > 0f && MouseOver && owner.mouseDown)
                {
                    sprite.color = Color.blue;
                    if (owner.MousePressed)
                    {
                        challenge.CompleteChallenge();//.completed = !challenge.completed;
                        //CSteamID id = (CSteamID)76561198140779563;
                        //SteamNetworkingIdentity identity = new SteamNetworkingIdentity();
                        //identity.SetSteamID(id);
                        //InnerWorkings.SendMessage($"#{x - 1};{y}", identity);
                    }
                }
                else if (challenge.hidden) sprite.color = new Color(0.01f, 0.01f, 0.01f);
                else if (MouseOver || challenge.completed) sprite.color = Color.red;
                else sprite.color = Color.grey;
            }

            public void UpdateText()
            {
                label.text = "";
                if (phrase != null)
                {
                    phrase.ClearAll();
                }
                phrase = (challenge as BingoChallenge).ConstructPhrase();
                if (phrase != null)
                {
                    phrase.AddAll(container);
                }
            }
        }
    }
}
