using BepInEx.Logging;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;


namespace BingoMode
{
    public class BingoModOptions : OptionInterface
    {
        public readonly Configurable<KeyCode> HUDKeybindKeyboard;
        public readonly Configurable<KeyCode> HUDKeybindC1;
        public readonly Configurable<KeyCode> HUDKeybindC2;
        public readonly Configurable<KeyCode> HUDKeybindC3;
        public readonly Configurable<KeyCode> HUDKeybindC4;
        public readonly Configurable<KeyCode> ResetBind;
        public readonly Configurable<string> SinglePlayerTeam;
        public readonly Configurable<bool> FillIcons;
        public readonly Configurable<bool> UseMapInput;
        public readonly Configurable<bool> PlayMenuSong;
        public readonly Configurable<bool> PlayEndingSong;
        public readonly Configurable<bool> PlayDangerSong;
        public readonly Configurable<bool> DiscordRichPresence;

        public readonly Configurable<bool> DialCharged;
        public readonly Configurable<int> DialAmount;

        private UIelement[] optionse;
        private UIelement[] optionse1;
        private bool greyedOut;

        public BingoModOptions(Plugin plugin)
        {
            HUDKeybindKeyboard = config.Bind<KeyCode>("HUDKeybind", KeyCode.Space);
            HUDKeybindC1 = config.Bind<KeyCode>("HUDKeybindC1", KeyCode.Joystick1Button5);
            HUDKeybindC2 = config.Bind<KeyCode>("HUDKeybindC2", KeyCode.Joystick2Button5);
            HUDKeybindC3 = config.Bind<KeyCode>("HUDKeybindC3", KeyCode.Joystick3Button5);
            HUDKeybindC4 = config.Bind<KeyCode>("HUDKeybindC4", KeyCode.Joystick4Button5);

            ResetBind = config.Bind<KeyCode>("Reset", KeyCode.Slash);

            SinglePlayerTeam = config.Bind<string>("SinglePlayerTeam", "Red");
            FillIcons = config.Bind<bool>("FillIcons", false);
            UseMapInput = config.Bind<bool>("UseMapInput", false);
            PlayMenuSong = config.Bind<bool>("PlayMenuSong", true);
            PlayEndingSong = config.Bind<bool>("PlayEndingSong", true);
            PlayDangerSong = config.Bind<bool>("PlayDangerSong", true);
            DiscordRichPresence = config.Bind<bool>("DiscordRichPresence", true);


            DialCharged = config.Bind<bool>("DialCharged", true);
            DialAmount = config.Bind<int>("DialAmount", 50, new ConfigurableInfo("", new ConfigAcceptableRange<int>(1, 100)));
        }

        public override void Initialize()
        {
            base.Initialize();

            OpTab tab = new OpTab(this, Translate("Main"));
            OpTab tab1 = new OpTab(this, Translate("Gameplay"));
            Tabs = new[] { tab, tab1 };

            optionse = new UIelement[]
            {
                new OpLabel(10f, 560f, Translate("Bingo Mode Config"), true),
                new OpLabel(10f, 512f, Translate("Open Bingo HUD keybind:")) {alignment = FLabelAlignment.Left, description = Translate("Which button opens/closes the Bingo grid in game")},
                new OpLabel(320f, 510f, Translate("-  Keyboard")) {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindKeyboard, new Vector2(170f, 505f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.AnyController) {description = Translate("Which button opens/closes the Bingo grid in game")},

                new OpLabel(320f, 470f, Translate("-  Controller 1")) {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC1, new Vector2(170f, 465f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller1),
                new OpLabel(320f, 430f, Translate("-  Controller 2")) {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC2, new Vector2(170f, 425f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller2),
                new OpLabel(320f, 390f, Translate("-  Controller 3")) {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC3, new Vector2(170f, 385f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller3),
                new OpLabel(320f, 350f, Translate("-  Controller 4")) {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC4, new Vector2(170f, 345f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller4),

                new OpLabel(430f, 512f, Translate("Use map input instead:")) {alignment = FLabelAlignment.Left},
                new OpCheckBox(UseMapInput, 560f, 509f),

                new OpLabel(10f, 300f, Translate("Quick reset keybind:")) {alignment = FLabelAlignment.Left},
                new OpKeyBinder(ResetBind, new Vector2(170f, 295f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.AnyController),

                new OpLabel(10f, 263f, Translate("Play custom music in bingo menu:")) {alignment = FLabelAlignment.Left},
                new OpCheckBox(PlayMenuSong, 288f, 260f),
                
                new OpLabel(10f, 223f, Translate("Play custom music when game ends:")) {alignment = FLabelAlignment.Left},
                new OpCheckBox(PlayEndingSong, 288f, 220f),
                
                new OpLabel(10f, 183f, Translate("Play custom music when your team is losing:")) {alignment = FLabelAlignment.Left},
                new OpCheckBox(PlayDangerSong, 288f, 180f),
                
                new OpLabel(10f, 143f, Translate("Singleplayer team color:")) {alignment = FLabelAlignment.Left, description = Translate("Which team's color to use in singleplayer")},
                new OpComboBox(SinglePlayerTeam, new Vector2(170f, 140f), 140f, new string[] {Translate("Red"), Translate("Blue"), Translate("Green"), Translate("Orange"), Translate("Pink"), Translate("Cyan"), Translate("Black"), Translate("Hurricane") }) {description = Translate("Which team's color to use in singleplayer")},

                new OpLabel(10f, 103f, Translate("Fill icon sprites:")) {alignment = FLabelAlignment.Left, description = Translate("Fill the crosses and arrows on certain goals")},
                new OpCheckBox(FillIcons, 288f, 100f),

                new OpLabel(10f, 63f, Translate("Discord Rich Presence:")) {alignment = FLabelAlignment.Left, description = Translate("Show Bingo Mode as your Discord activity (restart to take effect)")},
                new OpCheckBox(DiscordRichPresence, 288f, 60f),
            };
            tab.AddItems(optionse);

            optionse1 = new UIelement[]
            {
                new OpLabel(10f, 560f, Translate("Bingo Mode Gameplay Config"), true),

                new OpLabel(10f, 512f, Translate("Start with the Dial Warp perk fully charged:")) {alignment = FLabelAlignment.Left},
                new OpCheckBox(DialCharged, 258f, 509f),

                new OpLabel(10f, 472f, Translate("Ripple eggs required for dial warp:")) {alignment = FLabelAlignment.Left},
                new OpUpdown(DialAmount, new Vector2(210f, 466f), 60f),
            };
            tab1.AddItems(optionse1);
        }

        public override void Update()
        {
            base.Update();
            
            foreach (var item in Tabs[0].items)
            {
                if (item is OpCheckBox bock && bock.cfgEntry == UseMapInput)
                {
                    greyedOut = bock.GetValueBool();
                }

                if (item is OpKeyBinder g && !(g.cfgEntry == ResetBind))
                {
                    g.greyedOut = greyedOut;
                }
            }
        }
    }
}
