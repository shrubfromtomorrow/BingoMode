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
        public readonly Configurable<string> SinglePlayerTeam;
        public readonly Configurable<bool> UseMapInput;

        private UIelement[] optionse;
        private bool greyedOut;

        public BingoModOptions(Plugin plugin)
        {
            HUDKeybindKeyboard = config.Bind<KeyCode>("HUDKeybind", KeyCode.Space);
            HUDKeybindC1 = config.Bind<KeyCode>("HUDKeybindC1", KeyCode.Joystick1Button5);
            HUDKeybindC2 = config.Bind<KeyCode>("HUDKeybindC2", KeyCode.Joystick2Button5);
            HUDKeybindC3 = config.Bind<KeyCode>("HUDKeybindC3", KeyCode.Joystick3Button5);
            HUDKeybindC4 = config.Bind<KeyCode>("HUDKeybindC4", KeyCode.Joystick4Button5);
            SinglePlayerTeam = config.Bind<string>("SinglePlayerTeam", "Red");
            UseMapInput = config.Bind<bool>("UseMapInput", false);
        }

        public override void Initialize()
        {
            base.Initialize();

            OpTab tab = new OpTab(this, "Config");
            Tabs = new[] { tab };

            optionse = new UIelement[]
            {
                new OpLabel(10f, 560f, "Bingo Mod Config", true),
                new OpLabel(10f, 510f, "Open Bingo HUD keybind:") {alignment = FLabelAlignment.Left, description = "Which button opens/closes the Bingo grid in game"},
                new OpLabel(320f, 510f, "- Keyboard") {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindKeyboard, new Vector2(170f, 505f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.AnyController),

                 new OpLabel(320f, 470f, "-  Controller 1") {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC1, new Vector2(170f, 465f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller1),
                 new OpLabel(320f, 430f, "-  Controller 2") {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC2, new Vector2(170f, 425f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller2),
                 new OpLabel(320f, 390f, "-  Controller 3") {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC3, new Vector2(170f, 385f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller3),
                 new OpLabel(320f, 350f, "-  Controller 4") {alignment = FLabelAlignment.Left},
                new OpKeyBinder(HUDKeybindC4, new Vector2(170f, 345f), new Vector2(140f, 20f), false, OpKeyBinder.BindController.Controller4),

                new OpLabel(10f, 310f, "Singleplayer team color:") {alignment = FLabelAlignment.Left, description = "Which team's color to use in singleplayer"},
                new OpComboBox(SinglePlayerTeam, new Vector2(170f, 310f), 140f, ["Red", "Blue", "Green", "Yellow", "Pink", "Cyan", "Orange", "Purple"]),

                new OpLabel(430f, 510f, "Use map input instead:") {alignment = FLabelAlignment.Left},
                new OpCheckBox(UseMapInput, 560f, 510f)
            };
            tab.AddItems(optionse);
        }

        public override void Update()
        {
            base.Update();
            Plugin.logger.LogMessage(UseMapInput.Value);
            
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
