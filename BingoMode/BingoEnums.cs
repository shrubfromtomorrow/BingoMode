namespace BingoMode
{
    public class BingoEnums
    {
        public static Menu.Slider.SliderID CustomizerSlider;
        public static Menu.Slider.SliderID MultiplayerSlider;
        public static Menu.Slider.SliderID RandomizerSlider;

        public static SoundID BINGO_FINAL_BONG;

        public static ProcessManager.ProcessID BingoCredits;

        public static void Register()
        {
            CustomizerSlider = new("CustomizerSlider", true);
            MultiplayerSlider = new("MultiplayerSlider", true);
            RandomizerSlider = new("RandomizerSlider", true);

            BINGO_FINAL_BONG = new("BINGO_FINAL_BONG", true);

            BingoCredits = new("BingoCredits", true);
        }
    }
}
