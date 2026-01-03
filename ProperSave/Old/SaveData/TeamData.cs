using RoR2;
using System.Runtime.Serialization;

namespace ProperSave.Old.SaveData
{
    public class TeamData
    {
        [DataMember(Name = "e")]
        public long expirience;

        internal ProperSave.SaveData.TeamData Migrate()
        {
            return new ProperSave.SaveData.TeamData
            {
                experience = expirience,
            };
        }
    }
}
