using Music;

namespace BingoMode
{
    public class BingoSong : Song
    {
        int counter;

        public BingoSong(MusicPlayer musicPlayer, string name) : base(musicPlayer, name, MusicPlayer.MusicContext.StoryMode)
        {
            priority = 10f;
            stopAtDeath = false;
            stopAtGate = false;
            fadeInTime = 50f;

            if (name == "Bingo - Blithely Beached" || name == "Bingo - Scheming") counter = 2800;
        }

        public override void Update()
        {
            base.Update();

            if (counter > 0)
            {
                counter--;
                if (counter == 0)
                {
                    Plugin.logger.LogFatal("FADIN OUT");
                    FadeOut(400f);
                }
            }
        }
    }
}
