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

            // Stop the base Expedition HUD from appearing
            IL.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;

            // Nuh uh
            IL.WinState.CycleCompleted += WinState_CycleCompleted;

            // Adding HUD
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud1;
        }

        public static void HUD_InitSinglePlayerHud1(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            orig.Invoke(self, cam);

            if (BingoData.BingoMode)
            {
                self.AddPart(new BingoHUD(self));
            }
        }

        public static void WinState_CycleCompleted(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(
                x => x.MatchLdstr("Cycle complete, saving run data")
                ))
            {
                c.EmitDelegate(() =>
                {
                    if (ExpeditionGame.expeditionComplete && BingoData.BingoMode) ExpeditionGame.expeditionComplete = false;
                });
            }
            else Plugin.logger.LogError(nameof(WinState_CycleCompleted) + " Threw :(( " + il);
        }

        public static void HUD_InitSinglePlayerHud(ILContext il)
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
            else Plugin.logger.LogError(nameof(HUD_InitSinglePlayerHud) + " Threw :(( " + il);
        }

        public static void ExpeditionMenu_ctor(On.Menu.ExpeditionMenu.orig_ctor orig, ExpeditionMenu self, ProcessManager manager)
        {
            orig.Invoke(self, manager);

            GlobalBoard = new BingoBoard();

            self.pages.Add(new Page(self, null, "BINGO", 4));
        }

        public static void ExpeditionMenu_InitMenuPages(On.Menu.ExpeditionMenu.orig_InitMenuPages orig, ExpeditionMenu self)
        {
            orig.Invoke(self);

            if (!bingoPage.TryGetValue(self, out _))
            {
                bingoPage.Add(self, null);
            }
            bingoPage.TryGetValue(self, out var page);
            page = new BingoPage(self, self.pages[4], default);
            self.pages[4].subObjects.Add(page);
            self.pages[4].pos.x -= 1500f;
            Plugin.logger.LogMessage("Initialized new menu page " + self.pages[4] + " " + page);
        }

        public static void ExpeditionMenu_Singal(On.Menu.ExpeditionMenu.orig_Singal orig, ExpeditionMenu self, MenuObject sender, string message)
        {
            orig.Invoke(self, sender, message);

            if (message == "NEWBINGO")
            {
                self.UpdatePage(4);
                self.MovePage(new Vector2(1500f, 0f));
            }
        }

        // Creating butone
        public static void CharacterSelectPage_UpdateStats(On.Menu.CharacterSelectPage.orig_UpdateStats orig, CharacterSelectPage self)
        {
            orig.Invoke(self);
            Plugin.logger.LogMessage("ASS");

            self.confirmExpedition.pos.x += 90;

            if (!newBingoButton.TryGetValue(self, out _))
            {
                newBingoButton.Add(self, null);
            }
            newBingoButton.TryGetValue(self, out var button);
            button = new HoldButton(self.menu, self, "NEW\nBINGO", "NEWBINGO", new Vector2(590f, 180f), 30f);
            self.subObjects.Add(button);
        }

        public static void CharacterSelectPage_ClearStats(On.Menu.CharacterSelectPage.orig_ClearStats orig, CharacterSelectPage self)
        {
            orig.Invoke(self);

            if (newBingoButton.TryGetValue(self, out var button) && button != null)
            {
                button.RemoveSprites();
                button.RemoveSubObject(button);
            }
        }
    }
}
