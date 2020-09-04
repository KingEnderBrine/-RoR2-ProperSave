using R2API.Utils;
using RoR2;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class RunArtifactsData
    {
        [DataMember(Name = "a")]
        public bool[] artifacts;
        
        public RunArtifactsData()
        {
            artifacts = new bool[ArtifactCatalog.artifactCount];
            foreach (var artifact in RunArtifactManager.enabledArtifactsEnumerable)
            {
                artifacts[(int)artifact.artifactIndex] = true;
            }

            var artifactController = UnityEngine.Object.FindObjectOfType<ArtifactTrialMissionController>();
            var trialArtifact = artifactController?.GetFieldValue<int>("currentArtifactIndex") ?? -1;

            if (trialArtifact != -1)
            {
                artifacts[trialArtifact] = artifactController.artifactWasEnabled;
            }
        }

        public void LoadData()
        {
            for (int i = 0; i < ArtifactCatalog.artifactCount; i++)
            {
                var artifactDef = ArtifactCatalog.GetArtifactDef((ArtifactIndex)i);
                RunArtifactManager.instance.InvokeMethod("SetArtifactEnabled", new object[] { artifactDef, artifacts.ElementAtOrDefault(i) });
            }
        }
    }
}
