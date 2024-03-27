using Expedition;
using Menu;
using Menu.Remix.MixedUI;
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

        public BingoButton(Menu.Menu menu, MenuObject owner, Vector2 pos, Vector2 size, string singalText, int xCoord, int yCoord) : base(menu, owner, pos, size)
        {
            this.singalText = singalText;
            x = xCoord;
            y = yCoord;
            challenge = BingoHooks.GlobalBoard.challengeGrid[x, y];

            labelColor = Menu.Menu.MenuColor(Menu.Menu.MenuColors.MediumGrey);

            bkgRect = new RoundedRect(menu, owner, pos, size, true);
            subObjects.Add(bkgRect);
            selectRect = new RoundedRect(menu, owner, pos, size, false);
            subObjects.Add(selectRect);
            textLabel = new MenuLabel(menu, owner, "", pos, size, false);
            subObjects.Add(textLabel);

            //randomize = new SymbolButton(menu, this, "Sandbox_Randomize", "randomize_single", new Vector2(size.x * 0.1f, size.y * 0.7f));
            //randomize.size = size / 4f;
            //randomize.roundedRect.size = randomize.size;
            //randomize.symbolSprite.scale = 0.03f * randomize.size.x;
            //subObjects.Add(randomize);
            UpdateText();
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
            base.Update();
            buttonBehav.Update();
            bkgRect.fillAlpha = Mathf.Lerp(0.3f, 0.6f, buttonBehav.col);
            //bkgRect.addSize = new Vector2(10f, 6f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * 3.1415927f)) * (buttonBehav.clicked ? 0f : 1f);
            selectRect.addSize = new Vector2(-10f, -6f) * (buttonBehav.sizeBump + 0.5f * Mathf.Sin(buttonBehav.extraSizeBump * 3.1415927f)) * (buttonBehav.clicked ? 0f : 1f);
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
        }

        public override void Clicked()
        {
            Singal(this, singalText);

            menu.manager.ShowDialog(new CustomizerDialog(menu.manager, this));
        }

        public void UpdateText()
        {
            textLabel.text = challenge.description.WrapText(false, size.x * 0.8f);
        }
    }
}
