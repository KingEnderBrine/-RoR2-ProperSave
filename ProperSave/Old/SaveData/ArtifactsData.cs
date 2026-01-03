using ProperSave.Old.SaveData.Artifacts;
using System.Runtime.Serialization;

namespace ProperSave.Old.SaveData
{
    public class ArtifactsData
    {
        [DataMember(Name = "ed")]
        public EnigmaData enigmaData;
        [DataMember(Name = "pd")]
        public PrestigeData prestigeData;

        internal ProperSave.SaveData.ArtifactsData Migrate()
        {
            return new ProperSave.SaveData.ArtifactsData
            {
                enigmaData = enigmaData?.Migrate(),
                prestigeData = prestigeData?.Migrate(),
            };
        }
    }
}
