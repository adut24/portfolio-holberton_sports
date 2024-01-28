namespace Models
{
    public class HighScore : BaseModel
    {
        public readonly string userID;
        public string sport;
        public int score;

        public HighScore(string id, string userID, string sport, int score) : base(id)
        {
            this.userID = userID;
            this.sport = sport;
            this.score = score;
        }
    }
}