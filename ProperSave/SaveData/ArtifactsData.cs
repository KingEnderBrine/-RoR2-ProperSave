using ProperSave.SaveData.Artifacts;
using System.Runtime.Serialization;

namespace ProperSave.SaveData
{
    public class ArtifactsData
    {
        [DataMember(Name = "ed")]
        public EnigmaData EnigmaData;

        internal ArtifactsData()
        {
            EnigmaData = new EnigmaData();
        }

        internal void LoadData()
        {
            EnigmaData.LoadData();
        }
    }
}
