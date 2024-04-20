namespace BingoMode.BingoSteamworks
{
    public struct LobbyFilters
    {
        public string text;
        public int distance;
        public bool friendsOnly;

        public LobbyFilters(string text, int distance, bool friendsOnly)
        {
            this.text = text;
            this.distance = distance;
            this.friendsOnly = friendsOnly;
        }
    }
}
