using RoR2;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public partial class LoadoutData
    {
        public class LoadoutBodyData
        {
            [DataMember(Name = "bi")]
            public int bodyIndex;
            [DataMember(Name = "sp")]
            public uint skinPreference;
            [DataMember(Name = "sps")]
            public uint[] skillPreferences;

            public LoadoutBodyData(Loadout.BodyLoadoutManager.BodyLoadout bodyLoadout)
            {
                bodyIndex = bodyLoadout.bodyIndex;
                skinPreference = bodyLoadout.skinPreference;
                skillPreferences = bodyLoadout.skillPreferences;
            }

            public Loadout.BodyLoadoutManager.BodyLoadout Load()
            {
                return new Loadout.BodyLoadoutManager.BodyLoadout
                {
                    bodyIndex = bodyIndex,
                    skinPreference = skinPreference,
                    skillPreferences = skillPreferences
                };
            }
        }
    }
}
