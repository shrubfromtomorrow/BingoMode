namespace BingoMode
{
    public class BingoEnums
    {
        public static ProcessManager.ProcessID BingoWinScreen;
        public static ProcessManager.ProcessID BingoLoseScreen;

        public static Menu.Slider.SliderID CustomizerSlider;
        public static Menu.Slider.SliderID MultiplayerSlider;

        //public static SoundID Bingo_Complete;
        //public static SoundID Bingo_Complete_Enemy;
        //public static SoundID Square_Complete;
        //public static SoundID Square_Complete_Enemy;
        //public static SoundID Square_Complete_Almost_Win;
        //public static SoundID Square_Failed;
        //public static SoundID Square_Locked;
        //public static SoundID Square_Progress;

        public static void Register()
        {
            BingoWinScreen = new("BingoWinScreen", true);
            BingoLoseScreen = new("BingoLoseScreen", true);

            CustomizerSlider = new("CustomizerSlider", true);
            MultiplayerSlider = new("MultiplayerSlider", true);

            //Bingo_Complete = new("Bingo_Complete", true);
            //Bingo_Complete_Enemy = new("Bingo_Complete_Enemy", true);
            //Square_Complete = new("Square_Complete", true);
            //Square_Complete_Enemy = new("Square_Complete_Enemy", true);
            //Square_Complete_Almost_Win = new("Square_Complete_Almost_Win", true);
            //Square_Failed = new("Square_Failed", true);
            //Square_Locked = new("Square_Locked", true);
            //Square_Progress = new("Square_Progress", true);
        }
    }
}
