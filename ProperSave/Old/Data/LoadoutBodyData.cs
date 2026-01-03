using RoR2;
using System.Runtime.Serialization;

namespace ProperSave.Old.Data
{
    public class LoadoutBodyData
    {
        [DataMember(Name = "bi")]
        public int bodyIndex;
        [DataMember(Name = "sp")]
        public uint skinPreference;
        [DataMember(Name = "sps")]
        public uint[] skillPreferences;

        internal ProperSave.Data.LoadoutBodyData Migrate()
        {
            return new ProperSave.Data.LoadoutBodyData
            {
                bodyIndex = (BodyIndex)bodyIndex,
                skinPreference = skinPreference,
                skillPreferences = skillPreferences,
            };
        }
    }
}
