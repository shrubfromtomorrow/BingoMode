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

        public bool lockout;
        public bool gameMode;
        public bool friendsOnly;
        public AllowUnlocks perks;
        public AllowUnlocks burdens;
        public bool banMods; //Ids
        public int maxPlayers;

        public LobbySettings()
        {
        }

        public void Reset()
        {
            lockout = false;
            perks = AllowUnlocks.Any;
            burdens = AllowUnlocks.Any;
            maxPlayers = 0;
        }
    }
}
