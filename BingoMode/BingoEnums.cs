namespace BingoMode
{
    public class BingoEnums
    {
        public static ProcessManager.ProcessID BingoWinScreen;
        public static ProcessManager.ProcessID BingoLoseScreen;
        public static Menu.Slider.SliderID CustomizerSlider;
        public static Menu.Slider.SliderID MultiplayerSlider;

        public static void Register()
        {
            BingoWinScreen = new("BingoWinScreen", true);
            BingoLoseScreen = new("BingoLoseScreen", true);
            CustomizerSlider = new("CustomizerSlider", true);
            MultiplayerSlider = new("MultiplayerSlider", true);
        }
    }
}
