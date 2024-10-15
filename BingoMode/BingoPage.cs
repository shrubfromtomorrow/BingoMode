using BingoMode.Challenges;
using Expedition;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using System.Collections.Generic;
using UnityEngine;
using RWCustom;
using Steamworks;

namespace BingoMode
{
    using BingoSteamworks;
    using System.Linq;
    using static BingoMode.BingoSteamworks.LobbySettings;

    public class BingoPage : PositionedMenuObject
    {
        public ExpeditionMenu expMenu;
        public BingoBoard board;
        public BingoGrid grid;
        public int size;
        public FSprite pageTitle;
        public SymbolButton rightPage;
        public HoldButton startGame;
        public SymbolButton randomize;
        public OpHoldButton unlocksButton;
        public UIelementWrapper unlockWrapper;
        public MenuTabWrapper menuTabWrapper;
        public SymbolButton plusButton;
        public SymbolButton minusButton;
        public OpTextBox shelterSetting;
        public ConfigurableBase shelterSettingConf;
        public UIelementWrapper shelterSettingWrapper;
        public MenuLabel shelterLabel;

        // Multiplayer
        public SimpleButton multiButton;
        public RoundedRect multiMenuBg;
        public FSprite divider;
        public SymbolButton createLobby;
        public SymbolButton friendsNoFriends;
        public SymbolButton refreshSearch;
        public OpComboBox distanceFilter;
        public ConfigurableBase distanceFilterConf;
        public UIelementWrapper distanceFilterWrapper;
        public OpTextBox nameFilter;
        public ConfigurableBase nameFilterConf;
        public UIelementWrapper nameFilterWrapper;
        //public MenuTab tab;
        public List<LobbyInfo> foundLobbies;
        public FSprite[] lobbyDividers;
        public VerticalSlider slider;
        public float sliderF;
        readonly int[] maxItems = [21, 16];
        public bool inLobby;
        public MenuLabel lobbyName;
        public SymbolButton lobbySettingsInfo;
        public List<PlayerInfo> lobbyPlayers;
        public float lobbySlideIn;
        public float lastLobbySlideIn;
        public float slideStep;
        public bool fromContinueGame;
        public bool spectatorMode;

        public static readonly float desaturara = 0.25f;
        public static readonly Color[] TEAM_COLOR =
        {
            Custom.Desaturate(Color.red, desaturara),
            Custom.Desaturate(Color.blue, desaturara),
            Custom.Desaturate(Color.green, desaturara),
            Custom.Desaturate(Color.yellow, desaturara),
            Custom.Desaturate(Color.magenta, desaturara), // Pink
            Custom.Desaturate(Color.cyan, desaturara),
            Custom.Desaturate(new(1f, 0.45f, 0f), desaturara), // orange
            Custom.Desaturate(new(0.5f, 0f, 0.5f), desaturara), // purple
            Custom.Desaturate(Color.grey, desaturara), // Spectator
        };

        public static readonly string[] BANNED_MOD_IDS =
        {
            "devtools",
            "slime-cubed.devconsole",
            "fyre.BeastMaster",
            "warp",
            "maxi-mol.mousedrag"
        };

        public BingoPage(Menu.Menu menu, MenuObject owner, Vector2 pos) : base(menu, owner, pos)
        {
            expMenu = menu as ExpeditionMenu;
            board = BingoHooks.GlobalBoard;
            size = board.size;
            BingoData.BingoMode = false;
            BingoData.TeamsInBingo = [0];

            //tab = new MenuTab();
            //Container.AddChild(tab._container);
            //tab._Activate();
            //tab._Update();
            //tab._GrafUpdate(0f);

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

            //startGame = new BigSimpleButton(menu, this, "BEGIN", "STARTBINGO",
            //    new Vector2(menu.manager.rainWorld.screenSize.x * 0.75f, 40f),
            //    new Vector2(150f, 40f), FLabelAlignment.Center, true);
            float xx = menu.manager.rainWorld.screenSize.x * 0.79f;
            float yy = 85f;
            startGame = new HoldButton(menu, this, "BEGIN", "STARTBINGO",
                new Vector2(xx + 75f, yy + 160f), 40f);
            subObjects.Add(startGame);

            menuTabWrapper = new MenuTabWrapper(menu, this);
            subObjects.Add(menuTabWrapper);
            unlocksButton = new OpHoldButton(new Vector2(xx, yy), new Vector2(150f, 50f), menu.Translate("CONFIGURE<LINE>PERKS & BURDENS").Replace("<LINE>", "\r\n"), 20f);
            unlocksButton.OnPressDone += UnlocksButton_OnPressDone;
            unlocksButton.description = " ";
            unlockWrapper = new UIelementWrapper(menuTabWrapper, unlocksButton);

            minusButton = new SymbolButton(menu, this, "minus", "REMOVESIZE", new Vector2(xx - 45f, yy + 5f));
            minusButton.size = new Vector2(40f, 40f);
            minusButton.roundedRect.size = minusButton.size;
            subObjects.Add(minusButton);
            plusButton = new SymbolButton(menu, this, "plus", "ADDSIZE", new Vector2(xx + 155f, yy + 5f));
            plusButton.size = new Vector2(40f, 40f);
            plusButton.roundedRect.size = plusButton.size;
            subObjects.Add(plusButton);

            shelterSettingConf = MenuModList.ModButton.RainWorldDummy.config.Bind<string>("_ShelterSettingBingo", "_", (ConfigAcceptableBase)null);
            shelterSetting = new OpTextBox(shelterSettingConf as Configurable<string>, new Vector2(xx + 48, yy + 56), 100f);
            shelterSetting.alignment = FLabelAlignment.Center;
            shelterSetting.description = "The shelter players start in. Please type in a valid shelter's room name, or 'random'";
            shelterSetting.OnValueUpdate += ShelterSetting_OnValueUpdate;
            shelterSettingWrapper = new UIelementWrapper(menuTabWrapper, shelterSetting);
            shelterSetting.value = "random";

            shelterLabel = new MenuLabel(menu, this, "Shelter: ", new Vector2(xx + 26f, yy + 69), default, false);
            subObjects.Add(shelterLabel);

            //tab.AddItems(
            //[
            //    shelterSetting
            //]);

            // Multigameplayguy
            multiButton = new SimpleButton(menu, this, "Multiplayer", "SWITCH_MULTIPLAYER", expMenu.exitButton.pos + new Vector2(0f, -40f), new Vector2(140f, 30f));
            subObjects.Add(multiButton);
            multiMenuBg = new RoundedRect(menu, this, default, new Vector2(380f, 600f), true);
            subObjects.Add(multiMenuBg);

            divider = new FSprite("pixel");
            divider.scaleX = 380f;
            divider.scaleY = 2f;
            divider.anchorX = 0f;
            Container.AddChild(divider);

            distanceFilterConf = MenuModList.ModButton.RainWorldDummy.config.Bind<string>("_DistanceFilterBingo", "Near", (ConfigAcceptableBase)null);
            nameFilterConf = MenuModList.ModButton.RainWorldDummy.config.Bind<string>("_NameFilterBingo", "", (ConfigAcceptableBase)null);
            CreateSearchPage();

            slider = new VerticalSlider(menu, this, "", new Vector2(375f, 47f), new Vector2(30f, 500f), BingoEnums.MultiplayerSlider, true) { floatValue = 1f };
            //foreach (var line in slider.lineSprites)
            //{
            //    line.alpha = 0f;
            //}
            //slider.subtleSliderNob.outerCircle.alpha = 0f;
            subObjects.Add(slider);
            sliderF = 1f;
        }

        private void ShelterSetting_OnValueUpdate(UIconfig config, string value, string oldValue)
        {
            Plugin.logger.LogMessage(value);
            string lastDen = BingoData.BingoDen;
            if (value.Trim() == string.Empty)
            {
                BingoData.BingoDen = "random";
                return;
            }
            BingoData.BingoDen = value;
            Plugin.logger.LogMessage($"SETTING BINGO DEN FROM {lastDen} TO {BingoData.BingoDen}");
        }

        private void NameFilter_OnValueUpdate(UIconfig config, string value, string oldValue)
        {
            SteamTest.CurrentFilters.text = value;
            SteamTest.GetJoinableLobbies();
        }

        private void DistanceFilter_OnValueChanged(UIconfig config, string value, string oldValue)
        {
            int num = 1;
            switch (value)
            {
                case "Far":
                    num = 2;
                    break;
                case "Worldwide":
                    num = 3;
                    break;
            }

            SteamTest.CurrentFilters.distance = num;
            SteamTest.GetJoinableLobbies();
        }

        public void Switch(bool toInLobby, bool create) // (nintendo reference
        {
            if (inLobby == toInLobby) return;
            inLobby = toInLobby;
            if (toInLobby)
            {
                expMenu.exitButton.buttonBehav.greyedOut = true;
                rightPage.buttonBehav.greyedOut = true;
                startGame.buttonBehav.greyedOut = !create;
                shelterSetting.greyedOut = !create || fromContinueGame;
                randomize.buttonBehav.greyedOut = !create || fromContinueGame;
                plusButton.buttonBehav.greyedOut = !create || fromContinueGame;
                minusButton.buttonBehav.greyedOut = !create || fromContinueGame;
                multiButton.menuLabel.text = "Leave Lobby";
                multiButton.signalText = "LEAVE_LOBBY";
                grid.Switch(!create);
                RemoveSearchPage();
                CreateLobbyPage();
                GrafUpdate(menu.myTimeStacker);
                return;
            }

            expMenu.exitButton.buttonBehav.greyedOut = false;
            rightPage.buttonBehav.greyedOut = false;
            startGame.buttonBehav.greyedOut = false;
            randomize.buttonBehav.greyedOut = false;
            plusButton.buttonBehav.greyedOut = false;
            minusButton.buttonBehav.greyedOut = false;
            multiButton.menuLabel.text = "Multiplayer";
            multiButton.signalText = "SWITCH_MULTIPLAYER";
            grid.Switch(false);

            RemoveLobbyPage();
            CreateSearchPage();
            GrafUpdate(menu.myTimeStacker);
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);

            if (message == "GOBACK")
            {
                slideStep = -1f;
                slider.subtleSliderNob.outerCircle.alpha = 0f;
                foreach (var line in slider.lineSprites)
                {
                    line.alpha = 0f;
                }
                expMenu.UpdatePage(1);
                expMenu.MovePage(new Vector2(-1500f, 0f));
                return;
            }

            if (message == "STARTBINGO")
            {
                if (menu.manager.dialog != null) menu.manager.StopSideProcess(menu.manager.dialog);
                /*
                if (SteamTest.team == 8) // Spectator
                {
                    spectatorMode = true;

                    randomize.RemoveSprites();
                    RemoveSubObject(randomize);
                    plusButton.RemoveSprites();
                    RemoveSubObject(plusButton);
                    minusButton.RemoveSprites();
                    RemoveSubObject(minusButton);
                    shelterLabel.RemoveSprites();
                    RemoveSubObject(shelterLabel);
                    rightPage.RemoveSprites();
                    RemoveSubObject(rightPage);
                    startGame.RemoveSprites();
                    RemoveSubObject(startGame);

                    unlocksButton.Hide();
                    shelterSetting.Hide();
                    unlocksButton.Unload();
                    shelterSetting.Unload();
                    menuTabWrapper.wrappers.Remove(unlocksButton);
                    menuTabWrapper.wrappers.Remove(shelterSetting);
                    menuTabWrapper.subObjects.Remove(unlockWrapper);
                    menuTabWrapper.subObjects.Remove(shelterSettingWrapper);

                    grid.Switch(true);

                    if (slideStep == 0f) slideStep = 1f;
                    else slideStep = -slideStep;
                    float ff = slideStep == 1f ? 1f : 0f;
                    slider.subtleSliderNob.outerCircle.alpha = ff;
                    foreach (var line in slider.lineSprites)
                    {
                        line.alpha = ff;
                    }

                    menu.PlaySound(SoundID.MENU_Start_New_Game);

                    List<string> bannedRegionss = [];
                    foreach (var ch in ExpeditionData.challengeList)
                    {
                        if (ch is BingoNoRegionChallenge r) bannedRegionss.Add(r.region.Value);
                        if (ch is BingoAllRegionsExcept g) bannedRegionss.Add(g.region.Value);
                    }
                resette:
                    ExpeditionData.startingDen = ExpeditionGame.ExpeditionRandomStarts(menu.manager.rainWorld, ExpeditionData.slugcatPlayer);
                    BingoData.BingoDen = ExpeditionData.startingDen;
                    foreach (var banned in bannedRegionss)
                    {
                        if (ExpeditionData.startingDen.Substring(0, 2).ToLowerInvariant() == banned.ToLowerInvariant()) goto resette;
                    }

                    if (BingoData.MultiplayerGame)
                    {
                        SteamNetworkingIdentity hostIdentity = new SteamNetworkingIdentity();
                        hostIdentity.SetSteamID(SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby));
                        string connectedPlayers = "";


                        BingoData.BingoSaves[ExpeditionData.slugcatPlayer] = new(BingoHooks.GlobalBoard.size, SteamTest.team, hostIdentity, hostIdentity.GetSteamID() == SteamTest.selfIdentity.GetSteamID(), );
                    }
                    else BingoData.BingoSaves[ExpeditionData.slugcatPlayer] = new(BingoHooks.GlobalBoard.size);
                    if (SteamTest.LobbyMembers.Count > 0 && SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby) == SteamTest.selfIdentity.GetSteamID()) SteamTest.BroadcastStartGame();

                    return;
                }
                */

                BingoData.TeamsInBingo = [SteamTest.team];
                foreach (var playere in SteamTest.LobbyMembers)
                {
                    int team = int.Parse(SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, playere.GetSteamID(), "playerTeam"));
                    if (!BingoData.TeamsInBingo.Contains(team)) BingoData.TeamsInBingo.Add(team);
                }
                Plugin.logger.LogMessage("teams in bingo: " + BingoData.TeamsInBingo.Count);

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
                Plugin.logger.LogMessage("1");
                menu.manager.arenaSitting = null;
                menu.manager.rainWorld.progression.currentSaveState = null;
                menu.manager.rainWorld.progression.miscProgressionData.currentlySelectedSinglePlayerSlugcat = ExpeditionData.slugcatPlayer;
                menu.manager.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
                Plugin.logger.LogMessage("2");

                BingoData.InitializeBingo();
                BingoData.RedoTokens();
                Plugin.logger.LogMessage("3");

                List<string> bannedRegions = [];
                Plugin.logger.LogMessage("4");
                foreach (var ch in ExpeditionData.challengeList)
                {
                    if (ch is BingoNoRegionChallenge r) bannedRegions.Add(r.region.Value);
                    if (ch is BingoAllRegionsExcept g) bannedRegions.Add(g.region.Value);
                }
                Plugin.logger.LogMessage("5");
            reset:
                Plugin.logger.LogMessage("5r");
                if (BingoData.BingoDen == "random")
                {
                    ExpeditionData.startingDen = ExpeditionGame.ExpeditionRandomStarts(menu.manager.rainWorld, ExpeditionData.slugcatPlayer);
                    BingoData.BingoDen = ExpeditionData.startingDen;
                }
                else ExpeditionData.startingDen = BingoData.BingoDen;
                Plugin.logger.LogMessage("6");
                if (bannedRegions.Count > 0)
                {
                    Plugin.logger.LogMessage("6a " + BingoData.BingoDen + " " + ExpeditionData.startingDen);
                    foreach (var banned in bannedRegions)
                    {
                        if (banned == null || banned == "") continue;
                        Plugin.logger.LogMessage("6b " + banned);
                        if (ExpeditionData.startingDen.Substring(0, 2).ToLowerInvariant() == banned.ToLowerInvariant()) goto reset;
                    }
                }
                Plugin.logger.LogMessage("7");

                foreach (var kvp in menu.manager.rainWorld.progression.mapDiscoveryTextures)
                {
                    menu.manager.rainWorld.progression.mapDiscoveryTextures[kvp.Key] = null;
                }
                Plugin.logger.LogMessage("8");

                ExpeditionGame.PrepareExpedition();
                ExpeditionData.AddExpeditionRequirements(ExpeditionData.slugcatPlayer, false);
                ExpeditionData.earnedPassages++;
                Plugin.logger.LogMessage("9");
                bool isHost = false;
                SteamFinal.SendUpKeepCounter = SteamFinal.PlayerUpkeepTime;
                SteamFinal.HostUpkeep = SteamFinal.MaxHostUpKeepTime;
                SteamFinal.ReconnectTimer = SteamFinal.TryToReconnectTime;
                SteamFinal.UpkeepCounter = SteamFinal.MaxUpkeepCounter;
                if (BingoData.MultiplayerGame)
                {
                    Plugin.logger.LogMessage("10");
                    string connectedPlayers = "";

                    SteamNetworkingIdentity hostIdentity = new SteamNetworkingIdentity();
                    hostIdentity.SetSteamID(SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby));
                    isHost = hostIdentity.GetSteamID() == SteamTest.selfIdentity.GetSteamID();

                    Plugin.logger.LogMessage("11");
                    if (isHost && SteamTest.LobbyMembers.Count > 0)
                    {
                        Plugin.logger.LogMessage("11a");
                        SteamFinal.ConnectedPlayers.Clear();
                        SteamFinal.ReceivedPlayerUpKeep = [];
                        foreach (var player in SteamTest.LobbyMembers)
                        {
                            connectedPlayers += "bPlR" + player.GetSteamID64();
                            SteamFinal.ConnectedPlayers.Add(player);
                            SteamFinal.ReceivedPlayerUpKeep[player.GetSteamID64()] = false;
                            SteamFinal.SendUpKeepCounter = SteamFinal.PlayerUpkeepTime;
                        }
                        connectedPlayers = connectedPlayers.Substring(4);
                        Plugin.logger.LogMessage("CONNECTED PLAYERS STRING SAVING: " + connectedPlayers);
                    }
                    else if (!isHost)
                    {
                        Plugin.logger.LogMessage("11b");
                        SteamFinal.ReceivedHostUpKeep = true;
                        Plugin.logger.LogMessage("12");
                        SteamFinal.HostUpkeep = SteamFinal.MaxHostUpKeepTime;
                        Plugin.logger.LogMessage("13");
                        InnerWorkings.SendMessage("C" + SteamTest.selfIdentity.GetSteamID64(), hostIdentity);
                        Plugin.logger.LogMessage("14");
                    }

                    BingoData.BingoSaves[ExpeditionData.slugcatPlayer] = new(BingoHooks.GlobalBoard.size, SteamTest.team, hostIdentity, isHost, connectedPlayers, BingoData.globalSettings.lockout);

                    Plugin.logger.LogMessage("15");
                }
                else BingoData.BingoSaves[ExpeditionData.slugcatPlayer] = new(BingoHooks.GlobalBoard.size);
                Plugin.logger.LogMessage("16");
                Expedition.Expedition.coreFile.Save(false);
                menu.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
                menu.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game, 0.1f);
                menu.PlaySound(SoundID.MENU_Start_New_Game);
                Plugin.logger.LogMessage("17");
                if (BingoData.MultiplayerGame)
                {
                    Plugin.logger.LogMessage("18");
                    if (SteamTest.LobbyMembers.Count > 0 && isHost)
                    {
                        Plugin.logger.LogMessage("19");
                        SteamMatchmaking.SetLobbyData(SteamTest.CurrentLobby, "startGame", BingoData.BingoDen);
                        SteamMatchmaking.SetLobbyJoinable(SteamTest.CurrentLobby, false);
                    }
                }
                Plugin.logger.LogMessage("20");

                // This will be decided by the host when sending the first bingo state
                //for (int i = 0; i < BingoHooks.GlobalBoard.size; i++)
                //{
                //    for (int j = 0; j < BingoHooks.GlobalBoard.size; j++)
                //    {
                //        if ((BingoHooks.GlobalBoard.challengeGrid[i, j] as BingoChallenge).ReverseChallenge) 
                //        {
                //            Plugin.logger.LogMessage("Completing reverse challenge as team " + SteamTest.team);
                //            BingoHooks.GlobalBoard.challengeGrid[i, j].CompleteChallenge();
                //        }
                //    }
                //}
                return;
            }

            if (message == "RANDOMIZE")
            {
                Regen(false);
                return;
            }

            if (message == "ADDSIZE")
            {
                BingoHooks.GlobalBoard.size += 1;
                Regen(true);
                return;
            }

            if (message == "REMOVESIZE")
            {
                BingoHooks.GlobalBoard.size = Mathf.Max(1, BingoHooks.GlobalBoard.size - 1);
                Regen(true);
                return;
            }

            if (message == "CREATE_LOBBY")
            {
                menu.manager.ShowDialog(new CreateLobbyDialog(menu.manager, this));
                return;
            }

            if (message == "LEAVE_LOBBY")
            {
                foreach (var player in SteamTest.LobbyMembers)
                {
                    InnerWorkings.SendMessage("g" + SteamTest.selfIdentity.GetSteamID64(), player);
                }
                SteamTest.LeaveLobby();
                if (spectatorMode)
                {
                    expMenu.exitButton.buttonBehav.greyedOut = false;
                    expMenu.exitButton.Clicked();
                }
                return;
            }

            if (message == "SWITCH_MULTIPLAYER")
            {
                if (slideStep == 0f) slideStep = 1f;
                else slideStep = -slideStep;
                float ff = slideStep == 1f ? 1f : 0f;
                slider.subtleSliderNob.outerCircle.alpha = ff;
                foreach (var line in slider.lineSprites)
                {
                    line.alpha = ff;
                }
                return;
            }

            if (message == "REFRESH_SEARCH")
            {
                RemoveLobbiesSprites();

                SteamTest.GetJoinableLobbies();
                return;
            }

            if (message == "TOGGLE_FRIENDSONLY")
            {
                SteamTest.CurrentFilters.friendsOnly = !SteamTest.CurrentFilters.friendsOnly;
                friendsNoFriends.symbolSprite.SetElementByName(SteamTest.CurrentFilters.friendsOnly ? "Kill_Slugcat" : "Multiplayer_Death");
                return;
            }

            if (message.StartsWith("JOIN-"))
            {
                if (SteamTest.CurrentLobby != default) return;
                (sender as SimpleButton).buttonBehav.greyedOut = true;
                if (ulong.TryParse(message.Split('-')[1], out ulong lobid))
                {
                    Plugin.logger.LogMessage("Joining" + lobid);
                    CSteamID lobbid = new CSteamID(lobid);
                    if (SteamMatchmaking.GetLobbyData(lobbid, "banCheats") == "1")
                    {
                        List<string> totallyevilmods = [];
                        foreach (var mod in ModManager.ActiveMods)
                        {
                            if (BANNED_MOD_IDS.Contains(mod.id)) totallyevilmods.Add(mod.id);
                        }

                        if (totallyevilmods.Count > 0)
                        {
                            menu.manager.ShowDialog(new InfoDialog(menu.manager, "Please disable the following cheat mods if you wish to join this lobby:\n-" + string.Join("\n-", totallyevilmods)));
                            return;
                        }
                    }
                    var call = SteamMatchmaking.JoinLobby(lobbid);
                    SteamTest.lobbyEntered.Set(call, SteamTest.OnLobbyEntered);
                }
                else
                {
                    Plugin.logger.LogError("FAILED TO PARSE LOBBY ULONG FROM " + message);
                    (sender as SimpleButton).buttonBehav.greyedOut = false;
                }
                return;
            }

            if (message == "CHANGE_SETTINGS")
            {
                menu.manager.ShowDialog(new CreateLobbyDialog(menu.manager, this, true, true));
                return;
            }

            if (message == "INFO_SETTINGS")
            {
                menu.manager.ShowDialog(new CreateLobbyDialog(menu.manager, this, true, false));
                return;
            }

            if (message.StartsWith("KICK-"))
            {
                ulong playerId = ulong.Parse(message.Split('-')[1], System.Globalization.NumberStyles.Any);
                SteamNetworkingIdentity kickedPlayer = new SteamNetworkingIdentity();
                kickedPlayer.SetSteamID64(playerId);
                InnerWorkings.SendMessage("@", kickedPlayer);
                //if (playerId == SteamTest.selfIdentity.GetSteamID64()) ResetPlayerLobby();
                return;
            }

            if (message.StartsWith("SWTEAM-"))
            {
                ulong playerId = ulong.Parse(message.Split('-')[1], System.Globalization.NumberStyles.Any);
                if (playerId == SteamTest.selfIdentity.GetSteamID64())
                {
                    int lastTeame = int.Parse(SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, new CSteamID(playerId), "playerTeam"), System.Globalization.NumberStyles.Any);
                    int nextTeame = lastTeame + 1;
                    if (nextTeame > 8) nextTeame = 0;

                    SteamTest.team = nextTeame;
                    SteamMatchmaking.SetLobbyMemberData(SteamTest.CurrentLobby, "playerTeam", nextTeame.ToString());
                    ResetPlayerLobby();
                    foreach (var player in SteamTest.LobbyMembers)
                    {
                        InnerWorkings.SendMessage("q", player);
                    }
                    return;
                }
                SteamNetworkingIdentity kickedPlayer = new SteamNetworkingIdentity();
                kickedPlayer.SetSteamID64(playerId);
                int lastTeam = int.Parse(SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, new CSteamID(playerId), "playerTeam"), System.Globalization.NumberStyles.Any);
                int nextTeam = lastTeam + 1;
                if (nextTeam > 8) nextTeam = 0;
                InnerWorkings.SendMessage("%" + nextTeam, kickedPlayer);
                return;
            }
        }

        public void ResetPlayerLobby()
        {
            RemovePlayerInfoSprites();
            CreateLobbyPlayers();
        }

        public void Regen(bool sizeChange)
        {
            BingoHooks.GlobalBoard.GenerateBoard(BingoHooks.GlobalBoard.size, sizeChange);
            //if (grid != null)
            //{
            //    grid.RemoveSprites();
            //    RemoveSubObject(grid);
            //    grid = null;
            //}
            //grid = new BingoGrid(menu, page, new(menu.manager.rainWorld.screenSize.x / 2f, menu.manager.rainWorld.screenSize.y / 2f), 500f);
            //subObjects.Add(grid);
            menu.PlaySound(SoundID.MENU_Next_Slugcat);
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            pageTitle.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 680f;
            pageTitle.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 680f;

            float slide = Mathf.Lerp(lastLobbySlideIn, lobbySlideIn, timeStacker);
            multiMenuBg.pos = Vector2.Lerp(multiButton.lastPos, multiButton.pos, timeStacker) - new Vector2(25f, 625f) + Vector2.left * (1f - Custom.LerpExpEaseInOut(0f, 1f, slide)) * 1000f;
            lastLobbySlideIn = lobbySlideIn;
            lobbySlideIn = Mathf.Clamp01(lobbySlideIn + slideStep * 0.05f);
            slider.pos.x = multiMenuBg.pos.x + 350f;            
            foreach (var line in slider.lineSprites)
            {
                line.x = multiMenuBg.pos.x + 365f; 
            }
            divider.x = multiMenuBg.pos.x + .5f;
            divider.y = 583f;

            if (!inLobby)
            {
                createLobby.pos.x = multiMenuBg.pos.x + 338f;
                createLobby.pos.y = divider.y + 5.25f;

                friendsNoFriends.pos.x = createLobby.pos.x - 40f;
                friendsNoFriends.pos.y = createLobby.pos.y;

                refreshSearch.pos.x = friendsNoFriends.pos.x - 40f;
                refreshSearch.pos.y = createLobby.pos.y;

                distanceFilter.PosX = refreshSearch.pos.x - 105f;
                distanceFilter.PosY = createLobby.pos.y + 5f;

                nameFilter.PosX = distanceFilter.PosX - 145f;
                nameFilter.PosY = createLobby.pos.y + 5f;

                if (foundLobbies != null && foundLobbies.Count > 0) DrawDisplayedLobbies(timeStacker);
                return;
            }

            lobbyName.pos.x = divider.x + 190f;
            lobbyName.pos.y = divider.y + 25.25f;
            lobbyName.lastPos = lobbyName.pos;

            lobbySettingsInfo.pos.x = multiMenuBg.pos.x + 338f;
            lobbySettingsInfo.pos.y = divider.y + 5.25f;
            lobbySettingsInfo.lastPos = lobbySettingsInfo.pos;

            if (lobbyPlayers != null && lobbyPlayers.Count > 0) DrawPlayerInfo(timeStacker);
        }

        public override void Update()
        {
            base.Update();
            if (!BingoData.MultiplayerGame) return;
            if (SteamTest.CurrentLobby == default)
            {
                rightPage.buttonBehav.greyedOut = false;
                startGame.buttonBehav.greyedOut = false;
                expMenu.exitButton.buttonBehav.greyedOut = false;
            }
            else
            {
                rightPage.buttonBehav.greyedOut = true;
                expMenu.exitButton.buttonBehav.greyedOut = true;
                startGame.buttonBehav.greyedOut = SteamTest.selfIdentity.GetSteamID() != SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby);
            }
        }

        public override void RemoveSprites()
        {
            base.RemoveSprites();
            pageTitle.RemoveFromContainer();
            unlocksButton.Hide();
            shelterSetting.Hide();
            //tab.RemoveItems(unlocksButton, shelterSetting);
            unlocksButton.Unload();
            shelterSetting.Unload();
            menuTabWrapper.wrappers.Remove(unlocksButton);
            menuTabWrapper.wrappers.Remove(shelterSetting);
            menuTabWrapper.subObjects.Remove(unlockWrapper);
            menuTabWrapper.subObjects.Remove(shelterSettingWrapper);
            if (inLobby) RemoveLobbyPage();
            else RemoveSearchPage();
        }

        public void CreateLobbyPage()
        {
            lobbyName = new MenuLabel(expMenu, this, SteamMatchmaking.GetLobbyData(SteamTest.CurrentLobby, "name"), default, default, true);
            lobbyName.pos.x = divider.x + 190f;
            lobbyName.pos.y = divider.y + 25.25f;
            lobbyName.lastPos = lobbyName.pos;
            subObjects.Add(lobbyName);
            bool isHost = SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby) == SteamTest.selfIdentity.GetSteamID();
            lobbySettingsInfo = new SymbolButton(expMenu, this, isHost ? "settingscog" : "Menu_InfoI", isHost ? "CHANGE_SETTINGS" : "INFO_SETTINGS", default);
            lobbySettingsInfo.size = new Vector2(35f, 35f);
            lobbySettingsInfo.roundedRect.size = lobbySettingsInfo.size;
            lobbySettingsInfo.symbolSprite.scale = 0.9f;
            subObjects.Add(lobbySettingsInfo);

            CreateLobbyPlayers();
        }

        public void RemoveLobbyPage()
        {
            lobbyName.RemoveSprites();
            lobbySettingsInfo.RemoveSprites();
            RemoveSubObject(lobbyName);
            RemoveSubObject(lobbySettingsInfo);
            RemovePlayerInfoSprites();
        }

        public void CreateLobbyPlayers()
        {
            lobbyPlayers = [];
            bool isHost = SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby) == SteamTest.selfIdentity.GetSteamID();
            lobbyPlayers.Add(new PlayerInfo(this, SteamTest.selfIdentity.GetSteamID(), isHost));
            foreach (var p in SteamTest.LobbyMembers)
            {
                lobbyPlayers.Add(new PlayerInfo(this, p.GetSteamID(), isHost));
            }
            lobbyDividers = new FSprite[lobbyPlayers.Count - 1];
            for (int i = 0; i < lobbyDividers.Length; i++)
            {
                lobbyDividers[i] = new FSprite("pixel")
                {
                    scaleX = 340f,
                    scaleY = 1f,
                    anchorX = 0f
                };
                Container.AddChild(lobbyDividers[i]);
            }
            DrawPlayerInfo(menu.myTimeStacker);
        }

        public void CreateSearchPage()
        {
            createLobby = new SymbolButton(menu, this, "plus", "CREATE_LOBBY", default);
            createLobby.size = new Vector2(35f, 35f);
            createLobby.roundedRect.size = createLobby.size;
            createLobby.symbolSprite.scale = 0.9f;
            subObjects.Add(createLobby);

            friendsNoFriends = new SymbolButton(menu, this, "Multiplayer_Death", "TOGGLE_FRIENDSONLY", default);
            friendsNoFriends.size = new Vector2(35f, 35f);
            friendsNoFriends.roundedRect.size = friendsNoFriends.size;
            friendsNoFriends.symbolSprite.scale = 0.9f;
            subObjects.Add(friendsNoFriends);

            refreshSearch = new SymbolButton(menu, this, "Menu_Symbol_Repeats", "REFRESH_SEARCH", default);
            refreshSearch.size = new Vector2(35f, 35f);
            refreshSearch.roundedRect.size = refreshSearch.size;
            refreshSearch.symbolSprite.scale = 1.2f;
            subObjects.Add(refreshSearch);

            distanceFilter = new OpComboBox(distanceFilterConf as Configurable<string>, default, 100f, ["Near", "Far", "Worldwide"]);
            distanceFilter.OnValueChanged += DistanceFilter_OnValueChanged;

            nameFilter = new OpTextBox(nameFilterConf as Configurable<string>, default, 140f);
            nameFilter.allowSpace = true;
            nameFilter.OnValueUpdate += NameFilter_OnValueUpdate;

            createLobby.pos.x = multiMenuBg.pos.x + 338f;
            createLobby.pos.y = divider.y + 5.25f;

            friendsNoFriends.pos.x = createLobby.pos.x - 40f;
            friendsNoFriends.pos.y = createLobby.pos.y;

            refreshSearch.pos.x = friendsNoFriends.pos.x - 40f;
            refreshSearch.pos.y = createLobby.pos.y;

            distanceFilter.PosX = refreshSearch.pos.x - 105f;
            distanceFilter.PosY = createLobby.pos.y + 5f;
            distanceFilter.lastScreenPos = distanceFilter.ScreenPos;

            nameFilter.PosX = distanceFilter.PosX - 145f;
            nameFilter.PosY = createLobby.pos.y + 5f;
            nameFilter.lastScreenPos = nameFilter.ScreenPos;

            //tab.AddItems(
            //[
            //    distanceFilter,
            //    nameFilter
            //]);
            distanceFilterWrapper = new UIelementWrapper(menuTabWrapper, distanceFilter);
            nameFilterWrapper = new UIelementWrapper(menuTabWrapper, nameFilter);
        }

        public void RemoveSearchPage()
        {
            createLobby.RemoveSprites();
            friendsNoFriends.RemoveSprites();
            refreshSearch.RemoveSprites();
            RemoveSubObject(createLobby);
            RemoveSubObject(friendsNoFriends);
            RemoveSubObject(refreshSearch);
            distanceFilter.Hide();
            nameFilter.Hide();
            //tab.RemoveItems(distanceFilter, nameFilter);
            distanceFilter.Unload();
            nameFilter.Unload();
            menuTabWrapper.wrappers.Remove(distanceFilter);
            menuTabWrapper.wrappers.Remove(nameFilter);
            menuTabWrapper.subObjects.Remove(distanceFilterWrapper);
            menuTabWrapper.subObjects.Remove(nameFilterWrapper);
            RemoveLobbiesSprites();
        }

        public void RemoveLobbiesSprites()
        {
            if (foundLobbies != null && foundLobbies.Count > 0)
            {
                for (int i = 0; i < foundLobbies.Count; i++)
                {
                    foundLobbies[i].Remove();
                }
                foundLobbies.Clear();
            }
            if (lobbyDividers != null)
            {
                for (int i = 0; i < lobbyDividers.Length; i++)
                {
                    lobbyDividers[i].RemoveFromContainer();
                }
            }
        }

        public void RemovePlayerInfoSprites()
        {
            if (lobbyPlayers != null && lobbyPlayers.Count > 0)
            {
                for (int i = 0; i < lobbyPlayers.Count; i++)
                {
                    lobbyPlayers[i].Remove();
                }
                lobbyPlayers.Clear();
            }
            if (lobbyDividers != null)
            {
                for (int i = 0; i < lobbyDividers.Length; i++)
                {
                    lobbyDividers[i].RemoveFromContainer();
                }
            }
        }

        public void UnlocksButton_OnPressDone(UIfocusable trigger)
        {
            UnlockDialog unlockDialog = new UnlockDialog(menu.manager, (menu as ExpeditionMenu).challengeSelect);
            unlocksButton.Reset();
            unlocksButton.greyedOut = true;
            if (BingoData.MultiplayerGame)
            {
                bool isHost = SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby) == SteamTest.selfIdentity.GetSteamID();
                foreach (var perj in unlockDialog.perkButtons)
                {
                    perj.buttonBehav.greyedOut = perj.buttonBehav.greyedOut || BingoData.globalSettings.perks == AllowUnlocks.None || (BingoData.globalSettings.perks == AllowUnlocks.Inherited && !isHost);
                }
                foreach (var bur in unlockDialog.burdenButtons)
                {
                    bur.buttonBehav.greyedOut = bur.buttonBehav.greyedOut || BingoData.globalSettings.burdens == AllowUnlocks.None || (BingoData.globalSettings.burdens == AllowUnlocks.Inherited && !isHost);
                }
            }
            menu.manager.ShowDialog(unlockDialog);
        }

        public void CreateDisplayedLobbies()
        {
            for (int i = 0; i < foundLobbies.Count; i++)
            {
                Container.AddChild(foundLobbies[i].nameLabel);
                Container.AddChild(foundLobbies[i].playerLabel);
            }

            lobbyDividers = new FSprite[foundLobbies.Count - 1];
            for (int i = 0; i < lobbyDividers.Length; i++)
            {
                lobbyDividers[i] = new FSprite("pixel")
                {
                    scaleX = 340f,
                    scaleY = 1f,
                    anchorX = 0f
                };
                Container.AddChild(lobbyDividers[i]);
            }
        }

        public void DrawDisplayedLobbies(float timeStacker)
        {
            float refX = Mathf.Lerp(multiMenuBg.lastPos.x, multiMenuBg.pos.x, timeStacker) + 10f;
            float dif = 500f / maxItems[0];
            float sliderDif = dif * (foundLobbies.Count - maxItems[0] - 1);
            Vector2 pouse = new Vector2(refX, 560f);
            for (int i = 0; i < foundLobbies.Count; i++)
            {
                Vector2 origPos = pouse - new Vector2(0f, dif * i - sliderDif * (1f - (foundLobbies.Count < maxItems[0] ? 1f : sliderF)));
                foundLobbies[i].maxAlpha = Mathf.InverseLerp(36f, 46f, origPos.y) - Mathf.InverseLerp(566f, 576f, origPos.y);
                foundLobbies[i].Draw(origPos);
            }
            for (int i = 0; i < lobbyDividers.Length; i++)
            {
                Vector2 origPos = pouse - new Vector2(0f, dif * i - sliderDif * (1f - (foundLobbies.Count < maxItems[0] ? 1f : sliderF)));
                lobbyDividers[i].alpha = Mathf.InverseLerp(60f, 70f, origPos.y) - Mathf.InverseLerp(566f, 576f, origPos.y);
                lobbyDividers[i].SetPosition(origPos + new Vector2(2f, -12.5f));
            }
        }

        public void DrawPlayerInfo(float timeStacker)
        {
            float refX = Mathf.Lerp(multiMenuBg.lastPos.x, multiMenuBg.pos.x, timeStacker) + 10f;
            float dif = 500f / maxItems[0];
            float sliderDif = dif * (lobbyPlayers.Count - maxItems[0] - 1);
            Vector2 pouse = new Vector2(refX, 560f);
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                Vector2 origPos = pouse - new Vector2(0f, dif * i - sliderDif * (1f - (lobbyPlayers.Count < maxItems[0] ? 1f : sliderF)));
                lobbyPlayers[i].maxAlpha = Mathf.InverseLerp(36f, 46f, origPos.y) - Mathf.InverseLerp(566f, 576f, origPos.y);
                lobbyPlayers[i].Draw(origPos);
            }
            for (int i = 0; i < lobbyDividers.Length; i++)
            {
                Vector2 origPos = pouse - new Vector2(0f, dif * i - sliderDif * (1f - (lobbyPlayers.Count < maxItems[0] ? 1f : sliderF)));
                lobbyDividers[i].alpha = Mathf.InverseLerp(60f, 70f, origPos.y) - Mathf.InverseLerp(566f, 576f, origPos.y);
                lobbyDividers[i].SetPosition(origPos + new Vector2(2f, -12.5f));
            }
        }

        public void AddLobbies(List<CSteamID> lobbies)
        {
            //if (fromContinueGame)
            //{
            //    foreach (var lobby in lobbies)
            //    {
            //        ulong owner = SteamMatchmaking.GetLobbyOwner(lobby).m_SteamID;
            //        if (owner != SteamTest.selfIdentity.GetSteamID64() && owner == BingoData.BingoSaves[ExpeditionData.slugcatPlayer].hostID.GetSteamID64())
            //        {
            //            var call = SteamMatchmaking.JoinLobby(lobby);
            //            SteamTest.lobbyEntered.Set(call, SteamTest.OnLobbyEntered);
            //            return;
            //        }
            //    }
            //    return;
            //}
            foundLobbies = [];

            foreach (var lobby in lobbies)
            {
                Plugin.logger.LogMessage($"Examining lobby - {lobby}");
                int l = SteamMatchmaking.GetLobbyDataCount(lobby);
                for (int i = 0; i < l; i++)
                {
                    if (SteamMatchmaking.GetLobbyDataByIndex(lobby, i, out string key, 255, out string value, 8192))
                    {
                        Plugin.logger.LogMessage($"Data {i} - {key} - {value}");
                    }
                }
                try
                {
                    string name = SteamMatchmaking.GetLobbyData(lobby, "name");
                    //int maxPlayers = int.Parse(SteamMatchmaking.GetLobbyData(lobby, "maxPlayers"), System.Globalization.NumberStyles.Any);
                    int currentPlayers = SteamMatchmaking.GetNumLobbyMembers(lobby);
                    bool lockout = SteamMatchmaking.GetLobbyData(lobby, "lockout") == "1";
                    bool banCheats = SteamMatchmaking.GetLobbyData(lobby, "gameMode") == "1";
                    Plugin.logger.LogMessage($"Perks: {SteamMatchmaking.GetLobbyData(lobby, "perks").Trim()}");
                    Plugin.logger.LogMessage($"Burdens: {SteamMatchmaking.GetLobbyData(lobby, "burdens").Trim()}");
                    AllowUnlocks perks = (AllowUnlocks)(int.Parse(SteamMatchmaking.GetLobbyData(lobby, "perks").Trim(), System.Globalization.NumberStyles.Any));
                    AllowUnlocks burdens = (AllowUnlocks)(int.Parse(SteamMatchmaking.GetLobbyData(lobby, "burdens").Trim(), System.Globalization.NumberStyles.Any));
                    int maxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobby);
                    Plugin.logger.LogMessage($"Adding lobby info: {name}: {currentPlayers}/{maxPlayers}. Lockout - {lockout}, Ban Cheats - {banCheats}, Perks - {perks}, Burdens - {burdens}");
                    foundLobbies.Add(new LobbyInfo(this, lobby, name, maxPlayers, currentPlayers, lockout, banCheats, perks, burdens));
                    //Plugin.logger.LogWarning($"Challenges of lobby {lobby} are:\n{SteamMatchmaking.GetLobbyData(lobby, "challenges")}");
                }
                catch (System.Exception e)
                {
                    Plugin.logger.LogError("Failed to get lobby info from lobby " + lobby + ". Exception:\n" + e);
                }
            }

            CreateDisplayedLobbies();
        }

        public void SliderSetValue(Slider slider, float f)
        {
            if (slider.ID == BingoEnums.MultiplayerSlider)
            {
                sliderF = f;
            }
        }

        public float ValueOfSlider(Slider slider)
        {
            if (slider.ID == BingoEnums.MultiplayerSlider)
            {
                return sliderF;
            }
            return 0f;
        }

        public static string TeamName(int teamIndex)
        {
            switch (teamIndex)
            {
                case 0: return "Red";
                case 1: return "Blue";
                case 2: return "Green";
                case 3: return "Yellow";
                case 4: return "Pink";
                case 5: return "Cyan";
                case 6: return "Orange";
                case 7: return "Purple";
                case 8: return "Spectator";
            }
            return "Change";
        }

        public class PlayerInfo
        {
            public CSteamID playerID;
            public string nickname;
            public int team;
            public SimpleButton kick;
            public SimpleButton cycleTeam;
            public BingoPage page;
            public FLabel nameLabel;
            public float maxAlpha;

            public PlayerInfo(BingoPage page, CSteamID player, bool controls)
            {
                this.page = page;
                bool isHost = player == SteamTest.selfIdentity.GetSteamID();
                Plugin.logger.LogMessage("Getting info for player " + player+ ". Their player team: " + SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, player, "playerTeam"));
                Plugin.logger.LogMessage("thing: " + SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, player, "playerTeam"));
                if (SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, player, "playerTeam") == "") team = 0;
                else team = int.Parse(SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, player, "playerTeam"), System.Globalization.NumberStyles.Any);

                nickname = isHost ? SteamFriends.GetPersonaName() : SteamFriends.GetFriendPersonaName(player);
                if (nickname == null) nickname = "Cant get player's nickname";
                nameLabel = new FLabel(Custom.GetFont(), nickname)
                {
                    alignment = FLabelAlignment.Left,
                    anchorX = 0,
                    color = TEAM_COLOR[team]
                };
                page.Container.AddChild(nameLabel);

                if (controls)
                {
                    cycleTeam = new SimpleButton(page.menu, page, BingoPage.TeamName(team) + (team == 8 ? "" : " Team"), "SWTEAM-" + player.ToString(), new Vector2(-10000f, -10000f), new Vector2(90f, 16f));
                    page.subObjects.Add(cycleTeam);
                    if (!isHost)
                    {
                        kick = new SimpleButton(page.menu, page, "Kick", "KICK-" + player.ToString(), new Vector2(-10000f, -10000f), new Vector2(40f, 16f));
                        page.subObjects.Add(kick);
                    }
                }
            }

            public void Draw(Vector2 origPos)
            {
                nameLabel.SetPosition(origPos);
                float a = Mathf.Clamp01(maxAlpha);
                nameLabel.alpha = a;
                if (cycleTeam != null)
                {
                    cycleTeam.Container.alpha = a;
                    cycleTeam.pos = origPos + new Vector2(250f, -8f);
                    cycleTeam.lastPos = cycleTeam.pos;
                }
                if (kick != null)
                {
                    kick.Container.alpha = a;
                    kick.pos = origPos + new Vector2(210f, -8f);
                    kick.lastPos = kick.pos;
                }
            }

            public void Remove()
            {
                nameLabel.RemoveFromContainer();
                if (cycleTeam != null)
                {
                    cycleTeam.RemoveSprites();
                    page.RemoveSubObject(cycleTeam);
                }
                if (kick != null)
                {
                    kick.RemoveSprites();
                    page.RemoveSubObject(kick);
                }
            }
        }

        public class LobbyInfo
        {
            public CSteamID lobbyID;
            public string name;
            public int maxPlayers;
            public int currentPlayers;
            public bool lockout;
            public bool banCheats;
            public AllowUnlocks perks;
            public AllowUnlocks burdens;
            public FLabel nameLabel;
            public FLabel playerLabel;
            public float maxAlpha;
            public SimpleButton clicky;
            BingoPage page;
            public InfoPanel panel;

            public LobbyInfo(BingoPage page, CSteamID lobbyID, string name, int maxPlayers, int currentPlayers, bool lockout, bool banCheats, AllowUnlocks perks, AllowUnlocks burdens)
            {
                this.lobbyID = lobbyID;
                this.name = name;
                this.maxPlayers = maxPlayers;
                this.currentPlayers = currentPlayers;
                this.lockout = lockout;
                this.banCheats = banCheats;
                this.perks = perks;
                this.burdens = burdens;
                this.page = page;

                nameLabel = new FLabel(Custom.GetFont(), name)
                {
                    alignment = FLabelAlignment.Left,
                    anchorX = 0,
                };
                playerLabel = new FLabel(Custom.GetFont(), currentPlayers + "/" + maxPlayers);

                clicky = new SimpleButton(page.menu, page, "", "JOIN-" + lobbyID.ToString(), default, new Vector2(352f, 20f));
                page.subObjects.Add(clicky);

                panel = new InfoPanel(page, this);
            }

            public void Draw(Vector2 origPos)
            {
                nameLabel.SetPosition(origPos);
                playerLabel.SetPosition(origPos + new Vector2(335f, 0f));
                float a = Mathf.Clamp01(maxAlpha); // - 0.5f * Mathf.Abs(Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * Mathf.PI))
                nameLabel.alpha = a;
                playerLabel.alpha = a;

                foreach (var sprit in clicky.roundedRect.sprites)
                {
                    sprit.alpha = 0f;
                }
                clicky.roundedRect.fillAlpha = 0f;
                clicky.roundedRect.lasFillAplha = 0f;

                clicky.pos = origPos - new Vector2(5f, 10f);
                clicky.buttonBehav.greyedOut = a < 0.5f;

                panel.visible = clicky.IsMouseOverMe && a > 0.5f;
                panel.Draw(origPos + new Vector2(350f, 0f));
            }

            public void Remove()
            {
                nameLabel.RemoveFromContainer();
                playerLabel.RemoveFromContainer();
                clicky.RemoveSprites();
                page.RemoveSubObject(clicky);
                panel.Remove();
            }

            public class InfoPanel
            {
                public FSprite[] border = new FSprite[4];
                public FSprite background;
                public FLabel[] labels = new FLabel[5];
                public bool visible;
                readonly float width = 180f;
                readonly float height = 80f;

                public InfoPanel(BingoPage page, LobbyInfo info)
                {
                    background = new FSprite("pixel")
                    {
                        scaleX = width,
                        scaleY = height,
                        anchorX = 0f,
                        anchorY = 0f,
                        color = new Color(0.001f, 0.001f, 0.001f),
                        alpha = 0.9f
                    };
                    page.Container.AddChild(background);
                    for (int i = 0; i < 2; i++)
                    {
                        border[i] = new FSprite("pixel")
                        {
                            anchorX = 0f,
                            anchorY = 0f,
                            scaleX = width,
                            scaleY = 2f,
                            shader = page.menu.manager.rainWorld.Shaders["MenuText"]
                        };
                        page.Container.AddChild(border[i]);
                    }
                    for (int i = 2; i < 4; i++)
                    {
                        border[i] = new FSprite("pixel")
                        {
                            anchorX = 0f,
                            anchorY = 0f,
                            scaleX = 2f,
                            scaleY = height,
                            shader = page.menu.manager.rainWorld.Shaders["MenuText"]
                        };
                        page.Container.AddChild(border[i]);
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        labels[i] = new FLabel(Custom.GetFont(), "")
                        {
                            shader = page.menu.manager.rainWorld.Shaders["MenuText"],
                            anchorX = 0f,
                            anchorY = 0f,
                            alignment = FLabelAlignment.Center
                        };
                        page.Container.AddChild(labels[i]);
                    }

                    labels[0].text = "Lockout: " + (info.lockout ? "Yes" : "No");
                    labels[2].text = "Perks: " + (info.perks == AllowUnlocks.Any ? "Allowed" : info.perks == AllowUnlocks.None ? "Disabled" : "Host decides");
                    labels[3].text = "Burdens: " + (info.burdens == AllowUnlocks.Any ? "Allowed" : info.burdens == AllowUnlocks.None ? "Disabled" : "Host decides");
                    labels[4].text = "Banned cheat mods: " + (info.banCheats ? "YES" : "NO");
                }

                public void Draw(Vector2 pos)
                {
                    pos.y -= height / 2f;
                    for (int i = 0; i < border.Length; i++)
                    {
                        border[i].isVisible = visible;
                    }
                    for (int i = 0; i < labels.Length; i++)
                    {
                        labels[i].isVisible = visible;
                    }
                    background.isVisible = visible;
                    border[0].SetPosition(pos);
                    border[1].SetPosition(pos + new Vector2(0f, height));
                    border[2].SetPosition(pos);
                    border[3].SetPosition(pos + new Vector2(width, 0f));
                    background.SetPosition(pos);
                    float xDif = width / 2f;
                    float yDif = height / 5f;
                    labels[0].SetPosition(pos + new Vector2(xDif, 1.5f + yDif * 4f));
                    labels[1].SetPosition(pos + new Vector2(xDif, 1.5f + yDif * 3f));
                    labels[2].SetPosition(pos + new Vector2(xDif, 1.5f + yDif * 2f));
                    labels[3].SetPosition(pos + new Vector2(xDif, 1.5f + yDif));
                    labels[4].SetPosition(pos + new Vector2(xDif, 1.5f));
                }

                public void Remove()
                {
                    for (int i = 0; i < border.Length; i++)
                    {
                        border[i].RemoveFromContainer();
                    }
                    for (int i = 0; i < labels.Length; i++)
                    {
                        labels[i].RemoveFromContainer();
                    }
                    background.RemoveFromContainer();
                }
            }
        }
    }
}
