using System;
using System.Collections.Generic;
using Expedition;
using Menu.Remix;
using RWCustom;
using UnityEngine;
using Music;
using Menu;

namespace BingoMode
{
    public class BingoWinScreen : Menu.Menu
    {
        public MenuScene bgScene;
        public float leftOffset;
        public float rightOffset;
        public FSprite title;
        public SimpleButton continueButton;
        public BingoGrid grid;

        public BingoWinScreen(ProcessManager manager) : base(manager, BingoEnums.BingoWinScreen)
        {
            manager.musicPlayer?.MenuRequestsSong("RW_65 - Garden", 100f, 50f);
            
            pages = new List<Page>
            {
                new Page(this, null, "Main", 0)
            };
            scene = (ExpeditionGame.voidSeaFinish ? new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.Void_Slugcat_Down) : new InteractiveMenuScene(this, pages[0], MenuScene.SceneID.SleepScreen));
            scene.camPos.x -= 400f;
            pages[0].subObjects.Add(scene);

            leftOffset = Custom.GetScreenOffsets()[0];
            rightOffset = Custom.GetScreenOffsets()[1];

            title = new FSprite("bingotitle");
            title.SetAnchor(0.5f, 0f);
            title.x = 120f;
            title.y = 680f;
            title.shader = manager.rainWorld.Shaders["MenuText"];
            pages[0].Container.AddChild(title);

            grid = new BingoGrid(this, pages[0], new(leftOffset + 300f, manager.rainWorld.screenSize.y / 2f), 500f);
            pages[0].subObjects.Add(grid);

            continueButton = new SimpleButton(this, pages[0], Translate("CONTINUE"), "CONTINUE", new Vector2(rightOffset - 150f, 40f), new Vector2(100f, 30f));
            pages[0].subObjects.Add(continueButton);

            BingoData.FinishBingo();
            // Save
            // Do other stuff when completing
        }

        public override void Singal(MenuObject sender, string message)
        {
            if (message == "CONTINUE")
            {
                manager.RequestMainProcessSwitch(ExpeditionEnums.ProcessID.ExpeditionMenu);
            }
        }
    }
}
