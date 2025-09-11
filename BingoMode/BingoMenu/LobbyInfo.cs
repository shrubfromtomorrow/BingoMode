using BingoMode.BingoSteamworks;
using Menu;
using UnityEngine;
using static BingoMode.BingoSteamworks.LobbySettings;

namespace BingoMode.BingoMenu
{
    public class LobbyInfo : PositionedMenuObject
    {
        private const float MARGIN = 5f;
        private const float ALPHA_THRESHOLD = 0.01f;

        private MenuLabel nameLabel;
        private MenuLabel playerLabel;
        private SimpleButton button;
        private InfoPanel infoPanel;

        private float _alpha = 1f;

        public Vector2 size;
        public float Alpha
        {
            get => _alpha;
            set
            {
                if (_alpha == value)
                    return;

                nameLabel.label.alpha = value;
                playerLabel.label.alpha = value;

                button.buttonBehav.greyedOut = value < ALPHA_THRESHOLD;

                _alpha = value;
            }
        }

        public LobbyInfo(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, LobbyData data) : base(menu, owner, pos)
        {
            this.size = size;

            nameLabel = new(menu, this, data.name, new Vector2(MARGIN, size.y / 2f), Vector2.zero, false);
            nameLabel.label.alignment = FLabelAlignment.Left;
            nameLabel.label.anchorY = 0.5f;
            subObjects.Add(nameLabel);

            playerLabel = new(menu, this, $"{data.currentPlayers}/{data.maxPlayers}", new Vector2(size.x - MARGIN, size.y / 2f), Vector2.zero, false);
            playerLabel.label.alignment = FLabelAlignment.Right;
            playerLabel.label.anchorY = 0.5f;
            subObjects.Add(playerLabel);

            button = new(menu, this, "", "JOIN-" + data.lobbyID.ToString(), Vector2.zero, size);
            foreach (FSprite sprite in button.roundedRect.sprites)
                sprite.alpha = 0f;
            subObjects.Add(button);

            infoPanel = new(menu, this, new Vector2(size.x + MARGIN, (size.y / 2f) - (InfoPanel.HEIGHT / 2f)), data);
            subObjects.Add(infoPanel);
        }

        public override void GrafUpdate(float timeStacker)
        {
            button.roundedRect.fillAlpha = 0f;
            base.GrafUpdate(timeStacker);
            infoPanel.Visible = button.IsMouseOverMe && _alpha >= ALPHA_THRESHOLD;
        }

        public override void RemoveSprites()
        {
            base.RemoveSprites();
            //infoPanel.Visible = true;
            foreach (MenuObject obj in subObjects)
            {
                obj.RemoveSprites();
                RecursiveRemoveSelectables(obj);
            }
            subObjects.Clear();
        }

        private class InfoPanel : PositionedMenuObject
        {
            public const float WIDTH = 180f;
            public const float HEIGHT = 100f;
            private const float BORDER_WIDTH = 2f;
            private const float POLICE_SIZE = 15f; // not actually modifiable from here

            private bool _visible = true;

            private FSprite background;
            private FSprite[] border;
            private MenuLabel[] labels;

            public bool Visible
            {
                get => _visible;
                set
                {
                    if (_visible == value)
                        return;
                    background.isVisible = value;
                    foreach (FSprite line in border)
                        line.isVisible = value;
                    foreach (MenuLabel label in labels)
                        label.label.isVisible = value;
                    _visible = value;
                }
            }

            public InfoPanel(Menu.Menu menu, MenuObject owner, Vector2 pos, LobbyData data) : base(menu, owner, pos)
            {
                background = new FSprite("pixel")
                {
                    scaleX = WIDTH,
                    scaleY = HEIGHT,
                    anchorX = 0f,
                    anchorY = 0f,
                    color = Color.black,
                    alpha = 0.9f
                };
                Container.AddChild(background);

                border = new FSprite[4];
                for (int i = 0; i < 4; i++)
                {
                    border[i] = new FSprite("pixel")
                    {
                        anchorX = (i < 2) ? 0f : 1f,
                        anchorY = (i < 2) ? 0f : 1f,
                        scaleX = (i % 2 == 0) ? WIDTH : BORDER_WIDTH,
                        scaleY = (i % 2 == 0) ? BORDER_WIDTH : HEIGHT,
                        shader = page.menu.manager.rainWorld.Shaders["MenuText"]
                    };
                    Container.AddChild(border[i]);
                }

                labels = new MenuLabel[6];
                Vector2 labelPos = new(WIDTH / 2f, HEIGHT / (labels.Length + 1f) - POLICE_SIZE / 2f);
                for (int i = 0; i < labels.Length; i++)
                {
                    labels[i] = new(menu, this, "", labelPos, Vector2.zero, false);
                    labels[i].label.shader = page.menu.manager.rainWorld.Shaders["MenuText"];
                    labels[i].label.anchorX = 0f;
                    labels[i].label.anchorY = 0f;
                    labels[i].label.alignment = FLabelAlignment.Center;
                    subObjects.Add(labels[i]);
                    labelPos += Vector2.up * HEIGHT / (labels.Length + 1f);
                }

                labels[0].text = menu.Translate("Game mode: ") + data.gameMode;
                labels[1].text = menu.Translate("Mod version: ") + data.version;
                labels[2].text = menu.Translate("Perks: ") + (data.perks == AllowUnlocks.Any ? menu.Translate("Allowed") : data.perks == AllowUnlocks.None ? menu.Translate("Disabled") : menu.Translate("Host decides"));
                labels[3].text = menu.Translate("Burdens: ") + (data.burdens == AllowUnlocks.Any ? menu.Translate("Allowed") : data.burdens == AllowUnlocks.None ? menu.Translate("Disabled") : menu.Translate("Host decides"));
                labels[4].text = menu.Translate("Require host's mods: ") + (data.hostMods ? menu.Translate("Yes") : menu.Translate("No"));
                labels[5].text = menu.Translate("Slugcat: ") + SlugcatStats.getSlugcatName(new(data.slugcat));
            }

            public override void GrafUpdate(float timeStacker)
            {
                base.GrafUpdate(timeStacker);

                background.SetPosition(DrawPos(timeStacker));
                for (int i = 0; i < 4; i++)
                    border[i].SetPosition(DrawPos(timeStacker) + (i < 2 ? Vector2.zero : new Vector2(WIDTH, HEIGHT)));
            }

            public override void RemoveSprites()
            {
                base.RemoveSprites();
                foreach (MenuObject obj in subObjects)
                {
                    obj.RemoveSprites();
                    RecursiveRemoveSelectables(obj);
                }
                subObjects.Clear();
                background.RemoveFromContainer();
                foreach (FSprite line in border)
                    line.RemoveFromContainer();
            }
        }
    }
}
