using ProperSave.SaveData.Artifacts;
using System.Runtime.Serialization;

namespace ProperSave.SaveData
{
    public class ArtifactsData
    {
        [DataMember(Name = "ed")]
        public EnigmaData enigmaData;
        [DataMember(Name = "pd")]
        public PrestigeData prestigeData;

        internal ArtifactsData()
        {
            enigmaData = new EnigmaData();
            prestigeData = new PrestigeData();
        }

        internal void LoadData()
        {
            enigmaData.LoadData();
            prestigeData.LoadData();
        }
    }
}
