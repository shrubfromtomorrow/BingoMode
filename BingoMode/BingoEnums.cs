
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

        public class LandscapeType
        {
            public static Menu.MenuScene.SceneID Landscape_WRFA;
            public static Menu.MenuScene.SceneID Landscape_WARB;
            public static Menu.MenuScene.SceneID Landscape_WBLA;
            public static Menu.MenuScene.SceneID Landscape_WSKC;
            public static Menu.MenuScene.SceneID Landscape_WTDA;

            public static void RegisterValues()
            {
                Landscape_WRFA = new Menu.MenuScene.SceneID("Landscape_WRFA", true);
                Landscape_WARB = new Menu.MenuScene.SceneID("Landscape_WARB", true);
                Landscape_WBLA = new Menu.MenuScene.SceneID("Landscape_WBLA", true);
                Landscape_WSKC = new Menu.MenuScene.SceneID("Landscape_WSKC", true);
                Landscape_WTDA = new Menu.MenuScene.SceneID("Landscape_WTDA", true);
            }

            public static void UnregisterValues()
            {
                if (Landscape_WRFA != null) { Landscape_WRFA.Unregister(); Landscape_WRFA = null; }
                if (Landscape_WARB != null) { Landscape_WARB.Unregister(); Landscape_WARB = null; }
                if (Landscape_WBLA != null) { Landscape_WBLA.Unregister(); Landscape_WBLA = null; }
                if (Landscape_WSKC != null) { Landscape_WSKC.Unregister(); Landscape_WSKC = null; }
                if (Landscape_WTDA != null) { Landscape_WTDA.Unregister(); Landscape_WTDA = null; }
            }
        }
    }
}
