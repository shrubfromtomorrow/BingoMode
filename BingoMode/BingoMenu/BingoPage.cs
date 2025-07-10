using BingoMode.BingoChallenges;
using Expedition;
using Menu;
using Menu.Remix;
using Menu.Remix.MixedUI;
using RWCustom;
using Steamworks;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BingoMode.BingoMenu
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
        public SymbolButton shuffle;
        public OpHoldButton unlocksButton;
        public UIelementWrapper unlockWrapper;
        public MenuTabWrapper menuTabWrapper;
        public SymbolButton plusButton;
        public SymbolButton minusButton;
        public OpTextBox shelterSetting;
        public ConfigurableBase shelterSettingConf;
        public UIelementWrapper shelterSettingWrapper;
        public MenuLabel shelterLabel;
        public SimpleButton copyBoard;
        public SimpleButton pasteBoard;
        public SymbolButton eggButton;

        // Multiplayer
        public SimpleButton multiButton;
        public RoundedRect multiMenuBg;
        public FSprite divider;
        public SymbolButton createLobby;
        public SimpleButton friendsNoFriends;
        public SimpleButton refreshSearch;
        public OpTextBox nameFilter;
        public ConfigurableBase nameFilterConf;
        public UIelementWrapper nameFilterWrapper;
        public List<LobbyInfo> foundLobbies;
        public FSprite[] lobbyDividers;
        public VerticalSlider slider;
        public float sliderF;
        readonly int[] maxItems = [18, 16];
        public bool inLobby;
        public MenuLabel lobbyName;
        public SymbolButton lobbySettingsInfo;
        public List<PlayerInfo> lobbyPlayers;
        public float lobbySlideIn;
        public float lastLobbySlideIn;
        public float slideStep;

        public static readonly float desaturara = 0.1f;
        public static readonly Color[] TEAM_COLOR =
        {
            Custom.Saturate(new Color(0.9019608f, 0.05490196f, 0.05490196f), desaturara), // Red
            Custom.Saturate(new Color(0f, 0.5f, 1f), desaturara), // Blue
            Custom.Saturate(new Color(0.2f, 1f, 0f), desaturara), // Green
            Custom.Saturate(new Color(1f, 0.6f, 0f), desaturara), // Orange
            Custom.Saturate(new Color(1f, 0f, 1f), desaturara), // Pink
            Custom.Saturate(new Color(0f, 0.9098039f, 0.9019608f), desaturara), // Cyan
            Custom.Saturate(new Color(0.36862746f, 0.36862746f, 0.43529412f), desaturara), // Black
            Custom.Saturate(new Color(0.3f, 0f, 1f), desaturara), // Hurricane
            Custom.Saturate(Color.grey, desaturara), // Spectator
        };

        public static string TeamName(int teamIndex)
        {
            switch (teamIndex)
            {
                case 0: return "Red";
                case 1: return "Blue";
                case 2: return "Green";
                case 3: return "Orange";
                case 4: return "Pink";
                case 5: return "Cyan";
                case 6: return "Black";
                case 7: return "Hurricane";
                case 8: return "Board view";
            }
            return "Change";
        }

        public static int TeamNumber(string teamName)
        {
            switch (teamName)
            {
                case "Red": return 0;
                case "Blue": return 1;
                case "Green": return 2;
                case "Orange": return 3;
                case "Pink": return 4;
                case "Cyan": return 5;
                case "Black": return 6;
                case "Hurricane": return 7;
                case "Board view": return 8;
            }
            return 0;
        }

        public BingoPage(Menu.Menu menu, MenuObject owner, Vector2 pos) : base(menu, owner, pos)
        {
            expMenu = menu as ExpeditionMenu;
            board = BingoHooks.GlobalBoard;
            size = board.size;
            BingoData.BingoMode = false;
            BingoData.TeamsInBingo = [0];

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

            randomize = new SymbolButton(menu, this, "Sandbox_Randomize", "RANDOMIZE", new Vector2(530f, 690f));
            randomize.size = new Vector2(30f, 30f);
            randomize.roundedRect.size = randomize.size;
            subObjects.Add(randomize);

            shuffle = new SymbolButton(menu, this, "Menu_Symbol_Shuffle", "SHUFFLE", new Vector2(563f, 690f));
            shuffle.size = new Vector2(30f, 30f);
            shuffle.roundedRect.size = shuffle.size;
            subObjects.Add(shuffle);

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
            shelterSetting.description = "The shelter players start in. Please type in a valid shelter's room name (CASE SENSITIVE), or 'random'";
            shelterSetting.OnValueUpdate += ShelterSetting_OnValueUpdate;
            shelterSetting.maxLength = 100;
            shelterSettingWrapper = new UIelementWrapper(menuTabWrapper, shelterSetting);
            shelterSetting.value = "random";

            shelterLabel = new MenuLabel(menu, this, "Shelter: ", new Vector2(xx + 26f, yy + 69), default, false);
            subObjects.Add(shelterLabel);

            copyBoard = new SimpleButton(menu, this, "Copy board", "COPYTOCLIPBOARD", new Vector2(xx - 10f, yy - 25f), new Vector2(80f, 20f));
            subObjects.Add(copyBoard);
            pasteBoard = new SimpleButton(menu, this, "Paste board", "PASTEFROMCLIPBOARD", new Vector2(xx + 80f, yy - 25f), new Vector2(80f, 20f));
            subObjects.Add(pasteBoard);

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

            if (ExpeditionData.ints.Sum() >= 8)
            {
                eggButton = new SymbolButton(menu, this, "GuidanceSlugcat", "EGGBUTTON", new Vector2(663f, 25f));
                eggButton.roundedRect.size = new Vector2(40f, 40f);
                eggButton.size = eggButton.roundedRect.size;
                subObjects.Add(eggButton);
            }
        }

        private void ShelterSetting_OnValueUpdate(UIconfig config, string value, string oldValue)
        {

            string lastDen = BingoData.BingoDen;
            if (value.Trim() == string.Empty)
            {
                BingoData.BingoDen = "random";
                return;
            }
            BingoData.BingoDen = value;

        }

        private void NameFilter_OnValueUpdate(UIconfig config, string value, string oldValue)
        {
            SteamTest.CurrentFilters.text = value;
            SteamTest.GetJoinableLobbies();
        }

        public void UpdateLobbyHost(bool isHost)
        {
            shelterSetting.greyedOut = !isHost;
            randomize.buttonBehav.greyedOut = !isHost;
            shuffle.buttonBehav.greyedOut = !isHost;
            plusButton.buttonBehav.greyedOut = !isHost;
            minusButton.buttonBehav.greyedOut = !isHost;
            pasteBoard.buttonBehav.greyedOut = !isHost;
            grid.Switch(!isHost);

            if (isHost)
            {
                startGame.signalText = "STARTBINGO";
                startGame.menuLabel.text = "BEGIN";
                lobbySettingsInfo.UpdateSymbol("settingscog");
                lobbySettingsInfo.signalText = "CHANGE_SETTINGS";

                return;
            }
            startGame.signalText = "GETREADY";
            startGame.menuLabel.text = "I'M\nREADY";
            lobbySettingsInfo.UpdateSymbol("Menu_InfoI");
            lobbySettingsInfo.signalText = "INFO_SETTINGS";
        }

        public void Switch(bool toInLobby, bool create) // (nintendo reference
        {
            if (inLobby == toInLobby) return;
            inLobby = toInLobby;
            if (toInLobby)
            {
                if (BingoData.globalSettings.perks == AllowUnlocks.None)
                {
                    ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("unl-"));
                }
                if (BingoData.globalSettings.burdens == AllowUnlocks.None)
                {
                    ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("bur-"));
                }

                expMenu.exitButton.buttonBehav.greyedOut = true;
                rightPage.buttonBehav.greyedOut = true;
                if (!create)
                {
                    startGame.signalText = "GETREADY";
                    startGame.menuLabel.text = "I'M\nREADY";
                }
                shelterSetting.greyedOut = !create;
                randomize.buttonBehav.greyedOut = !create;
                shuffle.buttonBehav.greyedOut = !create;
                plusButton.buttonBehav.greyedOut = !create;
                minusButton.buttonBehav.greyedOut = !create;
                pasteBoard.buttonBehav.greyedOut = !create;
                expMenu.manualButton.buttonBehav.greyedOut = true;
                multiButton.menuLabel.text = "Leave Lobby";
                multiButton.signalText = "LEAVE_LOBBY";
                grid.Switch(!create);
                RemoveSearchPage();
                CreateLobbyPage();
                GrafUpdate(menu.myTimeStacker);
                return;
            }

            ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("unl-"));
            ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("bur-"));

            expMenu.exitButton.buttonBehav.greyedOut = false;
            rightPage.buttonBehav.greyedOut = false;
            startGame.buttonBehav.greyedOut = false;
            startGame.signalText = "STARTBINGO";
            startGame.menuLabel.text = "BEGIN";
            randomize.buttonBehav.greyedOut = false;
            shuffle.buttonBehav.greyedOut = false;
            plusButton.buttonBehav.greyedOut = false;
            minusButton.buttonBehav.greyedOut = false;
            pasteBoard.buttonBehav.greyedOut = false;
            expMenu.manualButton.buttonBehav.greyedOut = false;
            multiButton.menuLabel.text = "Multiplayer";
            multiButton.signalText = "SWITCH_MULTIPLAYER";
            grid.Switch(false);

            RemoveLobbyPage();
            CreateSearchPage();
            GrafUpdate(menu.myTimeStacker);
        }

        public static string ExpeditionRandomStartsUnlocked(RainWorld rainWorld, SlugcatStats.Name slug)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            Dictionary<string, List<string>> dictionary2 = new Dictionary<string, List<string>>();
            List<string> list2 = SlugcatStats.SlugcatStoryRegions(slug);
            if (File.Exists(AssetManager.ResolveFilePath("randomstarts.txt")))
            {
                string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("randomstarts.txt"));
                for (int i = 0; i < array.Length; i++)
                {
                    if (!array[i].StartsWith("//") && array[i].Length > 0)
                    {
                        string text = Regex.Split(array[i], "_")[0];
                        if (!(ExpeditionGame.lastRandomRegion == text))
                        {
                            if (!dictionary2.ContainsKey(text))
                            {
                                dictionary2.Add(text, new List<string>());
                            }
                            if (list2.Contains(text))
                            {
                                dictionary2[text].Add(array[i]);
                            }
                            else if (ModManager.MSC && (slug == SlugcatStats.Name.White || slug == SlugcatStats.Name.Yellow))
                            {
                                if (text == "OE")
                                {
                                    dictionary2[text].Add(array[i]);
                                }
                                if (text == "LC")
                                {
                                    dictionary2[text].Add(array[i]);
                                }
                                if (text == "MS" && array[i] != "MS_S07")
                                {
                                    dictionary2[text].Add(array[i]);
                                }
                            }
                            if (dictionary2[text].Contains(array[i]) && !dictionary.ContainsKey(text))
                            {
                                dictionary.Add(text, ExpeditionGame.GetRegionWeight(text));
                            }
                        }
                    }
                }
                System.Random random = new System.Random();
                int maxValue = dictionary.Values.Sum();
                int randomIndex = random.Next(0, maxValue);
                string key = dictionary.First(delegate (KeyValuePair<string, int> x)
                {
                    randomIndex -= x.Value;
                    return randomIndex < 0;
                }).Key;
                ExpeditionGame.lastRandomRegion = key;
                int num = (from list in dictionary2.Values
                           select list.Count).Sum();
                string text2 = dictionary2[key].ElementAt(UnityEngine.Random.Range(0, dictionary2[key].Count - 1));
                ExpLog.Log(string.Format("{0} | {1} valid regions for {2} with {3} possible dens", new object[]
                {
            text2,
            dictionary.Keys.Count,
            slug.value,
            num
                }));
                return text2;
            }
            return "SU_S01";
        }

        public override void Singal(MenuObject sender, string message)
        {
            base.Singal(sender, message);

            if (message == "COPYTOCLIPBOARD")
            {
                UniClipboard.SetText(BingoHooks.GlobalBoard.ToString());
                return;
            }

            if (message == "PASTEFROMCLIPBOARD")
            {
                BingoHooks.GlobalBoard.FromString(UniClipboard.GetText());
                SteamTest.UpdateOnlineBingo();
                return;
            }

            if (message == "GOBACK")
            {
                slideStep = -1f;
                slider.subtleSliderNob.outerCircle.alpha = 0f;
                foreach (var line in slider.lineSprites)
                {
                    line.alpha = 0f;
                }
                expMenu.manualButton.signalText = "MANUAL";
                expMenu.manualButton.menuLabel.text = expMenu.Translate("MANUAL");
                expMenu.UpdatePage(1);
                expMenu.MovePage(new Vector2(-1500f, 0f));
                if (Plugin.PluginInstance.BingoConfig.PlayMenuSong.Value && expMenu.manager.musicPlayer != null) expMenu.manager.musicPlayer.FadeOutAllSongs(50f);
                return;
            }

            if (message == "STARTBINGO")
            {
                if (menu.manager.dialog != null) menu.manager.StopSideProcess(menu.manager.dialog);

                if (SteamTest.team == 8)
                {
                    BingoData.TeamsInBingo = [];
                    SpectatorHooks.Hook();
                }
                else BingoData.TeamsInBingo = [SteamTest.team];

                if (lobbyPlayers != null)
                {
                    foreach (var playere in lobbyPlayers)
                    {

                        int team = int.Parse(SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, playere.identity.GetSteamID(), "playerTeam"));
                        if (!BingoData.TeamsInBingo.Contains(team) && team != 8) BingoData.TeamsInBingo.Add(team);
                    }
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

                BingoData.InitializeBingo();
                BingoData.RedoTokens();

                List<string> bannedRegions = [];
                foreach (var ch in ExpeditionData.challengeList)
                {
                    if (ch is BingoNoRegionChallenge r) bannedRegions.Add(r.region.Value);
                    if (ch is BingoAllRegionsExcept g) bannedRegions.Add(g.region.Value);
                    if (ch is BingoEnterRegionChallenge b) bannedRegions.Add(b.region.Value);
                    if (ch is BingoEnterRegionFromChallenge a) bannedRegions.Add(a.to.Value);
                }
                if (BingoData.BingoDen.ToLowerInvariant() == "random")
                {
                    int tries = 0;
                reset:
                    ExpeditionData.startingDen = ExpeditionRandomStartsUnlocked(menu.manager.rainWorld, ExpeditionData.slugcatPlayer);
                    BingoData.BingoDen = ExpeditionData.startingDen;

                    if (bannedRegions.Count > 0)
                    {
                        foreach (var banned in bannedRegions)
                        {
                            if (bannedRegions.Count == ChallengeUtils.GetSortedCorrectListForChallenge("regionsreal").Length)
                            {
                                BingoData.BingoDen = "SU_S01";
                                ExpeditionData.startingDen = "SU_S01";
                            }
                            else if (ExpeditionData.startingDen.Substring(0, 2).ToLowerInvariant() == banned.ToLowerInvariant())
                            {
                                tries++;
                                goto reset;
                            }
                            if (banned == null || banned == "") continue;
                        }
                    }
                }
                else ExpeditionData.startingDen = BingoData.BingoDen;

                if (SteamTest.team == 8)
                {
                    ExpeditionData.startingDen = "SU_S01";
                }

                foreach (var kvp in menu.manager.rainWorld.progression.mapDiscoveryTextures)
                {
                    menu.manager.rainWorld.progression.mapDiscoveryTextures[kvp.Key] = null;
                }

                ExpeditionGame.PrepareExpedition();
                ExpeditionData.AddExpeditionRequirements(ExpeditionData.slugcatPlayer, false);
                ExpeditionData.earnedPassages = 1;
                bool isHost = false;
                SteamFinal.SendUpKeepCounter = SteamFinal.PlayerUpkeepTime;
                SteamFinal.HostUpkeep = SteamFinal.MaxHostUpKeepTime;
                SteamFinal.ReconnectTimer = SteamFinal.TryToReconnectTime;
                SteamFinal.UpkeepCounter = SteamFinal.MaxUpkeepCounter;
                if (BingoData.MultiplayerGame)
                {
                    string connectedPlayers = "";

                    SteamNetworkingIdentity hostIdentity = new SteamNetworkingIdentity();
                    hostIdentity.SetSteamID(SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby));
                    isHost = hostIdentity.GetSteamID() == SteamTest.selfIdentity.GetSteamID();

                    if (isHost)
                    {
                        SteamFinal.ConnectedPlayers.Clear();
                        SteamFinal.ReceivedPlayerUpKeep = [];
                        if (lobbyPlayers != null)
                        {
                            foreach (var player in lobbyPlayers)
                            {
                                if (player.identity.GetSteamID64() == SteamTest.selfIdentity.GetSteamID64()) continue;
                                connectedPlayers += "bPlR" + player.identity.GetSteamID64();
                                SteamFinal.ConnectedPlayers.Add(player.identity);
                                SteamFinal.ReceivedPlayerUpKeep[player.identity.GetSteamID64()] = false;
                                SteamFinal.SendUpKeepCounter = SteamFinal.PlayerUpkeepTime;
                            }
                        }
                        if (connectedPlayers.StartsWith("bPlR")) connectedPlayers = connectedPlayers.Substring(4);

                    }
                    else if (!isHost)
                    {
                        SteamFinal.ReceivedHostUpKeep = true;
                        SteamFinal.HostUpkeep = SteamFinal.MaxHostUpKeepTime;
                        InnerWorkings.SendMessage("C" + SteamTest.selfIdentity.GetSteamID64(), hostIdentity);
                    }

                    BingoData.BingoSaves[ExpeditionData.slugcatPlayer] = new(BingoHooks.GlobalBoard.size, SteamTest.team, hostIdentity, isHost, connectedPlayers, BingoData.globalSettings.gamemode, false, false, false, BingoData.TeamsListToString(BingoData.TeamsInBingo), false);
                    BingoData.RandomStartingSeed = int.Parse(SteamMatchmaking.GetLobbyData(SteamTest.CurrentLobby, "randomSeed"), System.Globalization.NumberStyles.Any);
                }
                else
                {
                    int newTeam = TeamNumber(Plugin.PluginInstance.BingoConfig.SinglePlayerTeam.Value);

                    BingoData.BingoSaves[ExpeditionData.slugcatPlayer] = new(BingoHooks.GlobalBoard.size, false, newTeam, false, false);
                    SteamTest.team = newTeam;
                }
                Expedition.Expedition.coreFile.Save(false);
                menu.manager.menuSetup.startGameCondition = ProcessManager.MenuSetup.StoryGameInitCondition.New;
                menu.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game, 0.1f);
                menu.manager.rainWorld.progression.WipeSaveState(ExpeditionData.slugcatPlayer);
                menu.PlaySound(SoundID.MENU_Start_New_Game);
                if (BingoData.MultiplayerGame && isHost)
                {
                    SteamMatchmaking.SetLobbyData(SteamTest.CurrentLobby, "startGame", BingoData.BingoDen);
                    SteamMatchmaking.SetLobbyJoinable(SteamTest.CurrentLobby, false);
                }
                return;
            }

            if (message == "RANDOMIZE")
            {
                Regen(false);
                return;
            }

            if (message == "SHUFFLE")
            {
                Shuffle();
                return;
            }

            if (message == "ADDSIZE")
            {
                int lastSize = BingoHooks.GlobalBoard.size;
                BingoHooks.GlobalBoard.size = Mathf.Min(lastSize + 1, 9);
                if (lastSize != BingoHooks.GlobalBoard.size) Regen(true);
                return;
            }

            if (message == "REMOVESIZE")
            {
                int lastSize = BingoHooks.GlobalBoard.size;
                BingoHooks.GlobalBoard.size = Mathf.Max(1, lastSize - 1);
                if (lastSize != BingoHooks.GlobalBoard.size) Regen(true);
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
                SteamTest.GetJoinableLobbies();
                return;
            }

            if (message == "SWITCH_MULTIPLAYER")
            {
                if (slideStep == 0f) slideStep = 1f;
                else slideStep = -slideStep;
                float ff = slideStep == 1f ? 1f : 0f;
                if (slideStep == 1f) SteamTest.GetJoinableLobbies();
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
                friendsNoFriends.menuLabel.text = SteamTest.CurrentFilters.friendsOnly ? "Friends only: Yes" : "Friends only: No";
                return;
            }

            if (message.StartsWith("JOIN-"))
            {
                if (SteamTest.CurrentLobby != default) return;
                (sender as SimpleButton).buttonBehav.greyedOut = true;
                if (ulong.TryParse(message.Split('-')[1], out ulong lobid))
                {

                    CSteamID lobbid = new CSteamID(lobid);
                    string lobbyVersion = SteamMatchmaking.GetLobbyData(lobbid, "lobbyVersion");
                    if (lobbyVersion != Plugin.VERSION)
                    {
                        menu.manager.ShowDialog(new InfoDialog(menu.manager, $"Version mismatch.\nPlease make sure you're using the same Bingo mod version as the lobby.\nYour version: {Plugin.VERSION} Lobby version: {lobbyVersion}"));
                        return;
                    }
                    string hostRequiredMods = SteamMatchmaking.GetLobbyData(lobbid, "hostMods");
                    if (hostRequiredMods != "none")
                    {
                        List<string> modStrings = Regex.Split(hostRequiredMods, "<bMd>").ToList();

                        Dictionary<string, string> requiredMods = [];
                        string[] skipMods = GetCommonClientMods();
                        foreach (string m in modStrings)
                        {

                            string[] idAndName = m.Split('|');
                            if (skipMods.Contains(idAndName[0])) continue;

                            requiredMods.Add(idAndName[0], idAndName[1]);
                        }

                        List<string> tooManyMods = [];
                        foreach (var mod in ModManager.ActiveMods)
                        {
                            if (skipMods.Contains(mod.id)) continue;

                            if (requiredMods.Keys.Count == 0)
                            {
                                tooManyMods.Add(mod.name);
                            }

                            if (requiredMods.Count > 0 && requiredMods.ContainsKey(mod.id)) requiredMods.Remove(mod.id);
                        }

                        if (tooManyMods.Count > 0)
                        {
                            menu.manager.ShowDialog(new InfoDialog(menu.manager, "Please disable these mods if you wish to join this lobby:\n-" + string.Join("\n- ", tooManyMods)));
                            return;
                        }
                        if (requiredMods.Count > 0)
                        {
                            menu.manager.ShowDialog(new InfoDialog(menu.manager, "Please have all of these mods enabled if you wish to join this lobby:\n-" + string.Join("\n- ", requiredMods.Values)));
                            return;
                        }
                    }

                    string slug = SteamMatchmaking.GetLobbyData(lobbid, "slugcat");
                    SlugcatSelectMenu.SaveGameData saveGameData = SlugcatSelectMenu.MineForSaveData(menu.manager, new SlugcatStats.Name(slug));
                    if (saveGameData != null)
                    {
                        menu.manager.ShowDialog(new InfoDialog(menu.manager, $"You already have a saved game session as {SlugcatStats.getSlugcatName(new(slug))}.\nAre you sure you want to join this lobby?", lobbid));
                        return;
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
                return;
            }

            if (message.StartsWith("SWTEAM-"))
            {
                string[] data = message.Split('-');
                ulong playerId = ulong.Parse(data[1], System.Globalization.NumberStyles.Any);
                int playerTeam = int.Parse(data[2], System.Globalization.NumberStyles.Any);
                if (playerId == SteamTest.selfIdentity.GetSteamID64())
                {
                    SteamTest.team = playerTeam;
                    SteamMatchmaking.SetLobbyMemberData(SteamTest.CurrentLobby, "playerTeam", playerTeam.ToString());
                    return;
                }
                SteamNetworkingIdentity playerIdentity = new SteamNetworkingIdentity();
                playerIdentity.SetSteamID64(playerId);
                InnerWorkings.SendMessage("%" + playerTeam, playerIdentity);
                return;
            }

            if (message == "EGGBUTTON")
            {
                menu.PlaySound(SoundID.MENU_Player_Join_Game);
                if (ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer) > -1)
                {
                    if (ExpeditionData.ints[ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer)] == 1)
                    {
                        ExpeditionData.ints[ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer)] = 2;
                        return;
                    }
                    ExpeditionData.ints[ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer)] = 1;
                }
                return;
            }

            if (message == "GETREADY")
            {
                SteamMatchmaking.SetLobbyMemberData(SteamTest.CurrentLobby, "ready", "1");
                startGame.signalText = "GETUNREADY";
                startGame.menuLabel.text = "I'M NOT\nREADY";
                menu.PlaySound(SoundID.MENU_Start_New_Game);
            }

            if (message == "GETUNREADY")
            {
                SteamMatchmaking.SetLobbyMemberData(SteamTest.CurrentLobby, "ready", "0");
                startGame.signalText = "GETREADY";
                startGame.menuLabel.text = "I'M\nREADY";
                menu.PlaySound(SoundID.MENU_Start_New_Game);
            }
        }

        public static string[] GetCommonClientMods()
        {
            return ["bro.mergefix",
                "pjb3005.sharpener",
                "kevadroz.no_mod_update_confirm",
                "Gamer025.RemixAutoRestart",
                "willowwisp.audiofix",
                "rwremix",
                "dressmyslugcat",
                "fastrollbutton",
                "greyscreen",
                "vigaro.guardian",
                "healthbars",
                "improved-input-confirm",
                "franklygd.killfeed",
                "lsbjorn52.ModPresets",
                "notchoc.ModTags",
                "sabreml.musicannouncements",
                "SBCameraScroll",
                "slime-cubed.inputdisplay",
                "googlyeyes",
                "rebinddevtools"];
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

        public void Shuffle()
        {
            BingoHooks.GlobalBoard.ShuffleBoard();
            menu.PlaySound(SoundID.MENU_Next_Slugcat);
        }

        public override void GrafUpdate(float timeStacker)
        {
            base.GrafUpdate(timeStacker);

            pageTitle.x = Mathf.Lerp(owner.page.lastPos.x, owner.page.pos.x, timeStacker) + 680f;
            pageTitle.y = Mathf.Lerp(owner.page.lastPos.y, owner.page.pos.y, timeStacker) + 680f;

            if (eggButton != null && expMenu.challengeSelect != null)
            {
                int num = ExpeditionGame.ExIndex(ExpeditionData.slugcatPlayer);
                if (num > -1)
                {
                    eggButton.symbolSprite.color = ((ExpeditionData.ints[num] == 2) ? new HSLColor(Mathf.Sin(expMenu.challengeSelect.colorCounter / 20f), 1f, 0.75f).rgb : new Color(0.3f, 0.3f, 0.3f));
                }
            }

            float slide = Mathf.Lerp(lastLobbySlideIn, lobbySlideIn, timeStacker);
            multiMenuBg.pos = Vector2.Lerp(multiButton.lastPos, multiButton.pos, timeStacker) - new Vector2(25f, 625f) + Vector2.left * (1f - Custom.LerpExpEaseInOut(0f, 1f, slide)) * 1000f;
            lastLobbySlideIn = lobbySlideIn;
            lobbySlideIn = Mathf.Clamp01(lobbySlideIn + slideStep * 0.05f);
            slider.pos.x = multiMenuBg.pos.x + 350f;
            foreach (var line in slider.lineSprites)
            {
                line.x = multiMenuBg.pos.x + 365f;
            }
            divider.x = multiMenuBg.pos.x;
            divider.y = 583f;

            if (!inLobby)
            {
                createLobby.pos.x = multiMenuBg.pos.x + 338f;
                createLobby.pos.y = divider.y + 5.25f;

                friendsNoFriends.pos.x = createLobby.pos.x - 117f;
                friendsNoFriends.pos.y = createLobby.pos.y + 5f;

                refreshSearch.pos.x = friendsNoFriends.pos.x - 67f;
                refreshSearch.pos.y = createLobby.pos.y + 5f;

                nameFilter.PosX = refreshSearch.pos.x - 147f;
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
            multiButton.buttonBehav.greyedOut = !SteamTest.MultiplayerEnabled;
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
                //bool isHost = SteamTest.selfIdentity.GetSteamID() == SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby);
                //if (isHost)
                //{
                //    bool allReady = true;

                //    if (lobbyPlayers != null)
                //    {
                //        foreach (var player in lobbyPlayers)
                //        {
                //            allReady &= SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, player.identity.GetSteamID(), "ready") == "1";
                //        }
                //    }

                //    startGame.buttonBehav.greyedOut = !allReady;
                //}
                //else
                //{
                //    startGame.buttonBehav.greyedOut = false;
                //}
            }
        }

        public override void RemoveSprites()
        {
            base.RemoveSprites();
            pageTitle.RemoveFromContainer();
            unlocksButton.Hide();
            shelterSetting.Hide();
            unlocksButton.Unload();
            shelterSetting.Unload();
            shelterSetting.OnValueUpdate -= ShelterSetting_OnValueUpdate;
            nameFilter.OnValueUpdate -= NameFilter_OnValueUpdate;
            menuTabWrapper.wrappers.Remove(unlocksButton);
            menuTabWrapper.wrappers.Remove(shelterSetting);
            menuTabWrapper.wrappers.Remove(nameFilter);
            menuTabWrapper.subObjects.Remove(unlockWrapper);
            menuTabWrapper.subObjects.Remove(shelterSettingWrapper);
            menuTabWrapper.subObjects.Remove(nameFilterWrapper);
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

            List<SteamNetworkingIdentity> identities = [];
            int members = SteamMatchmaking.GetNumLobbyMembers(SteamTest.CurrentLobby);
            bool allReady = true;

            BingoData.MultiplayerGame = true;
            for (int i = 0; i < members; i++)
            {
                SteamNetworkingIdentity member = new SteamNetworkingIdentity();
                member.SetSteamID(SteamMatchmaking.GetLobbyMemberByIndex(SteamTest.CurrentLobby, i));
                if (!identities.Contains(member)) identities.Add(member);

                if (member.GetSteamID() != SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby)) allReady &= SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, member.GetSteamID(), "ready") == "1";
            }

            if (isHost) startGame.buttonBehav.greyedOut = !allReady;

            foreach (var p in identities)
            {
                lobbyPlayers.Add(new PlayerInfo(this, p, isHost, SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, p.GetSteamID(), "ready") == "1"));
            }

            lobbyPlayers = lobbyPlayers.OrderBy(x => x.playerIndex).ToList();

            if (lobbyPlayers.Count == 0)
            {
                Plugin.logger.LogMessage("No people in the lobby");
            }

            //lobbyDividers = new FSprite[lobbyPlayers.Count - 1];
            // This could be negative and I think caused and OverflowException so rewritten.
            lobbyDividers = new FSprite[Math.Max(0, lobbyPlayers.Count - 1)];
            for (int i = 0; i < lobbyDividers.Length; i++)
            {
                lobbyDividers[i] = new FSprite("LinearGradient200")
                {
                    rotation = 90f,
                    anchorY = 0f,
                    scaleY = 1.5f
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

            friendsNoFriends = new SimpleButton(menu, this, "Friends only: No", "TOGGLE_FRIENDSONLY", default, new Vector2(110f, 25f));
            subObjects.Add(friendsNoFriends);

            refreshSearch = new SimpleButton(menu, this, "Refresh", "REFRESH_SEARCH", default, new Vector2(60f, 25f));
            subObjects.Add(refreshSearch);

            nameFilter = new OpTextBox(nameFilterConf as Configurable<string>, default, 140f);
            nameFilter.allowSpace = true;
            nameFilter.OnValueUpdate += NameFilter_OnValueUpdate;

            createLobby.pos.x = multiMenuBg.pos.x + 338f;
            createLobby.pos.y = divider.y + 5.25f;

            friendsNoFriends.pos.x = createLobby.pos.x - 117f;
            friendsNoFriends.pos.y = createLobby.pos.y + 5f;

            refreshSearch.pos.x = friendsNoFriends.pos.x - 67f;
            refreshSearch.pos.y = createLobby.pos.y + 5f;

            nameFilter.PosX = refreshSearch.pos.x - 147f;
            nameFilter.PosY = createLobby.pos.y + 5f;
            nameFilter.lastScreenPos = nameFilter.ScreenPos;

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
            nameFilter.Hide();
            nameFilter.Unload();
            menuTabWrapper.wrappers.Remove(nameFilter);
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
                lobbyPlayers = [];
            }
            if (lobbyDividers != null)
            {
                for (int i = 0; i < lobbyDividers.Length; i++)
                {
                    lobbyDividers[i].RemoveFromContainer();
                }
                lobbyDividers = [];
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
            string[] bannedBurdens = ["bur-doomed"];
            string[] bannedPerks = ["unl-passage", "unl-karma"];
            foreach (var bur in unlockDialog.burdenButtons)
            {
                if (bannedBurdens.Contains(bur.signalText))
                {
                    bur.buttonBehav.greyedOut = true;
                    if (ExpeditionGame.activeUnlocks.Contains(bur.signalText)) unlockDialog.ToggleBurden(bur.signalText);
                }
            }
            foreach (var per in unlockDialog.perkButtons)
            {
                if (bannedPerks.Contains(per.signalText))
                {
                    per.buttonBehav.greyedOut = true;
                    if (ExpeditionGame.activeUnlocks.Contains(per.signalText)) unlockDialog.ToggleBurden(per.signalText);
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
            RemoveLobbiesSprites();
            foundLobbies = [];

            foreach (var lobby in lobbies)
            {
                //Plugin.logger.LogMessage($"Examining lobby - {lobby}");
                //int l = SteamMatchmaking.GetLobbyDataCount(lobby);
                //for (int i = 0; i < l; i++)
                //{
                //    if (SteamMatchmaking.GetLobbyDataByIndex(lobby, i, out string key, 255, out string value, 8192))
                //    {
                //        Plugin.logger.LogMessage($"Data {i} - {key} - {value}");
                //    }
                //}
                try
                {
                    string name = SteamMatchmaking.GetLobbyData(lobby, "name");
                    //int maxPlayers = int.Parse(SteamMatchmaking.GetLobbyData(lobby, "maxPlayers"), System.Globalization.NumberStyles.Any);
                    int currentPlayers = SteamMatchmaking.GetNumLobbyMembers(lobby);
                    BingoData.BingoGameMode gamemode = (BingoData.BingoGameMode)int.Parse(SteamMatchmaking.GetLobbyData(lobby, "gamemode"), System.Globalization.NumberStyles.Any);
                    bool hostMods = SteamMatchmaking.GetLobbyData(lobby, "hostMods") != "none";
                    string lobbyVersion = SteamMatchmaking.GetLobbyData(lobby, "lobbyVersion");
                    string slugcat = SteamMatchmaking.GetLobbyData(lobby, "slugcat");


                    AllowUnlocks perks = (AllowUnlocks)(int.Parse(SteamMatchmaking.GetLobbyData(lobby, "perks").Trim(), System.Globalization.NumberStyles.Any));
                    AllowUnlocks burdens = (AllowUnlocks)(int.Parse(SteamMatchmaking.GetLobbyData(lobby, "burdens").Trim(), System.Globalization.NumberStyles.Any));
                    int maxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobby);

                    foundLobbies.Add(new LobbyInfo(this, lobby, name, maxPlayers, currentPlayers, gamemode, hostMods, lobbyVersion, slugcat, perks, burdens));
                    //
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


        public void FocusOn(PlayerInfo exception)
        {
            ResetFocus();
            int g = lobbyPlayers.IndexOf(exception) + 1;
            for (int i = g; i < Mathf.Min(lobbyPlayers.Count, g + 4); i++)
            {
                lobbyPlayers[i].disabled = true;
            }
        }

        public void ResetFocus()
        {
            for (int i = 0; i < lobbyPlayers.Count; i++)
            {
                lobbyPlayers[i].disabled = false;
            }
        }

        public class PlayerInfo
        {
            public SteamNetworkingIdentity identity;
            public string nickname;
            public int team;
            public SimpleButton kick;
            public OpComboBox selectTeam;
            public BingoPage page;
            public FLabel nameLabel;
            public float maxAlpha;
            public int playerIndex;
            public Configurable<string> conf;
            public UIelementWrapper cWrapper;
            public bool disabled;
            public FSprite readyMark;

            public PlayerInfo(BingoPage page, SteamNetworkingIdentity identity, bool controls, bool ready)
            {
                this.page = page;
                this.identity = identity;
                bool isSelf = identity.GetSteamID() == SteamTest.selfIdentity.GetSteamID();
                bool isHost = identity.GetSteamID() == SteamMatchmaking.GetLobbyOwner(SteamTest.CurrentLobby);

                int.TryParse(SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, identity.GetSteamID(), "playerTeam"), System.Globalization.NumberStyles.Any, null, out team);
                int.TryParse(SteamMatchmaking.GetLobbyMemberData(SteamTest.CurrentLobby, identity.GetSteamID(), "playerIndex"), System.Globalization.NumberStyles.Any, null, out playerIndex);

                nickname = isSelf ? SteamFriends.GetPersonaName() : SteamFriends.GetFriendPersonaName(identity.GetSteamID());
                if (nickname == null) nickname = "???";
                nickname += " (" + TeamName(team) + ")";

                nameLabel = new FLabel(Custom.GetFont(), nickname)
                {
                    alignment = FLabelAlignment.Left,
                    anchorX = 0,
                    color = TEAM_COLOR[team]
                };
                page.Container.AddChild(nameLabel);

                string markAtlas = isHost ? "TinyCrown" : ready ? "TinyCheck" : "TinyX";
                Color markColor = isHost ? Color.yellow : ready ? Color.green : Color.red;
                readyMark = new FSprite(markAtlas)
                {
                    color = markColor
                };
                page.Container.AddChild(readyMark);

                if (controls)
                {
                    conf = MenuModList.ModButton.RainWorldDummy.config.Bind<string>("_PlayerInfoSelect", TeamName(team), (ConfigAcceptableBase)null);
                    selectTeam = new OpComboBox(conf, new Vector2(-10000f, -10000f), 90f, new string[] { "Red", "Blue", "Green", "Orange", "Pink", "Cyan", "Black", "Hurricane", "Board view" });
                    selectTeam.OnValueChanged += SelectTeam_OnValueChanged;
                    selectTeam.OnListOpen += FocusThing;
                    selectTeam.OnListClose += UnfocusThing;
                    cWrapper = new UIelementWrapper(page.menuTabWrapper, selectTeam);
                    if (!isSelf)
                    {
                        kick = new SimpleButton(page.menu, page, "Kick", "KICK-" + identity.GetSteamID64().ToString(), new Vector2(-10000f, -10000f), new Vector2(40f, 16f));
                        page.subObjects.Add(kick);
                    }
                }
            }

            private void SelectTeam_OnValueChanged(UIconfig config, string value, string oldValue)
            {
                page.Singal(null, "SWTEAM-" + identity.GetSteamID64() + "-" + TeamNumber(value));
            }

            private void UnfocusThing(UIfocusable trigger)
            {
                page.ResetFocus();
            }

            private void FocusThing(UIfocusable trigger)
            {
                page.FocusOn(this);
            }

            public void Draw(Vector2 origPos)
            {
                nameLabel.SetPosition(origPos + new Vector2(8f, 0f));
                readyMark.SetPosition(origPos);
                float a = Mathf.Clamp01(maxAlpha);
                bool unclicky = maxAlpha < 0.25f || disabled;
                nameLabel.alpha = a;
                readyMark.alpha = a;
                if (selectTeam != null)
                {
                    selectTeam.myContainer.alpha = disabled ? 0f : a;
                    selectTeam.pos = origPos + new Vector2(250f, -10.5f);
                    selectTeam.lastScreenPos = selectTeam.ScreenPos;
                    selectTeam.greyedOut = unclicky;
                    selectTeam._lastGreyedOut = unclicky;
                    if (unclicky) selectTeam._mouseDown = false;
                }
                if (kick != null)
                {
                    kick.pos = origPos + new Vector2(210f, -8f);
                    kick.lastPos = kick.pos;
                    kick.buttonBehav.greyedOut = unclicky;
                    foreach (var sprite in kick.roundedRect.sprites)
                    {
                        sprite.alpha = disabled ? 0f : a;
                    }
                    kick.menuLabel.label.alpha = disabled ? 0f : a;
                }
            }

            public void Remove()
            {
                nameLabel.RemoveFromContainer();
                readyMark.RemoveFromContainer();
                if (selectTeam != null)
                {
                    selectTeam.Hide();
                    selectTeam.Unload();
                    page.menuTabWrapper.wrappers.Remove(selectTeam);
                    page.menuTabWrapper.subObjects.Remove(cWrapper);
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
            public BingoData.BingoGameMode gamemode;
            public bool hostMods;
            public string version;
            public string slugcat;
            public AllowUnlocks perks;
            public AllowUnlocks burdens;
            public FLabel nameLabel;
            public FLabel playerLabel;
            public float maxAlpha;
            public SimpleButton clicky;
            BingoPage page;
            public InfoPanel panel;

            public LobbyInfo(BingoPage page, CSteamID lobbyID, string name, int maxPlayers, int currentPlayers, BingoData.BingoGameMode gamemode, bool hostMods, string version, string slugcat, AllowUnlocks perks, AllowUnlocks burdens)
            {
                this.lobbyID = lobbyID;
                this.name = name;
                this.maxPlayers = maxPlayers;
                this.currentPlayers = currentPlayers;
                this.gamemode = gamemode;
                this.hostMods = hostMods;
                this.version = version;
                this.slugcat = slugcat;
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
                public FSprite[] border;
                public FSprite background;
                public FLabel[] labels;
                public bool visible;
                readonly float width = 180f;
                readonly float height = 100f;

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

                    border = new FSprite[4];
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

                    labels = new FLabel[6];
                    for (int i = 0; i < labels.Length; i++)
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

                    labels[0].text = "Game mode: " + info.gamemode;
                    labels[1].text = "Mod version: " + info.version;
                    labels[2].text = "Perks: " + (info.perks == AllowUnlocks.Any ? "Allowed" : info.perks == AllowUnlocks.None ? "Disabled" : "Host decides");
                    labels[3].text = "Burdens: " + (info.burdens == AllowUnlocks.Any ? "Allowed" : info.burdens == AllowUnlocks.None ? "Disabled" : "Host decides");
                    labels[4].text = "Require host's mods: " + (info.hostMods ? "Yes" : "No");
                    labels[5].text = "Slugcat: " + SlugcatStats.getSlugcatName(new(info.slugcat));
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
                    float yDif = height / labels.Length;
                    labels[0].SetPosition(pos + new Vector2(xDif + 0.01f, 1.5f + yDif * 5f));
                    labels[1].SetPosition(pos + new Vector2(xDif + 0.01f, 1.5f + yDif * 4f));
                    labels[2].SetPosition(pos + new Vector2(xDif + 0.01f, 1.5f + yDif * 3f));
                    labels[3].SetPosition(pos + new Vector2(xDif + 0.01f, 1.5f + yDif * 2));
                    labels[4].SetPosition(pos + new Vector2(xDif + 0.01f, 1.5f + yDif));
                    labels[5].SetPosition(pos + new Vector2(xDif + 0.01f, 1.5f));
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