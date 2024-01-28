namespace Models
{
    public class AccessibilitySettings : BaseModel
    {
        public readonly string userID;
        public bool hasReducedMobility;
        public bool isOneHanded;

        public AccessibilitySettings(string id, string userID, bool hasReducedMobility, bool isOneHanded) : base(id)
        {
            this.userID = userID;
            this.hasReducedMobility = hasReducedMobility;
            this.isOneHanded = isOneHanded;
        }

        public AccessibilitySettings(string id) : base()
        {
            userID = id;
            hasReducedMobility = false;
            isOneHanded = false;
        }
    }
}