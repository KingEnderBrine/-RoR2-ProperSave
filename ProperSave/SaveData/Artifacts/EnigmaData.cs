using ProperSave.Data;
using ProperSave.Utils;
using RoR2.Artifacts;

namespace ProperSave.SaveData.Artifacts
{
    public class EnigmaData
    {
        public RngData serverInitialEquipmentRng;
        public RngData serverActivationEquipmentRng;

        internal static EnigmaData Create()
        {
            return new EnigmaData
            {
                serverInitialEquipmentRng = RngData.Create(EnigmaArtifactManager.serverInitialEquipmentRng),
                serverActivationEquipmentRng = RngData.Create(EnigmaArtifactManager.serverActivationEquipmentRng),
            };
        }

        internal void LoadData()
        {
            serverInitialEquipmentRng.LoadDataRef(ref EnigmaArtifactManager.serverInitialEquipmentRng);
            serverActivationEquipmentRng.LoadDataRef(ref EnigmaArtifactManager.serverActivationEquipmentRng);
        }

        internal static EnigmaData Read(ReaderContext context)
        {
            var data = new EnigmaData();
            data.serverInitialEquipmentRng = RngData.Read(context);
            data.serverActivationEquipmentRng = RngData.Read(context);

            return data;
        }

        internal void Write(WriterContext context)
        {
            serverInitialEquipmentRng.Write(context);
            serverActivationEquipmentRng.Write(context);
        }
    }
}
