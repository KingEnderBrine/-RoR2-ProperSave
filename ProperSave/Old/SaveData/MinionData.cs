using ProperSave.Old.Data;
using RoR2;
using RoR2.CharacterAI;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace ProperSave.Old.SaveData
{
    public class MinionData
    {
        [DataMember(Name = "mi")]
        public int masterIndex;

        [DataMember(Name = "m")]
        public CharacterMasterData master;

        [DataMember(Name = "dld")]
        public DevotedLemurianData devotedLemurianData;

        [DataMember(Name = "drd")]
        public DroneRepairData droneRepairData;

        internal ProperSave.SaveData.MinionData Migrate()
        {
            return new ProperSave.SaveData.MinionData
            {
                devotedLemurianData = devotedLemurianData?.Migrate(),
                droneRepairData = droneRepairData?.Migrate(),
                master = master.Migrate(),
                masterIndex = (MasterCatalog.MasterIndex)masterIndex,
            };
        }
    }
}
