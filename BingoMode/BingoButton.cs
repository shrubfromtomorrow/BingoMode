using BingoMode.Challenges;
using Expedition;
using Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace BingoMode
{
    public class BingoButton : ButtonTemplate
    {
        public RoundedRect bkgRect;
        public RoundedRect selectRect;
        public MenuLabel textLabel;
        public HSLColor labelColor;
        public Challenge challenge;
        public string singalText; // singal.
        public int x;
        public int y;
        public Phrase phrase;
        public FSprite[] boxSprites;
        public FLabel infoLabel;
        public bool boxVisible;
        public bool mouseOver;
        public bool lastMouseOver;
        public TriangleMesh[] teamColors;
        public Vector2[] corners;
        public bool showBG;
        public List<TriangleMesh> visible;

        public BingoButton(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, string singalText, int xCoord, int yCoord) : base(menu, owner, pos, size)
        {
            this.singalText = singalText;
            x = xCoord;
            y = yCoord;
            challenge = BingoHooks.GlobalBoard.challengeGrid[x, y];

            labelColor = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey);

            visible = [];
            showBG = true;
            corners = new Vector2[4];
            corners[0] = pos + new Vector2(-size.x / 2f, -size.x / 2f);
            corners[1] = pos + new Vector2(-size.x / 2f, size.x / 2f);
            corners[2] = pos + new Vector2(size.x / 2f, -size.x / 2f);
            corners[3] = pos + new Vector2(size.x / 2f, size.x / 2f);

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
                    alpha = 0.3f
                };
                Container.AddChild(teamColors[i]);
            }

            bkgRect = new RoundedRect(menu, owner, pos, size, true);
            subObjects.Add(bkgRect);
            selectRect = new RoundedRect(menu, owner, pos, size, false);
            subObjects.Add(selectRect);
            textLabel = new MenuLabel(menu, owner, "", pos, size, false);
            subObjects.Add(textLabel);

            boxSprites = new FSprite[5];
            int width = 400;
            int height = 75;
            infoLabel = new FLabel(Custom.GetFont(), "")
            {
                anchorX = 0.5f,
                anchorY = 0.5f,
                alignment = FLabelAlignment.Center
            };
            Container.AddChild(infoLabel);
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
                scaleX = width,
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
                Container.AddChild(boxSprites[i]);
                if (i > 0)
                {
                    boxSprites[i].scaleX += 1f;
                    boxSprites[i].scaleY += 1f;
                    boxSprites[i].color = Color.white;
                    boxSprites[i].shader = Custom.rainWorld.Shaders["MenuText"];
                }
            }

            (challenge as BingoChallenge).DescriptionUpdated += UpdateText;
            (challenge as BingoChallenge).ChallengeCompleted += UpdateTeamColors;
            (challenge as BingoChallenge).ChallengeFailed += UpdateTeamColors;

            UpdateText();
            UpdateTeamColors();
            buttonBehav.greyedOut = menu is not ExpeditionMenu;
        }

        public void UpdateTeamColors()
        {
            Plugin.logger.LogMessage($"Updating team colors for " + challenge);
            visible = [];
            bool g = false;
            for (int i = 0; i < teamColors.Length; i++)
            {
                teamColors[i].isVisible = false;
                if ((challenge as BingoChallenge).TeamsCompleted[i])
                {
                    Plugin.logger.LogMessage("Making it visible!");
                    visible.Add(teamColors[i]);
                    g = true;
                }
            }
            if (g) showBG = false;
        }

        public string SplitString(string s)
        {
            string modified = "";
            int limit = 0;
            foreach (var c in s)
            {
                limit += 6;
                if (limit > size.x * 0.8f)
                {
                    modified += "\n";
                    limit = 0;
                }
                modified += c;
            }
            return modified;
        }

        // Stolen from SimpleButton
        public void SetSize(Vector2 newSize)
        {
            size = newSize;
            bkgRect.size = size;
            selectRect.size = size;
            textLabel.size = size;
            textLabel.label.scale = size.magnitude * 2f;
        }

        // Stolen from SimpleButton
        public override void Update()
        {
            lastMouseOver = mouseOver;
            mouseOver = IsMouseOverMe;
            base.Update();
            buttonBehav.Update();
            bkgRect.fillAlpha = showBG ? Mathf.Lerp(0.3f, 0.6f, buttonBehav.col) : 0f;
            //bkgRect.addSize = new Vector2(10f, 6f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * 3.1415927f)) * (buttonBehav.clicked ? 0f : 1f);
            selectRect.addSize = new Vector2(-10f, -6f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * 3.1415927f)) * (buttonBehav.clicked ? 0f : 1f);

            boxVisible = mouseOver && !menu.manager.sideProcesses.Any(x => x is CustomizerDialog);
            if (boxVisible && lastMouseOver != mouseOver)
            {
                for (int i = 0; i < boxSprites.Length; i++)
                {
                    boxSprites[i].MoveToFront();
                }
                infoLabel.MoveToFront();
            }
        }

        // Mostly stolen from SimpleButton
        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            textLabel.label.color = InterpColor(timeStacker, labelColor);
            Color color = Color.Lerp(Menu.Menu.MenuRGB(Menu.Menu.MenuColors.Black), Menu.Menu.MenuRGB(Menu.Menu.MenuColors.White), Mathf.Lerp(buttonBehav.lastFlash, buttonBehav.flash, timeStacker));
            for (int i = 0; i < 9; i++)
            {
                bkgRect.sprites[i].color = color;
            }
            float num = 0.5f + 0.5f * Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * 3.1415927f * 2f);
            num *= buttonBehav.sizeBump;
            for (int j = 0; j < 8; j++)
            {
                selectRect.sprites[j].color = MyColor(timeStacker);
                selectRect.sprites[j].alpha = num;
            }

            Vector2 pagePos = Vector2.Lerp(page.lastPos, page.pos, timeStacker);
            // Phrase biz
            if (phrase != null)
            {
                phrase.centerPos = Vector2.Lerp(lastPos, pos, timeStacker) + pagePos + new Vector2(size.x / 2f, size.y / 2f);
                phrase.Draw();
            }

            // Info bocks
            for (int i = 0; i < boxSprites.Length; i++)
            {
                boxSprites[i].isVisible = boxVisible;
            }
            infoLabel.isVisible = boxVisible;
            float yStep = boxSprites[3].scaleY / 2f;
            boxSprites[0].SetPosition(pos + new Vector2(size.x + 5, -yStep + size.y / 2f));
            boxSprites[1].SetPosition(pos + new Vector2(size.x + 5, yStep + size.y / 2f));
            boxSprites[2].SetPosition(pos + new Vector2(size.x + 5, -yStep + size.y / 2f));
            boxSprites[3].SetPosition(pos + new Vector2(size.x + 5, -yStep + size.y / 2f));
            boxSprites[4].SetPosition(pos + new Vector2(size.x + 5 + boxSprites[0].scaleX, -yStep + size.y / 2f));
            infoLabel.SetPosition(pos + new Vector2(size.x + 5 + boxSprites[0].scaleX / 2f, size.y / 2f));

            if (visible.Count == 0) return;
            float dist = size.x / visible.Count;
            float halfStep = dist * 0.3f;
            Vector2 offset = new Vector2(size.x / 2f, size.y / 2f) + pagePos;
            int pixelStep = 2;
            for (int i = 0; i < visible.Count; i++)
            {
                visible[i].isVisible = true;

                int isFirst = i == 0 ? 0 : 1;
                int invIsFirst = i == 0 ? 1 : 0;
                int isLast = i == visible.Count - 1 ? 0 : 1;
                int invIsLast = i == visible.Count - 1 ? 1 : 0;

                visible[i].MoveVertice(0, corners[0] + offset + new Vector2(dist * i - halfStep * isFirst + pixelStep * invIsFirst, pixelStep));
                visible[i].MoveVertice(1, corners[1] + offset + new Vector2(dist * i + halfStep * isFirst + pixelStep * invIsFirst, -pixelStep));

                visible[i].MoveVertice(2, corners[0] + offset + new Vector2(dist * (i + 1) - halfStep * isLast - pixelStep * invIsLast, pixelStep));
                visible[i].MoveVertice(3, corners[1] + offset + new Vector2(dist * (i + 1) + halfStep * isLast - pixelStep * invIsLast, -pixelStep));
            }
        }

        public override void RemoveSprites()
        {
            base.RemoveSprites();
            if (phrase != null)
            {
                phrase.ClearAll();
            }
            foreach (var g in boxSprites)
            {
                g.RemoveFromContainer();
            }
            infoLabel.RemoveFromContainer();
            foreach (var g in teamColors)
            {
                g.RemoveFromContainer();
            }
            (challenge as BingoChallenge).DescriptionUpdated -= UpdateText;
            (challenge as BingoChallenge).ChallengeCompleted -= UpdateTeamColors;
            (challenge as BingoChallenge).ChallengeFailed -= UpdateTeamColors;
        }

        public override void Clicked()
        {
            Singal(this, singalText);
            menu.manager.ShowDialog(new CustomizerDialog(menu.manager, this));
        }

        public void UpdateText()
        {
            if (phrase != null)
            {
                phrase.ClearAll();
                phrase = null;
            }
            phrase = (challenge as BingoChallenge).ConstructPhrase();
            if (phrase != null)
            {
                phrase.AddAll(Container);
                Plugin.logger.LogMessage(size.x);
                phrase.scale = size.x / 100f;
            }
            textLabel.text = phrase == null ? challenge.description.WrapText(false, size.x * 0.8f) : "";
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
        }
    }
}
