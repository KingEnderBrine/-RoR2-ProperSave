using RoR2.Artifacts;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace ProperSave.Data.Artifacts
{
    public class EnigmaData
    {
        [DataMember(Name = "sier")]
        public RngData serverInitialEquipmentRng;
        [DataMember(Name = "saer")]
        public RngData serverActivationEquipmentRng;

        public EnigmaData()
        {
            serverInitialEquipmentRng = new RngData(typeof(EnigmaArtifactManager).GetPrivateStaticField<Xoroshiro128Plus>("serverInitialEquipmentRng"));
            serverActivationEquipmentRng = new RngData(typeof(EnigmaArtifactManager).GetPrivateStaticField<Xoroshiro128Plus>("serverActivationEquipmentRng"));
        }

        public void LoadData()
        {
            var sier = typeof(EnigmaArtifactManager).GetPrivateStaticField<Xoroshiro128Plus>("serverInitialEquipmentRng");
            serverInitialEquipmentRng.LoadDataRef(ref sier);

            var saer = typeof(EnigmaArtifactManager).GetPrivateStaticField<Xoroshiro128Plus>("serverActivationEquipmentRng");
            serverActivationEquipmentRng.LoadDataRef(ref saer);
        }
    }
}
