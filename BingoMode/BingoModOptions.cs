using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using RWCustom;
using UnityEngine;


namespace BingoMode
{
    public class BingoModOptions : OptionInterface
    {
        public readonly Configurable<KeyCode> HUDKeybind;
        public readonly Configurable<string> SinglePlayerTeam;

        private UIelement[] optionse;

        public BingoModOptions() : base()
        {
            HUDKeybind = config.Bind<KeyCode>("HUDKeybind", KeyCode.Tab);
            SinglePlayerTeam = config.Bind<string>("SinglePlayerTeam", "Red");
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
                new OpLabel(10f, 470f, "Singleplayer team color:") {alignment = FLabelAlignment.Left, description = "Which team's color to use in singleplayer"},
                new OpKeyBinder(HUDKeybind, new Vector2(170f, 505f), new Vector2(100f, 20f), false),
                new OpComboBox(SinglePlayerTeam, new Vector2(170f, 465f), 100f, ["Red", "Blue", "Green", "Yellow", "Pink", "Cyan", "Orange", "Purple"])
            };
            tab.AddItems(optionse);
        }
    }
}
