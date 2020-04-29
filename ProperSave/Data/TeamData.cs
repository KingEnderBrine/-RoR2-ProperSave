using RoR2;
using System;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class TeamData
    {
        [DataMember(Name = "e")]
        public long expirience;

        public TeamData()
        {
            expirience = (long)TeamManager.instance.GetTeamExperience(TeamIndex.Player);
        }

        public void LoadData()
        {
            TeamManager.instance.GiveTeamExperience(TeamIndex.Player, (ulong)expirience);
        }
    }
}
