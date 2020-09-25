using RoR2;
using System.Runtime.Serialization;

namespace ProperSave.SaveData
{
    public class TeamData
    {
        [DataMember(Name = "e")]
        public long expirience;

        internal TeamData()
        {
            expirience = (long)TeamManager.instance.GetTeamExperience(TeamIndex.Player);
        }

        internal void LoadData()
        {
            TeamManager.instance.GiveTeamExperience(TeamIndex.Player, (ulong)expirience);
        }
    }
}
