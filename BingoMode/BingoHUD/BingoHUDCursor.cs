using RWCustom;
using UnityEngine;

namespace BingoMode.BingoHUD
{
    public class BingoHUDCursor
    {
        public FSprite cursorSprite;
        public FSprite shadow;
        public float fade;
        public float lastFade;
        public Vector2 pos;
        public Vector2 lastPos;

        public BingoHUDCursor(FContainer container, Vector2 pos)
        {
            shadow = new FSprite("Futile_White", true);
            shadow.shader = Custom.rainWorld.Shaders["FlatLight"];
            shadow.color = new Color(0f, 0f, 0f);
            shadow.scale = 4f;
            container.AddChild(shadow);
            cursorSprite = new FSprite("Cursor", true);
            cursorSprite.anchorX = 0f;
            cursorSprite.anchorY = 1f;
            container.AddChild(cursorSprite);
        }

        public void Update()
        {
            lastPos = pos;
            pos = Futile.mousePosition;
            lastFade = fade;
            fade = Custom.LerpAndTick(fade, BingoHUDMain.Toggled ? 1f : 0f, 0.01f, 0.033333335f);
        }

        public void GrafUpdate(float timeStacker)
        {
            cursorSprite.x = Futile.mousePosition.x + 0.01f;
            cursorSprite.y = Futile.mousePosition.y + 0.01f;
            shadow.x = Futile.mousePosition.x + 3.01f;
            shadow.y = Futile.mousePosition.y - 8.01f;
            float num = Custom.SCurve(Mathf.Lerp(lastFade, fade, timeStacker), 0.6f);
            cursorSprite.alpha = num;
            shadow.alpha = Mathf.Pow(num, 3f) * 0.3f;
        }

        public void RemoveSprites()
        {
            shadow.RemoveFromContainer();
            cursorSprite.RemoveFromContainer();
        }
    }
}
