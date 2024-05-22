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
        public MenuTab tab;
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

        public static readonly Color[] TEAM_COLOR =
        {
            Custom.Desaturate(Color.red, 0.35f),
            Custom.Desaturate(Color.blue, 0.35f),
            Custom.Desaturate(Color.green, 0.35f),
            Custom.Desaturate(Color.yellow, 0.35f),
            Custom.Desaturate(Color.grey, 0.35f),
        };

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

            //startGame = new BigSimpleButton(menu, this, "BEGIN", "STARTBINGO",
            //    new Vector2(menu.manager.rainWorld.screenSize.x * 0.75f, 40f),
            //    new Vector2(150f, 40f), FLabelAlignment.Center, true);
            float xx = menu.manager.rainWorld.screenSize.x * 0.79f;
            float yy = 85f;
            startGame = new HoldButton(menu, this, "BEGIN", "STARTBINGO",
                new Vector2(xx + 75f, yy + 135f), 40f);
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

            tab = new MenuTab();
            Container.AddChild(tab._container);
            tab._Activate();
            tab._Update();
            tab._GrafUpdate(0f);

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

        private void NameFilter_OnValueChanged(UIconfig config, string value, string oldValue)
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
                randomize.buttonBehav.greyedOut = !create;
                plusButton.buttonBehav.greyedOut = !create;
                minusButton.buttonBehav.greyedOut = !create;
                multiButton.menuLabel.text = "Leave Lobby";
                multiButton.signalText = "LEAVE_LOBBY";
                grid.Switch(!create);
                RemoveSearchPage();
                CreateLobbyPage();
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
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);

            if (message == "GOBACK")
            {
                expMenu.UpdatePage(1);
                expMenu.MovePage(new Vector2(-1500f, 0f));
                return;
            }

            if (message == "STARTBINGO")
            {
                if (SteamTest.team == 4)  // Spectator
                {


                    return;
                }

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
                SteamTest.BroadcastStartGame();
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
                SteamTest.LeaveLobby();
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
                if (ulong.TryParse(message.Split('-')[1], out ulong lobid))
                {
                    Plugin.logger.LogMessage(lobid);
                    var call = SteamMatchmaking.JoinLobby((CSteamID)lobid);
                    SteamTest.lobbyEntered.Set(call, SteamTest.OnLobbyEntered);
                }
                else Plugin.logger.LogError("FAILED TO PARSE LOBBY ULONG FROM " + message);
                return;
            }

            if (message == "CHANGE_SETTINGS")
            {
                menu.manager.ShowDialog(new CreateLobbyDialog(menu.manager, this, true, true));
            }

            if (message == "INFO_SETTINGS")
            {
                menu.manager.ShowDialog(new CreateLobbyDialog(menu.manager, this, true, false));
            }

            if (message.StartsWith("KICK-"))
            {
                ulong playerId = ulong.Parse(message.Split('-')[1], System.Globalization.NumberStyles.Any);
                SteamNetworkingIdentity kickedPlayer = new SteamNetworkingIdentity();
                kickedPlayer.SetSteamID64(playerId);
                Plugin.logger.LogMessage("test test test: " + kickedPlayer.GetSteamID());
                InnerWorkings.SendMessage("@", kickedPlayer);
                if (playerId == SteamTest.selfIdentity.GetSteamID64()) ResetPlayerLobby();
            }

            if (message.StartsWith("SWTEAM-"))
            {
                ulong playerId = ulong.Parse(message.Split('-')[1], System.Globalization.NumberStyles.Any);
                if (playerId == SteamTest.selfIdentity.GetSteamID64())
                {
                    int lastTeame = int.Parse(SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, new CSteamID(playerId), "playerTeam"), System.Globalization.NumberStyles.Any);
                    int nextTeame = lastTeame + 1;
                    if (nextTeame > 4) nextTeame = 0;

                    SteamTest.team = nextTeame;
                    SteamMatchmaking.SetLobbyMemberData(SteamTest.CurrentLobby, "playerTeam", nextTeame.ToString());
                    if (playerId == SteamTest.selfIdentity.GetSteamID64()) ResetPlayerLobby();
                    return;
                }
                SteamNetworkingIdentity kickedPlayer = new SteamNetworkingIdentity();
                kickedPlayer.SetSteamID64(playerId);
                int lastTeam = int.Parse(SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, new CSteamID(playerId), "playerTeam"), System.Globalization.NumberStyles.Any);
                int nextTeam = lastTeam + 1;
                if (nextTeam > 4) nextTeam = 0;
                InnerWorkings.SendMessage("%;" + nextTeam, kickedPlayer);
                if (playerId == SteamTest.selfIdentity.GetSteamID64()) ResetPlayerLobby();
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

            lobbySettingsInfo.pos.x = multiMenuBg.pos.x + 338f;
            lobbySettingsInfo.pos.y = divider.y + 5.25f;

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
            tab.RemoveItems(unlocksButton);
            unlocksButton.Unload();
            menuTabWrapper.wrappers.Remove(unlocksButton);
            menuTabWrapper.subObjects.Remove(unlockWrapper);
            if (inLobby) RemoveLobbyPage();
            else RemoveSearchPage();
        }

        public void CreateLobbyPage()
        {
            lobbyName = new MenuLabel(expMenu, this, SteamMatchmaking.GetLobbyData(SteamTest.CurrentLobby, "name"), default, default, true);
            subObjects.Add(lobbyName);
            Plugin.logger.LogMessage("ASHOL " + SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby) + " - " + SteamTest.selfIdentity.GetSteamID());
            bool isHost = SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby) == SteamTest.selfIdentity.GetSteamID();
            Plugin.logger.LogFatal(isHost);
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
            nameFilter.OnValueChanged += NameFilter_OnValueChanged;

            tab.AddItems(
            [
                distanceFilter,
                nameFilter
            ]);
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
            tab.RemoveItems(distanceFilter, nameFilter);
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
            foundLobbies = [];

            foreach (var lobby in lobbies)
            {
                try
                {
                    string name = SteamMatchmaking.GetLobbyData(lobby, "name");
                    int maxPlayers = int.Parse(SteamMatchmaking.GetLobbyData(lobby, "maxPlayers"), System.Globalization.NumberStyles.Any);
                    int currentPlayers = SteamMatchmaking.GetNumLobbyMembers(lobby);
                    bool lockout = SteamMatchmaking.GetLobbyData(lobby, "lockout") == "1";
                    bool gameMode = SteamMatchmaking.GetLobbyData(lobby, "gameMode") == "1";
                    AllowUnlocks perks = (AllowUnlocks)(int.Parse(SteamMatchmaking.GetLobbyData(lobby, "perks"), System.Globalization.NumberStyles.Any));
                    AllowUnlocks burdens = (AllowUnlocks)(int.Parse(SteamMatchmaking.GetLobbyData(lobby, "burdens"), System.Globalization.NumberStyles.Any));

                    Plugin.logger.LogMessage($"Adding lobby info: {name}: {currentPlayers}/{maxPlayers}. Lockout - {lockout}, Game mode - {gameMode}, Perks - {perks}, Burdens - {burdens}");
                    foundLobbies.Add(new LobbyInfo(this, lobby, name, maxPlayers, currentPlayers, lockout, gameMode, perks, burdens));
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
                team = int.Parse(SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, player, "playerTeam"), System.Globalization.NumberStyles.Any);
                bool isHost = player == SteamTest.selfIdentity.GetSteamID();
                nickname = isHost ? SteamFriends.GetPersonaName() : SteamFriends.GetPlayerNickname(player);
                nameLabel = new FLabel(Custom.GetFont(), nickname)
                {
                    alignment = FLabelAlignment.Left,
                    anchorX = 0,
                    color = TEAM_COLOR[team]
                };
                page.Container.AddChild(nameLabel);

                if (controls)
                {
                    cycleTeam = new SimpleButton(page.menu, page, "Change team", "SWTEAM-" + player.ToString(), new Vector2(-10000f, -10000f), new Vector2(90f, 16f));
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
                cycleTeam.Container.alpha = a;
                cycleTeam.pos = origPos + new Vector2(250f, -8f);
                cycleTeam.lastPos = cycleTeam.pos;
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
                cycleTeam.RemoveSprites();
                page.RemoveSubObject(cycleTeam);
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
            public bool gameMode;
            public AllowUnlocks perks;
            public AllowUnlocks burdens;
            public FLabel nameLabel;
            public FLabel playerLabel;
            public float maxAlpha;
            public SimpleButton clicky;
            BingoPage page;
            public InfoPanel panel;

            public LobbyInfo(BingoPage page, CSteamID lobbyID, string name, int maxPlayers, int currentPlayers, bool lockout, bool gameMode, AllowUnlocks perks, AllowUnlocks burdens)
            {
                this.lobbyID = lobbyID;
                this.name = name;
                this.maxPlayers = maxPlayers;
                this.currentPlayers = currentPlayers;
                this.lockout = lockout;
                this.gameMode = gameMode;
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
                    labels[1].text = "Game Mode: " + (info.gameMode ? "Teams" : "Versus");
                    labels[2].text = "Perks: " + (info.perks == AllowUnlocks.Any ? "Allowed" : info.perks == AllowUnlocks.None ? "Disabled" : "Host decides");
                    labels[3].text = "Burdens: " + (info.burdens == AllowUnlocks.Any ? "Allowed" : info.burdens == AllowUnlocks.None ? "Disabled" : "Host decides");
                    labels[4].text = "Banned cheat mods: TO-DO";//(info. ? "YES" : "NO");
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
