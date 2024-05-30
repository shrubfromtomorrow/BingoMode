using Menu;
using Menu.Remix.MixedUI;
using RWCustom;
using UnityEngine;

namespace BingoMode
{
    public class InfoDialog : Dialog
    {
        float num;
        SimpleButton closeButton;
        FLabel infoText;

        public InfoDialog(ProcessManager manager, string message) : base(manager)
        {
            num = 85f;
            float num2 = LabelTest.GetWidth(Translate("CLOSE"), false) + 10f;
            if (num2 > num)
            {
                num = num2;
            }
            darkSprite.alpha = 0.95f;

            float yTop = 578f;

            closeButton = new SimpleButton(this, pages[0], Translate("CLOSE"), message == "The host quit the game." ? "QUITGAEM" : "CLOSE", new Vector2(683f - num / 2f, yTop - 305f), new Vector2(num, 35f));
            pages[0].subObjects.Add(closeButton);
            infoText = new FLabel(Custom.GetFont(), message)
            {
                anchorX = 0.5f,
                anchorY = 0.5f,
                alignment = FLabelAlignment.Center
            };
            infoText.SetPosition(new Vector2(683f, yTop - 185f));

            container.AddChild(infoText);
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);
            switch (message)
            {
                case "CLOSE":
                    manager.StopSideProcess(this);
                    break;
                case "QUITGAEM":
                    Custom.rainWorld.processManager.RequestMainProcessSwitch(ProcessManager.ProcessID.MainMenu);
                    manager.StopSideProcess(this);
                    break;
            }
        }
    }
}
