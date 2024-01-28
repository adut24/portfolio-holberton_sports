using System.Collections.Generic;

namespace Models
{
    public class User : BaseModel
    {
        public string nickname;
        public VolumeSettings volumeSettings;
        public AccessibilitySettings accessibilitySettings;
        public List<HighScore> highScores;
        public List<MatchHistory> matchHistory;

        public User(string id = null) : base(id)
        {
            highScores = new();
            matchHistory = new();
        }

        public User(string id, string nickname) : base(id)
        {
            this.nickname = nickname;
            highScores = new();
            matchHistory = new();
        }
    }
}
