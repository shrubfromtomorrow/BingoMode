using Expedition;
using Menu;
using Menu.Remix;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Steamworks;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BingoMode
{
    using BingoSteamworks;
    using Challenges;

    public class BingoHooks
    {
        public static BingoBoard GlobalBoard;

        public static ConditionalWeakTable<ExpeditionMenu, BingoPage> bingoPage = new ();
        public static ConditionalWeakTable<CharacterSelectPage, HoldButton> newBingoButton = new ();

        public static void EarlyApply()
        {
            // Ignoring bingo challenges in bingo (has to be done here)
            On.Expedition.ChallengeOrganizer.SetupChallengeTypes += ChallengeOrganizer_SetupChallengeTypes;
            // Remove all hooks while at it
            //On.Expedition.ExpeditionData.ClearActiveChallengeList += ExpeditionData_ClearActiveChallengeList;
            // Add the bingo challenges when loading challenges from string
            IL.Expedition.ExpeditionCoreFile.FromString += ExpeditionCoreFile_FromStringIL;
            On.Expedition.ExpeditionCoreFile.FromString += ExpeditionCoreFile_FromString;
        }

        public static void ExpeditionCoreFile_FromString(On.Expedition.ExpeditionCoreFile.orig_FromString orig, ExpeditionCoreFile self, string saveString)
        {
            if (GlobalBoard != null)
            {
                GlobalBoard.challengeGrid = new Challenge[GlobalBoard.size, GlobalBoard.size];
                GlobalBoard.recreateList = [];
            }
            orig.Invoke(self, saveString); // IL hook
            if (GlobalBoard != null)
            {
                for (int i = 0; i < GlobalBoard.recreateList.Count; i++)
                {
                    Plugin.logger.LogMessage(i + " - " + GlobalBoard.recreateList[i]);
                }
            }
            if (GlobalBoard != null) GlobalBoard.RecreateFromList();
        }

        public static void ExpeditionCoreFile_FromStringIL(ILContext il)
        {
            ILCursor c = new(il);
            ILCursor b = new(il);
            ILCursor a = new(il);
            ILCursor d = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld("Expedition.ChallengeOrganizer", "availableChallengeTypes")
                ))
            {
                c.EmitDelegate<Func<List<Challenge>, List<Challenge>>>((orig) =>
                {
                    List<Challenge> newList = [.. orig, .. BingoData.availableBingoChallenges];

                    return newList;
                });
            }
            else Plugin.logger.LogError("ExpeditionCoreFile_FromStringIL 1 threw!!! " + il);

            if (b.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("[CONTENT]")
                ))
            {
                b.Index += 5;
                b.Emit(OpCodes.Ldloc, 0);
                b.Emit(OpCodes.Ldloc, 7);
                b.Emit(OpCodes.Ldelem_Ref);
                b.EmitDelegate<Action<string>>((text) =>
                {
                    BingoData.BingoSaves = [];
                    try
                    {
                        if (text.StartsWith("BINGOS:") && Regex.Split(text, ":")[1] != "")
                        {
                            string[] array = Regex.Split(Regex.Split(text, ":")[1], "<>");
                            for (int i = 0; i < array.Length; i++)
                            {
                                string[] array2 = array[i].Split('#');
                                int size = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                                if (array2.Length > 2)
                                {
                                    int team = int.Parse(array2[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                                    SteamNetworkingIdentity hostIdentity = new SteamNetworkingIdentity();
                                    hostIdentity.SetSteamID64(ulong.Parse(array2[3], NumberStyles.Any, CultureInfo.InvariantCulture));
                                    bool isHost = array2[4] == "1";
                                    bool lockout = array2[6] == "1";

                                    Plugin.logger.LogMessage($"Loading multiplayer bingo save from string: Team-{team}, Host-{hostIdentity.GetSteamID()}, IsHost-{isHost}, Connected players-{array[5]}");

                                    BingoData.BingoSaves[new(array2[0])] = new(size, team, hostIdentity, isHost, array[5], lockout);

                                    if (array[5] != "")
                                    {
                                        SteamFinal.ConnectedPlayers = SteamFinal.PlayersFromString(array[5]);

                                        //foreach (var player in Regex.Split(array[5], "bPlR"))
                                        //{
                                        //    SteamNetworkingIdentity playerIdentity = new();
                                        //    playerIdentity.SetSteamID64(ulong.Parse(player, NumberStyles.Any));
                                        //    SteamFinal.ConnectedPlayers.Add(playerIdentity);
                                        //}
                                    }
                                }
                                else BingoData.BingoSaves[new(array2[0])] = new(size);
                            }
                        }
                    }
                    catch (Exception e) { Plugin.logger.LogError("Failed to do that!!!" + e); }
                });
            }
            else Plugin.logger.LogError("ExpeditionCoreFile_FromStringIL 2 threw!!! " + il);

            if (a.TryGotoNext(
                x => x.MatchLdcI4(6),
                x => x.MatchNewarr<System.String>(),
                x => x.MatchDup(),
                x => x.MatchLdcI4(0),
                x => x.MatchLdstr("["),
                x => x.MatchStelemRef(),
                x => x.MatchDup(),
                x => x.MatchLdcI4(1),
                x => x.MatchLdloc(27)
                ))
            {
                a.Emit(OpCodes.Ldloc, 30);
                a.Emit(OpCodes.Ldloc, 27);
                a.EmitDelegate<Action<Challenge, SlugcatStats.Name>>((c, slug) =>
                {
                    if (GlobalBoard != null && ExpeditionData.slugcatPlayer == slug)
                    {
                        Plugin.logger.LogMessage("Adding " + ExpeditionData.allChallengeLists[slug].Last());
                        GlobalBoard.recreateList.Add(ExpeditionData.allChallengeLists[slug].Last());
                    }
                });
            }
            else Plugin.logger.LogError("ExpeditionCoreFile_FromStringIL 3 threw!!! " + il);

            if (d.TryGotoNext(MoveType.Before,
                x => x.MatchLdstr("ERROR: Problem recreating challenge type with reflection: ")
                ))
            {
                d.Emit(OpCodes.Ldloc, 28);
                d.Emit(OpCodes.Ldloc, 27);
                d.EmitDelegate<Action<string[], SlugcatStats.Name>>((array11, name) =>
                {
                    try
                    {
                        string t = array11[0];
                        Challenge challenge = (Activator.CreateInstance(BingoData.availableBingoChallenges.Find((Challenge c) => c.GetType().Name == t).GetType()) as Challenge).Generate();
                        if (challenge != null)
                        {
                            Plugin.logger.LogInfo("Regenerating broken challenge " + challenge);
                            if (!ExpeditionData.allChallengeLists.ContainsKey(name))
                            {
                                ExpeditionData.allChallengeLists.Add(name, []);
                            }
                            ExpeditionData.allChallengeLists[name].Add(challenge);
                        }
                    }
                    catch (Exception ex)
                    {
                        Plugin.logger.LogError("Error while regenerating broken challenge, call that shit inception fr how did this happen: " + ex);
                        Challenge challenge = Activator.CreateInstance(BingoData.availableBingoChallenges.Find((Challenge c) => c.GetType().Name == "BingoDodgeLeviathanChallenge").GetType()) as Challenge;
                        challenge.FromString(array11[1]);
                        if (challenge != null)
                        {
                            Plugin.logger.LogInfo("Regenerating broken challenge");
                            if (!ExpeditionData.allChallengeLists.ContainsKey(name))
                            {
                                ExpeditionData.allChallengeLists.Add(name, []);
                            }
                            ExpeditionData.allChallengeLists[name].Add(challenge);
                        }
                    }
                });
            }
            else Plugin.logger.LogError("ExpeditionCoreFile_FromStringIL 4 threw!!! " + il);
        }

       //public static void ExpeditionData_ClearActiveChallengeList(On.Expedition.ExpeditionData.orig_ClearActiveChallengeList orig)
       //{
       //    orig.Invoke();
       //
       //    try { BingoData.HookAll(ExpeditionData.challengeList, false); } catch { }
       //}

        public static void ChallengeOrganizer_SetupChallengeTypes(On.Expedition.ChallengeOrganizer.orig_SetupChallengeTypes orig)
        {
            BingoData.availableBingoChallenges ??= [];
            orig.Invoke();
            BingoData.availableBingoChallenges.AddRange(ChallengeOrganizer.availableChallengeTypes.Where(x => x is BingoChallenge).ToList());
            ChallengeOrganizer.availableChallengeTypes.RemoveAll(x => x is BingoChallenge);
        }

        public static void Apply()
        {
            // Adding the bingo page to exp menu
            On.Menu.ExpeditionMenu.ctor += ExpeditionMenu_ctor;
            On.Menu.ExpeditionMenu.InitMenuPages += ExpeditionMenu_InitMenuPages;
            On.Menu.ExpeditionMenu.Singal += ExpeditionMenu_Singal;
            On.Menu.ExpeditionMenu.UpdatePage += ExpeditionMenu_UpdatePage;

            // Adding new bingo button to the character select page
            //On.Menu.ChallengeSelectPage.Singal += ChallengeSelectPage_Singal;
            On.Menu.CharacterSelectPage.UpdateStats += CharacterSelectPage_UpdateStats;
            On.Menu.CharacterSelectPage.ClearStats += CharacterSelectPage_ClearStats;

            // Win and lose screens
            IL.WinState.CycleCompleted += WinState_CycleCompleted;
            IL.ProcessManager.PostSwitchMainProcess += ProcessManager_PostSwitchMainProcess;

            // Add Bingo HUD and Stop the base Expedition HUD from appearing
            On.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHud;
            //IL.HUD.HUD.InitSinglePlayerHud += HUD_InitSinglePlayerHudIL;

            // Ficks
            On.Menu.ChallengeSelectPage.SetUpSelectables += ChallengeSelectPage_SetUpSelectables;

            // Unlocks butone
            On.Menu.UnlockDialog.Singal += UnlockDialog_Singal;
            On.Menu.UnlockDialog.Update += UnlockDialog_Update;

            // Passage butone
            On.Menu.SleepAndDeathScreen.ctor += SleepAndDeathScreen_ctor;

            // Saving and loaading shit
            IL.Expedition.ExpeditionCoreFile.ToString += ExpeditionCoreFile_ToStringIL;
            On.Expedition.ExpeditionCoreFile.ToString += ExpeditionCoreFile_ToString;
            On.Menu.CharacterSelectPage.AbandonButton_OnPressDone += CharacterSelectPage_AbandonButton_OnPressDone;

            // Preventing expedition antics
            IL.RainWorldGame.GoToDeathScreen += RainWorldGame_GoToDeathScreen;

            // Custom den
            On.Expedition.ExpeditionGame.ExpeditionRandomStarts += ExpeditionGame_ExpeditionRandomStarts;

            // Multiplayer lobbies slider
            On.Menu.ExpeditionMenu.SliderSetValue += ExpeditionMenu_SliderSetValue;
            On.Menu.ExpeditionMenu.ValueOfSlider += ExpeditionMenu_ValueOfSlider;

            // Remove challenge preview list
            On.Menu.CharacterSelectPage.UpdateChallengePreview += CharacterSelectPage_UpdateChallengePreview;

            // Make everyone quit if the host quits
            On.ProcessManager.RequestMainProcessSwitch_ProcessID_float += ProcessManager_RequestMainProcessSwitch_ProcessID_float;
        }

        private static void ProcessManager_RequestMainProcessSwitch_ProcessID_float(On.ProcessManager.orig_RequestMainProcessSwitch_ProcessID_float orig, ProcessManager self, ProcessManager.ProcessID ID, float fadeOutSeconds)
        {
            orig.Invoke(self, ID, fadeOutSeconds);

            if (BingoData.BingoMode && SteamTest.LobbyMembers.Count > 0 && ID == ProcessManager.ProcessID.MainMenu &&
                SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby) == SteamTest.selfIdentity.GetSteamID())
            {
                foreach (var player in SteamTest.LobbyMembers)
                {
                    InnerWorkings.SendMessage("e", player);
                }
            }
        }

        private static void CharacterSelectPage_AbandonButton_OnPressDone(On.Menu.CharacterSelectPage.orig_AbandonButton_OnPressDone orig, CharacterSelectPage self, Menu.Remix.MixedUI.UIfocusable trigger)
        {
            BingoData.BingoSaves.Remove(ExpeditionData.slugcatPlayer);
            orig.Invoke(self, trigger);
        }

        private static float ExpeditionMenu_ValueOfSlider(On.Menu.ExpeditionMenu.orig_ValueOfSlider orig, ExpeditionMenu self, Slider slider)
        {
            if (slider.ID == BingoEnums.MultiplayerSlider && bingoPage.TryGetValue(self, out var page))
            {
                return page.ValueOfSlider(slider);
            }

            return orig.Invoke(self, slider);
        }

        private static void ExpeditionMenu_SliderSetValue(On.Menu.ExpeditionMenu.orig_SliderSetValue orig, ExpeditionMenu self, Slider slider, float f)
        {
            orig.Invoke(self, slider, f);

            if (slider.ID == BingoEnums.MultiplayerSlider && bingoPage.TryGetValue(self, out var page))
            {
                page.SliderSetValue(slider, f);
            }
        }

        private static string ExpeditionGame_ExpeditionRandomStarts(On.Expedition.ExpeditionGame.orig_ExpeditionRandomStarts orig, RainWorld rainWorld, SlugcatStats.Name slug)
        {
            if (BingoData.BingoMode) return BingoData.RandomBingoDen(slug);
            else return orig.Invoke(rainWorld, slug);
        }

        private static string ExpeditionCoreFile_ToString(On.Expedition.ExpeditionCoreFile.orig_ToString orig, ExpeditionCoreFile self)
        {
            Plugin.logger.LogMessage("Current challenge list tostring");
            foreach (var gruh in ExpeditionData.challengeList)
            {
                Plugin.logger.LogMessage(ExpeditionData.challengeList.IndexOf(gruh) + " - " + gruh);
            }
            return orig.Invoke(self);
        }

        private static void RainWorldGame_GoToDeathScreen(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld("ModManager", "Expedition")
                ))
            {
                c.EmitDelegate<Func<bool, bool>>((orig) =>
                {
                    if (BingoData.BingoMode) orig = false;
                    return orig;
                });
            }
            else Plugin.logger.LogMessage("RainWorldGame_GoToDeathScreen IL failed!!!! " + il);
        }

        private static void ExpeditionCoreFile_ToStringIL(ILContext il)
        {
            ILCursor c = new(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("<expC>")
                ))
            {
                c.Index++;
                c.EmitDelegate<Func<List<string>, List<string>>>((orig) =>
                {
                    string text = "";
                    for (int i = 0; i < BingoData.BingoSaves.Count; i++)
                    {
                        BingoData.BingoSaveData saveData = BingoData.BingoSaves.ElementAt(i).Value;
                        text += BingoData.BingoSaves.ElementAt(i).Key + "#" + saveData.size.ToString();
                        if (SteamFinal.IsSaveMultiplayer(saveData))
                        {
                            text +=
                            "#" +
                            saveData.team + 
                            "#" +
                            saveData.hostID + 
                            "#" + 
                            (saveData.isHost ? "1" : "0") +
                            "#" +
                            saveData.connectedPlayers +
                            "#" +
                            (saveData.lockout ? "1" : "0");
                        }
                        if (i < BingoData.BingoSaves.Count - 1)
                        {
                            text += "<>";
                        }
                    }
                    orig.Add("BINGOS:" + text);

                    return orig;
                });
            }
            else Plugin.logger.LogError(nameof(ExpeditionCoreFile_ToStringIL) + " Threw :(( " + il);
        }

        private static void SleepAndDeathScreen_ctor(On.Menu.SleepAndDeathScreen.orig_ctor orig, SleepAndDeathScreen self, ProcessManager manager, ProcessManager.ProcessID ID)
        {
            orig.Invoke(self, manager, ID);

            if (!ExpeditionGame.activeUnlocks.Contains("unl-passage") && BingoData.BingoMode)
            {
                ExpLog.Log("Add Expedition Passage but bingo");
                self.AddExpeditionPassageButton();
            }
        }

        private static void UnlockDialog_Update(On.Menu.UnlockDialog.orig_Update orig, UnlockDialog self)
        {
            orig.Invoke(self);

            if (bingoPage.TryGetValue(self.owner.menu as ExpeditionMenu, out var pag) && pag.unlocksButton.greyedOut)
            {
                self.pageTitle.x = pag.pos.x + 685f;
                self.pageTitle.y = pag.pos.y + 680f;
            }
        }

        private static void UnlockDialog_Singal(On.Menu.UnlockDialog.orig_Singal orig, UnlockDialog self, MenuObject sender, string message)
        {
            orig.Invoke(self, sender, message);

            if (message == "CLOSE")
            {
                if (bingoPage.TryGetValue(self.owner.menu as ExpeditionMenu, out var pag))
                {
                    pag.unlocksButton.greyedOut = false;
                    pag.unlocksButton.Reset();
                }

                if (!BingoData.MultiplayerGame || SteamTest.LobbyMembers.Count == 0 || SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby) != SteamTest.selfIdentity.GetSteamID()) return;
                if (BingoData.globalSettings.perks == LobbySettings.AllowUnlocks.Inherited)
                {
                    SteamMatchmaking.SetLobbyData(SteamTest.CurrentLobby, "perkList", Expedition.Expedition.coreFile.ActiveUnlocksString(ExpeditionGame.activeUnlocks.Where(x => x.StartsWith("unl-")).ToList()));
                }
                if (BingoData.globalSettings.burdens == LobbySettings.AllowUnlocks.Inherited)
                {
                    SteamMatchmaking.SetLobbyData(SteamTest.CurrentLobby, "burdenList", Expedition.Expedition.coreFile.ActiveUnlocksString(ExpeditionGame.activeUnlocks.Where(x => x.StartsWith("bur-")).ToList()));
                }
            }
        }

        public static void ChallengeSelectPage_SetUpSelectables(On.Menu.ChallengeSelectPage.orig_SetUpSelectables orig, ChallengeSelectPage self)
        {
            if (self.menu.currentPage == 4) return;
            orig.Invoke(self);
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
                    if (orig == BingoEnums.BingoLoseScreen)
                    {
                        self.currentMainLoop = new BingoLoseScreen(self);
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
                    if (BingoData.BingoMode)
                    {
                        orig = BingoEnums.BingoWinScreen;
                    }

                    return orig;
                });
            }
            else Plugin.logger.LogError(nameof(WinState_CycleCompleted) + " Threw 1 :(( " + il);

            if (b.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("Cycle complete, saving run data"),
                x => x.MatchCallOrCallvirt("Expedition.ExpLog", "Log")
                ))
            {
                b.EmitDelegate(() =>
                {
                    if (BingoData.BingoMode) 
                    {
                        ExpeditionGame.expeditionComplete = false;//GlobalBoard.CheckWin();
                    }
                });
            }
            else Plugin.logger.LogError(nameof(WinState_CycleCompleted) + " Threw 2 :(( " + il);
        }

        //public static void HUD_InitSinglePlayerHudIL(ILContext il)
        //{
        //    ILCursor c = new(il);
        //
        //    if (c.TryGotoNext(MoveType.After,
        //        x => x.MatchLdsfld("ModManager", "Expedition")
        //        ))
        //    {
        //        c.EmitDelegate<Func<bool, bool>>((orig) =>
        //        {
        //            orig &= !BingoData.BingoMode;
        //
        //            return orig;
        //        });
        //    }
        //    else Plugin.logger.LogError(nameof(HUD_InitSinglePlayerHudIL) + " Threw :(( " + il);
        //}

        public static void HUD_InitSinglePlayerHud(On.HUD.HUD.orig_InitSinglePlayerHud orig, HUD.HUD self, RoomCamera cam)
        {
            bool exp = ModManager.Expedition;
            if (BingoData.BingoMode)
            {
                ModManager.Expedition = false;
                self.AddPart(new BingoHUD(self));
            }
            if (self.rainWorld == null) Plugin.logger.LogMessage("rw null");
            if (self.rainWorld.options == null) Plugin.logger.LogMessage("op null");
            orig.Invoke(self, cam);
            ModManager.Expedition = exp;
        }

        public static void ExpeditionMenu_ctor(On.Menu.ExpeditionMenu.orig_ctor orig, ExpeditionMenu self, ProcessManager manager)
        {
            orig.Invoke(self, manager);
            if (ExpeditionData.challengeList != null && ExpeditionData.challengeList.Count > 0) BingoData.HookAll(ExpeditionData.challengeList, false);
            self.pages.Add(new Page(self, null, "BINGO", 4));
            BingoData.globalMenu = self;
            BingoData.MultiplayerGame = false;
            SteamTest.team = 0;
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
           // Plugin.logger.LogMessage("Initialized bingo menu page " + self.pages[4] + " " + page);
        }

        public static void ExpeditionMenu_Singal(On.Menu.ExpeditionMenu.orig_Singal orig, ExpeditionMenu self, MenuObject sender, string message)
        {
            orig.Invoke(self, sender, message);

            if (self.pagesMoving) return;
            if (message == "NEWBINGO")
            {
                SteamTest.team = 0;
                if (bingoPage.TryGetValue(self, out var page))
                {
                    self.UpdatePage(4);
                    Plugin.logger.LogMessage("regenerating here at ExpeditionMenu_Singal");
                    GlobalBoard.GenerateBoard(GlobalBoard.size);
                    if (page.grid != null)
                    {
                        page.grid.RemoveSprites();
                        page.RemoveSubObject(page.grid);
                        page.grid = null;
                    }
                    page.grid = new BingoGrid(self, page, new(self.manager.rainWorld.screenSize.x / 2f, self.manager.rainWorld.screenSize.y / 2f), 500f);
                    page.subObjects.Add(page.grid);
                    self.MovePage(new Vector2(1500f, 0f));
                }
            }
            if (message == "LOADBINGO")
            {
                BingoData.InitializeBingo();
                LoadBingoNoStart();

                if (ModManager.CoopAvailable)
                {
                    for (int i = 1; i < self.manager.rainWorld.options.JollyPlayerCount; i++)
                    {
                        self.manager.rainWorld.RequestPlayerSignIn(i, null);
                    }
                    for (int j = self.manager.rainWorld.options.JollyPlayerCount; j < 4; j++)
                    {
                        self.manager.rainWorld.DeactivatePlayer(j);
                    }
                }

                self.manager.arenaSitting = null;
                self.manager.rainWorld.progression.currentSaveState = null;
                self.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = ExpeditionData.slugcatPlayer;

                if (self.manager.rainWorld.progression.IsThereASavedGame(ExpeditionData.slugcatPlayer))
                {
                    Expedition.Expedition.coreFile.Save(false);
                    self.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.Load;
                    self.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
                    self.PlaySound(SoundID.MENU_Continue_Game);
                }
            }
            if (message == "TRYREJOIN")
            {
                if (bingoPage.TryGetValue(self, out var page))
                {
                    page.fromContinueGame = true;

                    SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
                    SteamMatchmaking.AddRequestLobbyListResultCountFilter(100);
                    SteamAPICall_t call = SteamMatchmaking.RequestLobbyList();
                    SteamTest.lobbyMatchList.Set(call, SteamTest.OnLobbyMatchListFromContinue);
                }
            }
            if (message == "CREATELOB")
            {
                if (bingoPage.TryGetValue(self, out var page))
                {
                    self.UpdatePage(4);
                    self.MovePage(new Vector2(1500f, 0f));
                    LoadBingoNoStart();
                    if (page.grid != null)
                    {
                        page.grid.RemoveSprites();
                        page.RemoveSubObject(page.grid);
                        page.grid = null;
                    }
                    page.grid = new BingoGrid(self, page, new(self.manager.rainWorld.screenSize.x / 2f, self.manager.rainWorld.screenSize.y / 2f), 500f);
                    page.subObjects.Add(page.grid);
                    page.fromContinueGame = true;

                    page.multiButton.buttonBehav.greyedOut = false;
                    page.multiButton.Clicked();
                    page.createLobby.buttonBehav.greyedOut = false;
                    page.createLobby.Clicked();
                }
            }
        }

        public static void LoadBingoNoStart()
        {
            if (BingoData.BingoSaves == null || !BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer)) return;
            int size = BingoData.BingoSaves[ExpeditionData.slugcatPlayer].size;
            GlobalBoard.challengeGrid = new Challenge[size, size];
            int chIndex = 0;
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    GlobalBoard.challengeGrid[i, j] = ExpeditionData.challengeList[chIndex];
                    //ExpeditionData.challengeList.Add(GlobalBoard.challengeGrid[i, j]);
                    chIndex++;
                }
            }
        }

        public static void ExpeditionMenu_UpdatePage(On.Menu.ExpeditionMenu.orig_UpdatePage orig, ExpeditionMenu self, int pageIndex)
        {
            if (pageIndex == 4)
            {
                if (bingoPage.TryGetValue(self, out var page))
                {
                    self.selectedObject = page.randomize;
                    page.fromContinueGame = false;
                }
                else self.selectedObject = self.characterSelect.slugcatButtons[0];
            }

            orig.Invoke(self, pageIndex);
        }

        // Creating butone
        public static void CharacterSelectPage_UpdateStats(On.Menu.CharacterSelectPage.orig_UpdateStats orig, CharacterSelectPage self)
        {
            if (BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer))
            {
                SlugcatSelectMenu.SaveGameData saveGameData = SlugcatSelectMenu.MineForSaveData(self.menu.manager, ExpeditionData.slugcatPlayer);

                bool isMultiplayer = SteamFinal.IsSaveMultiplayer(BingoData.BingoSaves[ExpeditionData.slugcatPlayer]);
                //bool isHost = BingoData.BingoSaves[ExpeditionData.slugcatPlayer].isHost; 
                bool isSpectator = BingoData.BingoSaves[ExpeditionData.slugcatPlayer].team == 8; //TODO
                if (saveGameData == null)
                {
                    BingoData.BingoSaves.Remove(ExpeditionData.slugcatPlayer);
                    goto invok;
                }
                self.slugcatDescription.text = "";
                if (!newBingoButton.TryGetValue(self, out _))
                {
                    newBingoButton.Add(self, new HoldButton(self.menu, self, isMultiplayer ? "CONTINUE\nMULTIPLAYER" : "CONTINUE\nBINGO", "LOADBINGO", new Vector2(680f, 210f), 30f));
                }
                newBingoButton.TryGetValue(self, out var bb);
                self.subObjects.Add(bb);
                self.abandonButton.Show();
                return;
            }
            invok:
            orig.Invoke(self);

            self.confirmExpedition.pos.x += 90;

            if (!newBingoButton.TryGetValue(self, out _))
            {
                newBingoButton.Add(self, new HoldButton(self.menu, self, "PLAY\nBINGO", "NEWBINGO", new Vector2(590f, 180f), 30f));
            }
            newBingoButton.TryGetValue(self, out var button);
            self.subObjects.Add(button);
        }

        private static void CharacterSelectPage_UpdateChallengePreview(On.Menu.CharacterSelectPage.orig_UpdateChallengePreview orig, CharacterSelectPage self)
        {
            orig.Invoke(self);

            if (BingoData.BingoSaves.ContainsKey(ExpeditionData.slugcatPlayer))
            {
                if (self.strikethroughs != null)
                {
                    for (int i = 0; i < self.strikethroughs.Count; i++)
                    {
                        if (self.strikethroughs[i] != null)
                        {
                            self.strikethroughs[i].RemoveFromContainer();
                        }
                    }
                }
                self.strikethroughs = new List<FSprite>();
                if (self.challengePreviews != null)
                {
                    for (int j = 0; j < self.challengePreviews.Count; j++)
                    {
                        self.challengePreviews[j].RemoveSprites();
                        self.challengePreviews[j].RemoveSubObject(self.challengePreviews[j]);
                    }
                    self.challengePreviews = new List<MenuLabel>();
                }
            }
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
