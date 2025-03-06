using RWCustom;
using UnityEngine;

namespace BingoMode.BingoHUD
{
    using System;

    public class BingoHUDButton
    {
        public BingoHUDMain hud;
        public float alpha;
        public float lastAlpha;
        public Vector2 size;
        public Vector2 pos;
        public Vector2 lastPos;
        public Vector2 ogPos;
        public Vector2[] corners;
        public FSprite background;
        public FLabel text;
        public FSprite[] border;
        public bool lastMouseOver;
        public bool mouseOver;
        public Action callback;
        public bool active;

        public BingoHUDButton(BingoHUDMain hud, Vector2 pos, string text, Action callback)
        {
            this.hud = hud;
            this.callback = callback;
            size = new Vector2(80f, 30f);
            FContainer container = hud.hud.fContainers[1];

            background = new FSprite("pixel") { scaleX = size.x, scaleY = size.y, color = new Color(0.01f, 0.01f, 0.01f), alpha = 0.7f };
            container.AddChild(background);

            this.text = new FLabel(Custom.GetFont(), text) { alignment = FLabelAlignment.Center };
            container.AddChild(this.text);

            border = new FSprite[4];
            for (int i = 0; i < border.Length; i++)
            {
                border[i] = new FSprite("pixel")
                {
                    scaleX = (i < 2) ? size.x : 2f,
                    anchorX = (i < 2) ? 0f : 0.5f,
                    scaleY = (i < 2) ? 2f : size.y,
                    anchorY = (i < 2) ? 0.5f : 0f
                };
                container.AddChild(border[i]);
            }

            alpha = 0f;
            ogPos = pos;
            this.pos = ogPos;
            lastPos = ogPos;

            corners = new Vector2[3];
            corners[0] = pos + new Vector2(-size.x / 2f, -size.y / 2f);
            corners[1] = pos + new Vector2(-size.x / 2f, size.y / 2f);
            corners[2] = pos + new Vector2(size.x / 2f, -size.y / 2f);
            border[0].SetPosition(corners[0]);
            border[1].SetPosition(corners[1]);
            border[2].SetPosition(corners[0]);
            border[3].SetPosition(corners[2]);
        }

        public void Update()
        {
            lastMouseOver = mouseOver;
            mouseOver = hud.mousePosition.x > pos.x - size.x / 2f && hud.mousePosition.y > pos.y - size.y / 2f
                    && hud.mousePosition.x < pos.x + size.x / 2f && hud.mousePosition.y < pos.y + size.y / 2f;

            lastPos = pos;
            pos = ogPos;
            
            if (alpha > 0.5f && mouseOver && hud.MouseLeftDown)
            {
                callback.Invoke();
                hud.hud.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
            }
        }

        public void Draw(float timeStacker)
        {
            // Position
            Vector2 drawPos = Vector2.Lerp(lastPos, pos, timeStacker);

            background.SetPosition(drawPos);
            text.SetPosition(drawPos);

            corners[0] = drawPos + new Vector2(-size.x / 2f, -size.y / 2f);
            corners[1] = drawPos + new Vector2(-size.x / 2f, size.y / 2f);
            corners[2] = drawPos + new Vector2(size.x / 2f, -size.y / 2f);
            border[0].SetPosition(corners[0]);
            border[1].SetPosition(corners[1]);
            border[2].SetPosition(corners[0]);
            border[3].SetPosition(corners[2]);

            // Alpha
            float drawAlpha = Mathf.Lerp(lastAlpha, alpha, timeStacker) * (mouseOver && hud.mouseDown ? 0.3f : mouseOver ? 0.6f : 1f);
            background.alpha = drawAlpha * 0.7f;
            text.alpha = drawAlpha;
            for (int i = 0; i < border.Length; i++)
            {
                border[i].alpha = drawAlpha;
            }
        }

        public void Remove()
        {
            background.RemoveFromContainer();
            text.RemoveFromContainer();
            for (int i = 0; i < border.Length; i++)
            {
                border[i].RemoveFromContainer();
            }
        }
    }
}
