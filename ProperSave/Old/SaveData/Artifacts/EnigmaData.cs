using ProperSave.Old.Data;
using RoR2.Artifacts;
using System.Runtime.Serialization;

namespace ProperSave.Old.SaveData.Artifacts
{
    public class EnigmaData
    {
        [DataMember(Name = "sier")]
        public RngData serverInitialEquipmentRng;
        [DataMember(Name = "saer")]
        public RngData serverActivationEquipmentRng;

        internal ProperSave.SaveData.Artifacts.EnigmaData Migrate()
        {
            return new ProperSave.SaveData.Artifacts.EnigmaData
            {
                serverInitialEquipmentRng = serverInitialEquipmentRng.Migrate(),
                serverActivationEquipmentRng = serverActivationEquipmentRng.Migrate(),
            };
        }
    }
}
