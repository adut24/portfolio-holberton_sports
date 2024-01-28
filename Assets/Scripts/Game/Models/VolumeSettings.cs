namespace Models
{
    public class VolumeSettings : BaseModel
    {
        public readonly string userID;
        public float bgm;
        public float sfx;

        public VolumeSettings(string id, string userID, float bgmVolume, float sfxVolume) : base(id)
        {
            this.userID = userID;
            bgm = bgmVolume;
            sfx = sfxVolume;
        }

        public VolumeSettings(string id) : base()
        {
            userID = id;
            bgm = 1.0f;
            sfx = 1.0f;
        }
    }
}