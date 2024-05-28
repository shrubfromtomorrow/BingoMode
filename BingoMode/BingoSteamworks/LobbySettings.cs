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
        public bool friendsOnly;
        public AllowUnlocks perks;
        public AllowUnlocks burdens;
        public bool banMods; //Ids

        public LobbySettings()
        {
        }

        public void Reset()
        {
            lockout = false;
            perks = AllowUnlocks.Any;
            burdens = AllowUnlocks.Any;
            banMods = false;
        }
    }
}
