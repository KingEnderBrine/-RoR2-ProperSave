using ProperSave.Data.Artifacts;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class ArtifactsData
    {
        [DataMember(Name = "ed")]
        public EnigmaData EnigmaData;

        public ArtifactsData()
        {
            EnigmaData = new EnigmaData();
        }

        public void LoadData()
        {
            EnigmaData.LoadData();
        }
    }
}
