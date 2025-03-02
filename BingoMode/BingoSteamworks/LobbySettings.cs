namespace BingoMode.BingoSteamworks
{
    public class LobbySettings
    {
        public enum AllowUnlocks
        {
            Any,
            Inherited,
            None
        }

        public BingoData.BingoGameMode gamemode;
        public bool friendsOnly;
        public AllowUnlocks perks;
        public AllowUnlocks burdens;
        public bool hostMods;

        public LobbySettings()
        {
        }

        public void Reset()
        {
            gamemode = BingoData.BingoGameMode.Bingo;
            perks = AllowUnlocks.Any;
            burdens = AllowUnlocks.Any;
            hostMods = false;
        }
    }
}
