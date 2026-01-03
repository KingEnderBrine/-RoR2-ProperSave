using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using RoR2;
using RoR2.Artifacts;

namespace ProperSave.Old.SaveData.Artifacts
{
    public class PrestigeData
    {
        [DataMember(Name = "msc")]
        public int mountainShrineCount;

        internal ProperSave.SaveData.Artifacts.PrestigeData Migrate()
        {
            return new ProperSave.SaveData.Artifacts.PrestigeData
            {
                mountainShrineCount = mountainShrineCount,
            };
        }
    }
}
