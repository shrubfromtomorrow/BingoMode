namespace BingoMode.BingoSteamworks
{
    public struct LobbyFilters
    {
        public string text;
        public bool friendsOnly;

        public LobbyFilters(string text, int distance, bool friendsOnly)
        {
            this.text = text;
            this.friendsOnly = friendsOnly;
        }
    }
}
