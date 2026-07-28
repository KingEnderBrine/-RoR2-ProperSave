using ProperSave.Utils;
using RoR2;
using System.Collections.Generic;

namespace ProperSave.SaveData
{
    public class RunArtifactsData
    {
        public List<ArtifactIndex> artifacts = new List<ArtifactIndex>();
        
        internal static RunArtifactsData Create()
        {
            var data = new RunArtifactsData();

            var artifactController = UnityEngine.Object.FindObjectOfType<ArtifactTrialMissionController>();
            var trialArtifact = (ArtifactIndex)(artifactController?.currentArtifactIndex ?? -1);

            foreach (var artifact in RunArtifactManager.enabledArtifactsEnumerable)
            {
                if (trialArtifact == artifact.artifactIndex && !artifactController.artifactWasEnabled)
                {
                    continue;
                }
                data.artifacts.Add(artifact.artifactIndex);
            }

            return data;
        }

        internal void LoadData()
        {
            for (var i = 0; i < ArtifactCatalog.artifactCount; i++)
            {
                var artifactDef = ArtifactCatalog.GetArtifactDef((ArtifactIndex)i);
                RunArtifactManager.instance.SetArtifactEnabled(artifactDef, false);
            }

            foreach (var artifactIndex in artifacts)
            {
                if (artifactIndex == ArtifactIndex.None)
                {
                    continue;
                }

                RunArtifactManager.instance.SetArtifactEnabled(ArtifactCatalog.GetArtifactDef(artifactIndex), true);
            }
        }

        internal static RunArtifactsData Read(ReaderContext context)
        {
            var data = new RunArtifactsData();
            var reader = context.Reader;
            var version = context.Version;

            var artifactsCount = version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32();
            data.artifacts = new List<ArtifactIndex>(artifactsCount);
            for (var i = 0; i < artifactsCount; i++)
            {
                data.artifacts.Add(SharedIndexHelpers.ResolveArtifact(version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32(), context));
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(artifacts.Count);
            for (var i = 0; i < artifacts.Count; i++)
            {
                writer.WritePacked(SharedIndexHelpers.FromArtifact(artifacts[i], context));
            }
        }
    }
}
