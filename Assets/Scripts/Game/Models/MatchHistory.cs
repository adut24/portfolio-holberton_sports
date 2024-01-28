namespace Models
{
    public class MatchHistory : BaseModel
    {
        public readonly string userID;
        public readonly string opponentID;
        public string sport;
        public int victories;
        public int defeats;

        public MatchHistory(string id, string userID, string opponentID, string sport, int victories, int defeats) : base(id)
        {
            this.userID = userID;
            this.opponentID = opponentID;
            this.sport = sport;
            this.victories = victories;
            this.defeats = defeats;
        }
    }
}