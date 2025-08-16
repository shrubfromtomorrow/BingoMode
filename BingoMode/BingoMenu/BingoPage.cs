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
        public SymbolButton filter;
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

        #region Multiplayer
        private const float MULTIPLAYER_PANEL_WIDTH = 380f;
        private const float MULTIPLAYER_PANEL_HEIGHT = 600f;

        public SimpleButton multiplayerButton;
        private MultiplayerPanel multiplayerPanel;
        public float multiplayerSlideIn;
        public float multiplayerSlideStep;
        public bool InLobby { get => multiplayerPanel.InLobby; }
        #endregion

        #region Randomizer
        private const float RANDOMIZER_PANEL_WIDTH = 190f;
        private const float RANDOMIZER_PANEL_HEIGHT = 300f;

        private SimpleButton randomizerButton;
        private RandomizerPanel randomizerPanel;
        private float randomizerSlideIn;
        private float randomizerSlideStep;
        #endregion

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

            filter = new SymbolButton(menu, this, "filter", "FILTER", new Vector2(497f, 690f));
            filter.size = new Vector2(30f, 30f);
            filter.symbolSprite.scale = 0.8f;
            filter.roundedRect.size = filter.size;
            subObjects.Add(filter);

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

            multiplayerButton = new SimpleButton(menu, this, "Multiplayer", "SWITCH_MULTIPLAYER", expMenu.exitButton.pos + new Vector2(0f, -40f), new Vector2(140f, 30f));
            subObjects.Add(multiplayerButton);
            multiplayerPanel = new(menu, this, Vector2.zero, new Vector2(MULTIPLAYER_PANEL_WIDTH, MULTIPLAYER_PANEL_HEIGHT));
            subObjects.Add(multiplayerPanel);

            randomizerButton = new(menu, this, "Profiles", "SWITCH_RANDOMIZATION", expMenu.manualButton.pos + new Vector2(0, -40f), expMenu.manualButton.size);
            subObjects.Add(randomizerButton);
            randomizerPanel = new(menu, this, Vector2.zero, new Vector2(RANDOMIZER_PANEL_WIDTH, RANDOMIZER_PANEL_HEIGHT));
            subObjects.Add(randomizerPanel);

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

        public void UpdateLobbyHost(bool isHost)
        {
            shelterSetting.greyedOut = !isHost;
            randomize.buttonBehav.greyedOut = !isHost;
            shuffle.buttonBehav.greyedOut = !isHost;
            filter.buttonBehav.greyedOut = !isHost;
            plusButton.buttonBehav.greyedOut = !isHost;
            minusButton.buttonBehav.greyedOut = !isHost;
            pasteBoard.buttonBehav.greyedOut = !isHost;
            randomizerButton.buttonBehav.greyedOut = !isHost;
            grid.Switch(!isHost);

            startGame.signalText = isHost ? "STARTBINGO" : "GETREADY";
            startGame.menuLabel.text = isHost ? "BEGIN" : "I'M\nREADY";
            multiplayerPanel.UpdateLobbyHost(isHost);
        }

        public void Switch(bool toInLobby, bool create) // (nintendo reference
        {
            if (InLobby == toInLobby)
                return;

            if (toInLobby)
                multiplayerPanel.SwitchToLobby(create);
            else
                multiplayerPanel.SwitchToSearch();
            if (toInLobby)
            {
                if (BingoData.globalSettings.perks == AllowUnlocks.None)
                    ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("unl-"));
                if (BingoData.globalSettings.burdens == AllowUnlocks.None)
                    ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("bur-"));

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
                filter.buttonBehav.greyedOut = !create;
                plusButton.buttonBehav.greyedOut = !create;
                minusButton.buttonBehav.greyedOut = !create;
                pasteBoard.buttonBehav.greyedOut = !create;
                expMenu.manualButton.buttonBehav.greyedOut = true;
                multiplayerButton.menuLabel.text = "Leave Lobby";
                multiplayerButton.signalText = "LEAVE_LOBBY";
                grid.Switch(!create);
                return;
            }

            ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("unl-"));
            ExpeditionGame.activeUnlocks.RemoveAll(x => x.StartsWith("bur-"));

            expMenu.exitButton.buttonBehav.greyedOut = false;
            rightPage.buttonBehav.greyedOut = false;
            startGame.signalText = "STARTBINGO";
            startGame.menuLabel.text = "BEGIN";
            randomize.buttonBehav.greyedOut = false;
            shuffle.buttonBehav.greyedOut = false;
            filter.buttonBehav.greyedOut = false;
            plusButton.buttonBehav.greyedOut = false;
            minusButton.buttonBehav.greyedOut = false;
            pasteBoard.buttonBehav.greyedOut = false;
            expMenu.manualButton.buttonBehav.greyedOut = false;
            multiplayerButton.menuLabel.text = "Multiplayer";
            multiplayerButton.signalText = "SWITCH_MULTIPLAYER";
            grid.Switch(false);
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
                multiplayerSlideStep = -1f;
                randomizerSlideStep = -1f;
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
                else
                    BingoData.TeamsInBingo = [SteamTest.team];

                List<PlayerData> players = SteamTest.GetPlayersData();
                foreach (PlayerData player in players)
                    if (!BingoData.TeamsInBingo.Contains(player.team) && player.team != 8)
                        BingoData.TeamsInBingo.Add(player.team);

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
                        foreach (var player in players)
                        {
                            if (player.identity.GetSteamID64() == SteamTest.selfIdentity.GetSteamID64())
                                continue;
                            connectedPlayers += "bPlR" + player.identity.GetSteamID64();
                            SteamFinal.ConnectedPlayers.Add(player.identity);
                            SteamFinal.ReceivedPlayerUpKeep[player.identity.GetSteamID64()] = false;
                            SteamFinal.SendUpKeepCounter = SteamFinal.PlayerUpkeepTime;
                        }
                        if (connectedPlayers.StartsWith("bPlR"))
                            connectedPlayers = connectedPlayers.Substring(4);
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

            if (message == "FILTER")
            {
                OpenFilterDialog();
                return;
            }

            if (message == "SWITCH")
            {
                SwitchChals(sender);
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

            if (message == "LEAVE_LOBBY")
            {
                SteamTest.LeaveLobby();
                SteamTest.GetJoinableLobbies();
                return;
            }

            if (message == "SWITCH_MULTIPLAYER")
            {
                if (multiplayerSlideStep == 0f)
                    multiplayerSlideStep = 1f;
                else
                    multiplayerSlideStep = -multiplayerSlideStep;
                float ff = multiplayerSlideStep == 1f ? 1f : 0f;
                if (multiplayerSlideStep == 1f)
                    SteamTest.GetJoinableLobbies();
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

            if (message == "SWITCH_RANDOMIZATION")
            {
                if (randomizerSlideStep == 0f) randomizerSlideStep = 1f;
                else randomizerSlideStep = -randomizerSlideStep;
                float ff = randomizerSlideStep == 1f ? 1f : 0f;
                return;
            }
        }

        public void ResetPlayerLobby() => multiplayerPanel.ResetPlayerLobby();

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

        public void OpenFilterDialog()
        {
            menu.manager.ShowDialog(new FilterDialog(menu.manager));
            menu.PlaySound(SoundID.MENU_Button_Standard_Button_Pressed);
        }

        public void SwitchChals(MenuObject sender)
        {
            BingoButton chal = sender as BingoButton;
            BingoHooks.GlobalBoard.SwitchChals(chal.challenge, chal.x, chal.y);
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

            MultiplayerSlide(timeStacker);
            RandomizerSlide(timeStacker);
        }

        private void MultiplayerSlide(float timeStacker)
        {
            const float DIST_TO_EDGE = MULTIPLAYER_PANEL_WIDTH + 50f;
            const float OFFSET_TO_BUTTON_X = -25f; // left of button to left of panel
            const float OFFSET_TO_BUTTON_Y = -25f; // bottom of button to top of panel
            multiplayerSlideIn = Mathf.Clamp01(multiplayerSlideIn + multiplayerSlideStep * 0.05f);
            multiplayerPanel.pos =
                    multiplayerButton.pos
                    + new Vector2(OFFSET_TO_BUTTON_X, OFFSET_TO_BUTTON_Y - MULTIPLAYER_PANEL_HEIGHT)
                    + Vector2.left * (1f - Custom.LerpExpEaseInOut(0f, 1f, multiplayerSlideIn)) * DIST_TO_EDGE;
            multiplayerPanel.Visible = multiplayerSlideIn >= 0.01f;
        }

        private void RandomizerSlide(float timeStacker)
        {
            const float DIST_TO_EDGE = RANDOMIZER_PANEL_WIDTH + 50f;
            const float OFFSET_TO_BUTTON_X = 25f; // right of button to right of panel
            const float OFFSET_TO_BUTTON_Y = -25f; // bottom of button to top of panel
            randomizerSlideIn = Mathf.Clamp01(randomizerSlideIn + randomizerSlideStep * 0.05f);
            randomizerPanel.pos =
                    randomizerButton.pos
                    + new Vector2(randomizerButton.size.x + OFFSET_TO_BUTTON_X - RANDOMIZER_PANEL_WIDTH, OFFSET_TO_BUTTON_Y - RANDOMIZER_PANEL_HEIGHT)
                    + Vector2.right * (1f - Custom.LerpExpEaseInOut(0f, 1f, randomizerSlideIn)) * DIST_TO_EDGE;
            randomizerPanel.Visible = randomizerSlideIn >= 0.01f;
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
            menuTabWrapper.wrappers.Remove(unlocksButton);
            menuTabWrapper.wrappers.Remove(shelterSetting);
            menuTabWrapper.subObjects.Remove(unlockWrapper);
            menuTabWrapper.subObjects.Remove(shelterSettingWrapper);
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

        public void AddLobbies(List<CSteamID> lobbies) => multiplayerPanel.AddLobbies(lobbies);

        public void SliderSetValue(Slider slider, float f)
        {
            if (slider.ID == BingoEnums.MultiplayerSlider)
                multiplayerPanel.sliderF = f;
            else if (slider.ID == BingoEnums.RandomizerSlider)
                randomizerPanel.sliderF = f;
        }

        public float ValueOfSlider(Slider slider)
        {
            if (slider.ID == BingoEnums.MultiplayerSlider)
                return multiplayerPanel.sliderF;
            if (slider.ID == BingoEnums.RandomizerSlider)
                return randomizerPanel.sliderF;
            return 0f;
        }
    }
}