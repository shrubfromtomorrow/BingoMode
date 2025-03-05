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

            counter = 2800;
        }

        public override void Update()
        {
            base.Update();

            if (counter > 0)
            {
                counter--;
                if (counter == 0)
                {
                    FadeOut(400f);
                }
            }
        }
    }
}
