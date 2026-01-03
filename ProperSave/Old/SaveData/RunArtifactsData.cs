using RoR2;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Old.SaveData
{
    public class RunArtifactsData
    {
        [DataMember(Name = "a")]
        public bool[] artifacts;

        internal ProperSave.SaveData.RunArtifactsData Migrate()
        {
            return new ProperSave.SaveData.RunArtifactsData
            {
                artifacts = artifacts
                    .Select((a, i) => a ? (ArtifactIndex)i : ArtifactIndex.None)
                    .Where(a => a != ArtifactIndex.None)
                    .ToList(),
            };
        }
    }
}
