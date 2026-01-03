using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using RoR2;

namespace ProperSave.Old.Data
{
    public class DevotedLemurianData
    {
        [DataMember(Name = "ii")]
        public int itemIndex;

        [DataMember(Name = "dev")]
        public int devotedEvolutionLevel;

        internal ProperSave.Data.DevotedLemurianData Migrate()
        {
            return new ProperSave.Data.DevotedLemurianData
            {
                devotedEvolutionLevel = devotedEvolutionLevel,
                itemIndex = (ItemIndex)itemIndex,
            };
        }
    }
}
