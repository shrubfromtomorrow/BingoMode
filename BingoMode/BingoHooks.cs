using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Expedition;
using Menu;
using Menu.Remix;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace BingoMode
{
    public class BingoHooks
    {
        public static BingoBoard GlobalBoard;

        public static ConditionalWeakTable<ExpeditionMenu, BingoPage> bingoPage = new ();
        public static ConditionalWeakTable<CharacterSelectPage, HoldButton> newBingoButton = new ();

        public static void EarlyApply()
        {
            // Ignoring bingo challenges in bingo (has to be done here)
            On.Expedition.ChallengeOrganizer.SetupChallengeTypes += ChallengeOrganizer_SetupChallengeTypes;
        }

        public static void ChallengeOrganizer_SetupChallengeTypes(On.Expedition.ChallengeOrganizer.orig_SetupChallengeTypes orig)
        {
            BingoData.availableBingoChallenges ??= new();
            orig.Invoke();
            BingoData.availableBingoChallenges.AddRange(ChallengeOrganizer.availableChallengeTypes);
            ChallengeOrganizer.availableChallengeTypes = ChallengeOrganizer.availableChallengeTypes.Where(x => !x.GetType().Name.StartsWith("Bingo")).ToList();
        }

        public static void Apply()
        {
            // Adding the bingo page to exp menu
            On.Menu.ExpeditionMenu.ctor += ExpeditionMenu_ctor;
            On.Menu.ExpeditionMenu.InitMenuPages += ExpeditionMenu_InitMenuPages;
            On.Menu.ExpeditionMenu.Singal += ExpeditionMenu_Singal;

            // Adding new bingo button to the character select page
            //On.Menu.ChallengeSelectPage.Singal += ChallengeSelectPage_Singal;
            On.Menu.CharacterSelectPage.UpdateStats += CharacterSelectPage_UpdateStats;
            On.Menu.CharacterSelectPage.ClearStats += CharacterSelectPage_ClearStats;
            On.Menu.CharacterSelectPage.UpdateSelectedSlugcat += CharacterSelectPage_UpdateSelectedSlugcat;

            // Win and lose screens
            IL.WinState.CycleCompleted += WinState_CycleCompleted;
            IL.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;

            // Add Bingo HUD
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
            // Stop the base Expedition HUD from appearing
            IL.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHudIL;

            // 
        }

        public static void CharacterSelectPage_UpdateSelectedSlugcat(On.Menu.CharacterSelectPage.orig_UpdateSelectedSlugcat orig, CharacterSelectPage self, int num)
        {
            orig.Invoke(self, num);

            BingoData.FillPossibleTokens(ExpeditionData.slugcatPlayer);
        }

        public static void ProcessManager_PostSwitchMainProcess(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                x => x.MatchLdsfld("Expedition.ExpeditionEnums/ProcessID", "ExpeditionMenu")
                ))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<ProcessManager.ProcessID, ProcessManager>>((orig, self) =>
                {
                    if (orig == BingoEnums.BingoWinScreen)
                    {
                        self.currentMainLoop = new BingoWinScreen(self);
                        self.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
                    }
                });
            }
            else Plugin.logger.LogError(nameof(ProcessManager_PostSwitchMainProcess) + " Threw :(( " + il);
        }

        public static void WinState_CycleCompleted(ILContext il)
        {
            ILCursor c = new(il);
            ILCursor b = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld("Expedition.ExpeditionEnums/ProcessID", "ExpeditionWinScreen")
                ))
            {
                c.EmitDelegate<Func<ProcessManager.ProcessID, ProcessManager.ProcessID>>((orig) =>
                {
                    if (orig == ExpeditionEnums.ProcessID.ExpeditionWinScreen)
                    {
                        orig = BingoEnums.BingoWinScreen;
                    }

                    return orig;
                });
            }
            else Plugin.logger.LogError(nameof(WinState_CycleCompleted) + " Threw 1 :(( " + il);

            if (b.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("Cycle complete, saving run data")
                ))
            {
                b.EmitDelegate(() =>
                {
                    if (BingoData.BingoMode) 
                    {
                        ExpeditionGame.expeditionComplete = GlobalBoard.CheckWin();
                    }
                });
            }
            else Plugin.logger.LogError(nameof(WinState_CycleCompleted) + " Threw 2 :(( " + il);
        }

        public static void HUD_InitSinglePlayerHudIL(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld("ModManager", "Expedition")
                ))
            {
                c.EmitDelegate<Func<bool, bool>>((orig) =>
                {
                    orig &= !BingoData.BingoMode;

                    return orig;
                });
            }
            else Plugin.logger.LogError(nameof(HUD_InitSinglePlayerHudIL) + " Threw :(( " + il);
        }

        public static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig.Invoke(self, cam);

            if (BingoData.BingoMode)
            {
                self.AddPart(new BingoHUD(self));
            }
        }

        public static void ExpeditionMenu_ctor(On.Menu.ExpeditionMenu.orig_ctor orig, ExpeditionMenu self, ProcessManager manager)
        {
            orig.Invoke(self, manager);

            self.pages.Add(new Page(self, null, "BINGO", 4));
        }

        public static void ExpeditionMenu_InitMenuPages(On.Menu.ExpeditionMenu.orig_InitMenuPages orig, ExpeditionMenu self)
        {
            orig.Invoke(self);

            GlobalBoard = new BingoBoard();

            if (!bingoPage.TryGetValue(self, out _))
            {
                bingoPage.Add(self, new BingoPage(self, self.pages[4], default));
            }
            bingoPage.TryGetValue(self, out var page);
            self.pages[4].subObjects.Add(page);
            self.pages[4].pos.x -= 1500f;
            Plugin.logger.LogMessage("Initialized bingo menu page " + self.pages[4] + " " + page);
        }

        public static void ExpeditionMenu_Singal(On.Menu.ExpeditionMenu.orig_Singal orig, ExpeditionMenu self, MenuObject sender, string message)
        {
            orig.Invoke(self, sender, message);

            if (message == "NEWBINGO")
            {
                self.UpdatePage(4);
                self.MovePage(new Vector2(1500f, 0f));
                GlobalBoard.GenerateBoard(GlobalBoard.size);
                if (bingoPage.TryGetValue(self, out var page))
                {
                    page.grid.RemoveSprites();
                    page.RemoveSubObject(page.grid);
                    page.grid = new BingoGrid(self, page, new(self.manager.rainWorld.screenSize.x / 2f, self.manager.rainWorld.screenSize.y / 2f), 500f);
                    page.subObjects.Add(page.grid);
                }
            }
        }

        // Creating butone
        public static void CharacterSelectPage_UpdateStats(On.Menu.CharacterSelectPage.orig_UpdateStats orig, CharacterSelectPage self)
        {
            orig.Invoke(self);

            self.confirmExpedition.pos.x += 90;

            if (!newBingoButton.TryGetValue(self, out _))
            {
                newBingoButton.Add(self, new HoldButton(self.menu, self, "NEW\nBINGO", "NEWBINGO", new Vector2(590f, 180f), 30f));
            }
            newBingoButton.TryGetValue(self, out var button);
            self.subObjects.Add(button);
        }

        public static void CharacterSelectPage_ClearStats(On.Menu.CharacterSelectPage.orig_ClearStats orig, CharacterSelectPage self)
        {
            orig.Invoke(self);

            if (newBingoButton.TryGetValue(self, out var button) && button != null)
            {
                button.RemoveSprites();
                button.RemoveSubObject(button);
                newBingoButton.Remove(self);
            }
        }
    }
}
