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
        public readonly Configurable<bool> UseMapInput;
        public readonly Configurable<bool> PlayMenuSong;
        public readonly Configurable<bool> PlayEndingSong;
        public readonly Configurable<bool> PlayDangerSong;

        private UIelement[] optionse;
        private bool greyedOut;

        public BingoModOptions(Plugin plugin)
        {
            HUDKeybindKeyboard = config.Bind<KeyCode>("HUDKeybind", KeyCode.Space);
            HUDKeybindC1 = config.Bind<KeyCode>("HUDKeybindC1", KeyCode.Joystick1Button5);
            HUDKeybindC2 = config.Bind<KeyCode>("HUDKeybindC2", KeyCode.Joystick2Button5);
            HUDKeybindC3 = config.Bind<KeyCode>("HUDKeybindC3", KeyCode.Joystick3Button5);
            HUDKeybindC4 = config.Bind<KeyCode>("HUDKeybindC4", KeyCode.Joystick4Button5);

            ResetBind = config.Bind<KeyCode>("Reset", KeyCode.Backspace);

            SinglePlayerTeam = config.Bind<string>("SinglePlayerTeam", "Red");
            UseMapInput = config.Bind<bool>("UseMapInput", false);
            PlayMenuSong = config.Bind<bool>("PlayMenuSong", true);
            PlayEndingSong = config.Bind<bool>("PlayEndingSong", true);
            PlayDangerSong = config.Bind<bool>("PlayDangerSong", true);
        }

        public override void Initialize()
        {
            base.Initialize();

            OpTab tab = new OpTab(this, "Config");
            Tabs = new[] { tab };

            optionse = new UIelement[]
            {
                new OpLabel(10f, 560f, "Bingo Mod Config", true),
                new OpLabel(10f, 512f, "Open Bingo HUD keybind:") {alignment = FLabelAlignment.Left, description = "Which button opens/closes the Bingo grid in game"},
                new OpLabel(320f, 510f, "-  Keyboard") {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindKeyboard, new Vector2(170f, 505f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.AnyController) {description = "Which button opens/closes the Bingo grid in game"},

                new OpLabel(320f, 470f, "-  Controller 1") {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC1, new Vector2(170f, 465f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller1),
                new OpLabel(320f, 430f, "-  Controller 2") {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC2, new Vector2(170f, 425f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller2),
                new OpLabel(320f, 390f, "-  Controller 3") {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC3, new Vector2(170f, 385f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller3),
                new OpLabel(320f, 350f, "-  Controller 4") {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC4, new Vector2(170f, 345f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller4),

                new OpLabel(430f, 512f, "Use map input instead:") {alignment = FLabelAlignment.Left},
                new OpCheckBox(UseMapInput, 560f, 509f),

                new OpLabel(10f, 300f, "Quick reset keybind:") {alignment = FLabelAlignment.Left},
                new OpKeyBinder(ResetBind, new Vector2(170f, 295f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.AnyController),

                new OpLabel(10f, 263f, "Play custom music in bingo menu:") {alignment = FLabelAlignment.Left},
                new OpCheckBox(PlayMenuSong, 288f, 260f),
                
                new OpLabel(10f, 223f, "Play custom music when game ends:") {alignment = FLabelAlignment.Left},
                new OpCheckBox(PlayEndingSong, 288f, 220f),
                
                new OpLabel(10f, 183f, "Play custom music when your team is losing:") {alignment = FLabelAlignment.Left},
                new OpCheckBox(PlayDangerSong, 288f, 180f),
                
                new OpLabel(10f, 143f, "Singleplayer team color:") {alignment = FLabelAlignment.Left, description = "Which team's color to use in singleplayer"},
                new OpComboBox(SinglePlayerTeam, new Vector2(170f, 140f), 140f, new string[] {"Red", "Blue", "Green", "Orange", "Pink", "Cyan", "Black", "Hurricane" }) {description = "Which team's color to use in singleplayer"},
            };
            tab.AddItems(optionse);
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

                if (item is OpKeyBinder g)
                {
                    g.greyedOut = greyedOut;
                }
            }
        }
    }
}
