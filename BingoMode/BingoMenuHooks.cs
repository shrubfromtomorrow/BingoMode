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

namespace BingoMode
{
    public class BingoMenuHooks
    {
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
        }

        public static void ExpeditionMenu_ctor(On.Menu.ExpeditionMenu.orig_ctor orig, ExpeditionMenu self, ProcessManager manager)
        {
            orig.Invoke(self, manager);

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
            page = new(self, self.pages[4], default);
            self.pages[4].subObjects.Add(page);
            self.pages[4].pos.y -= 1500f;
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
