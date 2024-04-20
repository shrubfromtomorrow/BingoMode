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
    using System;
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
        public bool showMultiMenu;
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
        readonly int maxItems = 21;

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

            tab = new MenuTab();
            Container.AddChild(tab._container);
            tab._Activate();
            tab._Update();
            tab._GrafUpdate(0f);

            distanceFilterConf = MenuModList.ModButton.RainWorldDummy.config.Bind<string>("_DistanceFilterBingo", "Near", (ConfigAcceptableBase)null);
            distanceFilter = new OpComboBox(distanceFilterConf as Configurable<string>, default, 100f, ["Near", "Far", "Worldwide"]);
            distanceFilter.OnValueChanged += DistanceFilter_OnValueChanged;

            nameFilterConf = MenuModList.ModButton.RainWorldDummy.config.Bind<string>("_NameFilterBingo", "", (ConfigAcceptableBase)null);
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

            slider = new VerticalSlider(menu, this, "", new Vector2(380f, 47f), new Vector2(30f, 500f), BingoEnums.MultiplayerSlider, true) { floatValue = 1f };
            foreach (var line in slider.lineSprites)
            {
                line.alpha = 0f;
            }
            slider.subtleSliderNob.outerCircle.alpha = 0f;
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
                SteamTest.BroadcastStartGame();
            }

            if (message == "RANDOMIZE")
            {
                Regen(false);
            }

            if (message == "ADDSIZE")
            {
                BingoHooks.GlobalBoard.size += 1;
                Regen(true);
            }

            if (message == "REMOVESIZE")
            {
                BingoHooks.GlobalBoard.size = Mathf.Max(1, BingoHooks.GlobalBoard.size - 1);
                Regen(true);
            }

            if (message == "CREATE_LOBBY")
            {
                SteamTest.CreateLobby();
            }

            if (message == "JOIN_LOBBY")
            {
                SteamTest.GetJoinableLobbies();
            }

            if (message == "LEAVE_LOBBY")
            {
                SteamTest.LeaveLobby();
            }

            if (message == "SWITCH_MULTIPLAYER")
            {
                showMultiMenu = !showMultiMenu;
                float ff = slider.subtleSliderNob.outerCircle.alpha == 0f ? 1f : 0f;
                slider.subtleSliderNob.outerCircle.alpha = ff;
                foreach (var line in slider.lineSprites)
                {
                    line.alpha = ff;
                }
            }

            if (message == "REFRESH_SEARCH")
            {
                RemoveLobbiesSprites();

                //SteamTest.GetJoinableLobbies();
                foundLobbies = [];
                foundLobbies.Add(new LobbyInfo((CSteamID)0, "Joar's lobby", 4, 1, true, false, AllowUnlocks.Any, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)0, "Joar's lobby", 4, 1, true, false, AllowUnlocks.Any, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)0, "Joar's lobby", 4, 1, true, false, AllowUnlocks.Any, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)0, "Joar's lobby", 4, 1, true, false, AllowUnlocks.Any, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)0, "Joar's lobby", 4, 1, true, false, AllowUnlocks.Any, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)0, "Joar's lobby", 4, 1, true, false, AllowUnlocks.Any, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)0, "Joar's lobby", 4, 1, true, false, AllowUnlocks.Any, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)0, "Joar's lobby", 4, 1, true, false, AllowUnlocks.Any, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)0, "Joar's lobby", 4, 1, true, false, AllowUnlocks.Any, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)1, "Gamer's lobby", 8, 3, false, true, AllowUnlocks.Inherited, AllowUnlocks.Inherited));
                foundLobbies.Add(new LobbyInfo((CSteamID)1, "Gamer's lobby", 8, 3, false, true, AllowUnlocks.Inherited, AllowUnlocks.Inherited));
                foundLobbies.Add(new LobbyInfo((CSteamID)1, "Gamer's lobby", 8, 3, false, true, AllowUnlocks.Inherited, AllowUnlocks.Inherited));
                foundLobbies.Add(new LobbyInfo((CSteamID)1, "Gamer's lobby", 8, 3, false, true, AllowUnlocks.Inherited, AllowUnlocks.Inherited));
                foundLobbies.Add(new LobbyInfo((CSteamID)1, "Gamer's lobby", 8, 3, false, true, AllowUnlocks.Inherited, AllowUnlocks.Inherited));
                foundLobbies.Add(new LobbyInfo((CSteamID)1, "Gamer's lobby", 8, 3, false, true, AllowUnlocks.Inherited, AllowUnlocks.Inherited));
                foundLobbies.Add(new LobbyInfo((CSteamID)1, "Gamer's lobby", 8, 3, false, true, AllowUnlocks.Inherited, AllowUnlocks.Inherited));
                foundLobbies.Add(new LobbyInfo((CSteamID)1, "Gamer's lobby", 8, 3, false, true, AllowUnlocks.Inherited, AllowUnlocks.Inherited));
                foundLobbies.Add(new LobbyInfo((CSteamID)1, "Gamer's lobby", 8, 3, false, true, AllowUnlocks.Inherited, AllowUnlocks.Inherited));
                foundLobbies.Add(new LobbyInfo((CSteamID)1, "Gamer's lobby", 8, 3, false, true, AllowUnlocks.Inherited, AllowUnlocks.Inherited));
                foundLobbies.Add(new LobbyInfo((CSteamID)1, "Gamer's lobby", 8, 3, false, true, AllowUnlocks.Inherited, AllowUnlocks.Inherited));
                foundLobbies.Add(new LobbyInfo((CSteamID)2, "Nacu's lobby", 2, 1, true, true, AllowUnlocks.None, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)2, "Nacu's lobby", 2, 1, true, true, AllowUnlocks.None, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)2, "Nacu's lobby", 2, 1, true, true, AllowUnlocks.None, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)2, "Nacu's lobby", 2, 1, true, true, AllowUnlocks.None, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)2, "Nacu's lobby", 2, 1, true, true, AllowUnlocks.None, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)2, "Nacu's lobby", 2, 1, true, true, AllowUnlocks.None, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)2, "Nacu's lobby", 2, 1, true, true, AllowUnlocks.None, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)2, "Nacu's lobby", 2, 1, true, true, AllowUnlocks.None, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)2, "Nacu's lobby", 2, 1, true, true, AllowUnlocks.None, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)2, "Nacu's lobby", 2, 1, true, true, AllowUnlocks.None, AllowUnlocks.None));
                foundLobbies.Add(new LobbyInfo((CSteamID)2, "Nacu's lobby", 2, 1, true, true, AllowUnlocks.None, AllowUnlocks.None));
                CreateDisplayedLobbies();
            }

            if (message == "TOGGLE_FRIENDSONLY")
            {
                SteamTest.CurrentFilters.friendsOnly = !SteamTest.CurrentFilters.friendsOnly;
                friendsNoFriends.symbolSprite.SetElementByName(SteamTest.CurrentFilters.friendsOnly ? "Kill_Slugcat" : "Multiplayer_Death");
            }
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

            multiMenuBg.pos = Vector2.Lerp(multiButton.lastPos, multiButton.pos, timeStacker) - new Vector2(25f, 625f) + Vector2.left * (!showMultiMenu ? 1000f : 0f);

            divider.x = multiMenuBg.pos.x + .5f;
            divider.y = 583f;

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
            distanceFilter.Hide();
            nameFilter.Hide();
            unlocksButton.Hide();
            tab.RemoveItems(distanceFilter, nameFilter, unlocksButton);
            distanceFilter.Unload();
            nameFilter.Unload();
            unlocksButton.Unload();
            menuTabWrapper.wrappers.Remove(distanceFilter);
            menuTabWrapper.wrappers.Remove(nameFilter);
            menuTabWrapper.wrappers.Remove(unlocksButton);
            menuTabWrapper.subObjects.Remove(distanceFilterWrapper);
            menuTabWrapper.subObjects.Remove(nameFilterWrapper);
            menuTabWrapper.subObjects.Remove(unlockWrapper);
            RemoveLobbiesSprites();
            RemoveSubObject(slider);
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

        public void UnlocksButton_OnPressDone(UIfocusable trigger)
        {
            UnlockDialog unlockDialog = new UnlockDialog(menu.manager, (menu as ExpeditionMenu).challengeSelect);
            unlocksButton.Reset();
            unlocksButton.greyedOut = true;
            if (BingoData.MultiplayerGame)
            {
                foreach (var perj in unlockDialog.perkButtons)
                {
                    perj.buttonBehav.greyedOut = perj.buttonBehav.greyedOut || BingoData.globalSettings.perks != LobbySettings.AllowUnlocks.Any;
                }
                foreach (var bur in unlockDialog.burdenButtons)
                {
                    bur.buttonBehav.greyedOut = bur.buttonBehav.greyedOut || BingoData.globalSettings.burdens != AllowUnlocks.Any;
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
            float refX = Mathf.Lerp(multiMenuBg.lastPos.x, multiMenuBg.pos.x, timeStacker);
            float dif = 500f / maxItems;
            float sliderDif = dif * (foundLobbies.Count - maxItems - 1);
            Vector2 pouse = new Vector2(refX + 10f, 560f);
            for (int i = 0; i < foundLobbies.Count; i++)
            {
                Vector2 origPos = pouse - new Vector2(0f, dif * i - sliderDif * (1f - (foundLobbies.Count < maxItems ? 1f : sliderF)));
                foundLobbies[i].maxAlpha = Mathf.InverseLerp(36f, 46f, origPos.y) - Mathf.InverseLerp(566f, 576f, origPos.y);
                foundLobbies[i].Draw(origPos, timeStacker);
            }
            for (int i = 0; i < lobbyDividers.Length; i++)
            {
                Vector2 origPos = pouse - new Vector2(0f, dif * i - sliderDif * (1f - (foundLobbies.Count < maxItems ? 1f : sliderF)));
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
                    foundLobbies.Add(new LobbyInfo(lobby, name, maxPlayers, currentPlayers, lockout, gameMode, perks, burdens));
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

            public LobbyInfo(CSteamID lobbyID, string name, int maxPlayers, int currentPlayers, bool lockout, bool gameMode, AllowUnlocks perks, AllowUnlocks burdens)
            {
                this.lobbyID = lobbyID;
                this.name = name;
                this.maxPlayers = maxPlayers;
                this.currentPlayers = currentPlayers;
                this.lockout = lockout;
                this.gameMode = gameMode;
                this.perks = perks;
                this.burdens = burdens;

                nameLabel = new FLabel(Custom.GetFont(), name)
                {
                    alignment = FLabelAlignment.Left,
                    anchorX = 0,
                };
                playerLabel = new FLabel(Custom.GetFont(), currentPlayers + "/" + maxPlayers);
            }

            public void Draw(Vector2 origPos, float timeStacker)
            {
                Vector2 namePos = origPos;
                nameLabel.SetPosition(namePos);
                playerLabel.SetPosition(namePos + new Vector2(335f, 0f));
                float a = Mathf.Clamp01(maxAlpha); // - 0.5f * Mathf.Abs(Mathf.Sin(Mathf.Lerp(buttonBehav.lastSin, buttonBehav.sin, timeStacker) / 30f * Mathf.PI))
                nameLabel.alpha = a;
                playerLabel.alpha = a;
            }

            public void Remove()
            {
                nameLabel.RemoveFromContainer();
                playerLabel.RemoveFromContainer();
            }
        }
    }
}
