using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BingoMode.Challenges;
using Expedition;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace BingoMode
{
    public class BingoPage : PositionedMenuObject
    {
        public ExpeditionMenu expMenu;
        public BingoBoard board;
        public BingoGrid grid;
        public int size;
        public FSprite pageTitle;
        public SymbolButton rightPage;
        public BigSimpleButton startGame;
        public SymbolButton randomize;
        public OpHoldButton unlocksButton;
        public UIelementWrapper unlockWrapper;
        public MenuTabWrapper menuTabWrapper;
        public SymbolButton plusButton;
        public SymbolButton minusButton;

        public BingoPage(Menu.Menu menu, MenuObject owner, Vector2 pos) : base(menu, owner, pos)
        {
            expMenu = menu as ExpeditionMenu;
            board = BingoHooks.GlobalBoard;
            size = board.size;
            BingoData.BingoMode = false;

            pageTitle = new FSprite("bingotitle");
            pageTitle.SetAnchor(0.5f, 0f);
            pageTitle.x = 683f;
            pageTitle.y = 680f;
            pageTitle.shader = menu.manager.rainWorld.Shaders["MenuText"];
            Container.AddChild(pageTitle);

            rightPage = new SymbolButton(menu, this, "Big_Menu_Arrow", "GOBACK", new Vector2(783f, 685f));
            rightPage.symbolSprite.rotation = 90f;
            rightPage.size = new Vector2(45f, 45f);
            rightPage.roundedRect.size = rightPage.size;
            subObjects.Add(rightPage);

            randomize = new SymbolButton(menu, this, "Sandbox_Randomize", "RANDOMIZE", new Vector2(563f, 690f));
            randomize.size = new Vector2(30f, 30f);
            randomize.roundedRect.size = randomize.size;
            subObjects.Add(randomize);

            //grid = new BingoGrid(menu, this, new(menu.manager.rainWorld.screenSize.x / 2f, menu.manager.rainWorld.screenSize.y / 2f), 500f);
            //subObjects.Add(grid);

            startGame = new BigSimpleButton(menu, this, "BEGIN", "STARTBINGO",
                new Vector2(menu.manager.rainWorld.screenSize.x * 0.75f, 40f),
                new Vector2(150f, 40f), FLabelAlignment.Center, true);
            subObjects.Add(startGame);

            menuTabWrapper = new MenuTabWrapper(menu, this);
            subObjects.Add(menuTabWrapper);
            unlocksButton = new OpHoldButton(new Vector2(menu.manager.rainWorld.screenSize.x * 0.75f, 100f), new Vector2(150f, 50f), menu.Translate("CONFIGURE<LINE>PERKS & BURDENS").Replace("<LINE>", "\r\n"), 20f);
            unlocksButton.OnPressDone += UnlocksButton_OnPressDone;
            unlocksButton.description = " ";
            unlockWrapper = new UIelementWrapper(this.menuTabWrapper, this.unlocksButton);

            minusButton = new SymbolButton(menu, this, "minus", "REMOVESIZE", new Vector2(643f, 620f));
            minusButton.size = new Vector2(40f, 40f);
            minusButton.roundedRect.size = this.minusButton.size;
            subObjects.Add(this.minusButton);
            plusButton = new SymbolButton(menu, this, "plus", "ADDSIZE", new Vector2(693f, 620f));
            plusButton.size = new Vector2(40f, 40f);
            plusButton.roundedRect.size = this.plusButton.size;
            subObjects.Add(this.plusButton);
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);

            if (message == "GOBACK")
            {
                expMenu.UpdatePage(1);
                expMenu.MovePage(new Vector2(-1500f, 0f));
            }

            if (message == "STARTBINGO")
            {
                if (ModManager.JollyCoop && ModManager.CoopAvailable)
                {
                    for (int i = 1; i < menu.manager.rainWorld.options.JollyPlayerCount; i++)
                    {
                        menu.manager.rainWorld.RequestPlayerSignIn(i, null);
                    }
                    for (int j = menu.manager.rainWorld.options.JollyPlayerCount; j < 4; j++)
                    {
                        menu.manager.rainWorld.DeactivatePlayer(j);
                    }
                }

                menu.manager.arenaSitting = null;
                menu.manager.rainWorld.progression.currentSaveState = null;
                menu.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = ExpeditionData.slugcatPlayer;
                menu.manager.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);

                List<string> bannedRegions = [];
                foreach (var ch in ExpeditionData.challengeList)
                {
                    if (ch is BingoNoRegionChallenge r) bannedRegions.Add(r.region.Value);
                    if (ch is BingoAllRegionsExcept g) bannedRegions.Add(g.region.Value);
                }
            reset:
                ExpeditionData.startingDen = ExpeditionGame.ExpeditionRandomStarts(menu.manager.rainWorld, ExpeditionData.slugcatPlayer);
                foreach (var banned in bannedRegions) 
                {
                    if (ExpeditionData.startingDen.Substring(0, 2).ToLowerInvariant() == banned.ToLowerInvariant()) goto reset;
                }

                foreach (var kvp in menu.manager.rainWorld.progression.mapDiscoveryTextures)
                {
                    menu.manager.rainWorld.progression.mapDiscoveryTextures[kvp.Key] = null;
                }

                BingoData.InitializeBingo();
                ExpeditionGame.PrepareExpedition();
                ExpeditionData.AddExpeditionRequirements(ExpeditionData.slugcatPlayer, false);
                ExpeditionData.earnedPassages++;
                BingoData.BingoSaves[ExpeditionData.slugcatPlayer] = BingoHooks.GlobalBoard.size;
                Expedition.Expedition.coreFile.Save(false);
                menu.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
                menu.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
                menu.PlaySound(SoundID.MENU_Start_New_Game);
            }

            if (message == "RANDOMIZE")
            {
                Regen();
            }

            if (message == "ADDSIZE")
            {
                BingoHooks.GlobalBoard.size += 1;
                Regen();
            }

            if (message == "REMOVESIZE")
            {
                BingoHooks.GlobalBoard.size = Mathf.Max(1, BingoHooks.GlobalBoard.size - 1);
                Regen();
            }

            // Also initialize bingo on continue
        }

        public void Regen()
        {
            BingoHooks.GlobalBoard.GenerateBoard(BingoHooks.GlobalBoard.size);
            if (grid != null)
            {
                grid.RemoveSprites();
                RemoveSubObject(grid);
                grid = null;
            }
            grid = new BingoGrid(menu, page, new(menu.manager.rainWorld.screenSize.x / 2f, menu.manager.rainWorld.screenSize.y / 2f), 500f);
            subObjects.Add(grid);
            menu.PlaySound(SoundID.MENU_Next_Slugcat);
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);
            //pageTitle.SetPosition(Vector2.Lerp(owner.page.lastPos, owner.page.pos, timeStacker) + new Vector2(680f, 680f));

            pageTitle.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 680f;
            pageTitle.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 680f;
        }

        public override void RemoveSprites()
        {
            base.RemoveSprites();
            pageTitle.RemoveFromContainer();
        }

        public void UnlocksButton_OnPressDone(UIfocusable trigger)
        {
            UnlockDialog unlockDialog = new UnlockDialog(menu.manager, (menu as ExpeditionMenu).challengeSelect);
            unlocksButton.Reset();
            unlocksButton.greyedOut = true;
            menu.manager.ShowDialog(unlockDialog);
            //unlockDialog.pages[0].Container.AddChild(this.levelSprite);
            //unlockDialog.pages[0].Container.AddChild(this.levelSprite2);
            //unlockDialog.pages[0].Container.AddChild(this.levelContainer);
            //unlockDialog.pages[0].Container.AddChild(this.currentLevelLabel.label);
            //unlockDialog.pages[0].Container.AddChild(this.nextLevelLabel.label);
            //unlockDialog.pages[0].Container.AddChild(this.levelOverloadLabel.label);
            //unlockDialog.pages[0].Container.AddChild(this.pointsLabel.label);
        }
    }
}
