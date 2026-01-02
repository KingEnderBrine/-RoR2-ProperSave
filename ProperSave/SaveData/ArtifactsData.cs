using ProperSave.SaveData.Artifacts;
using ProperSave.Utils;
using System;
using System.Runtime.Serialization;

namespace ProperSave.SaveData
{
    public class ArtifactsData
    {
        public EnigmaData enigmaData;
        public PrestigeData prestigeData;

        internal static ArtifactsData Create()
        {
            return new ArtifactsData
            {
                enigmaData = EnigmaData.Create(),
                prestigeData = PrestigeData.Create(),
            };
        }

        internal void LoadData()
        {
            enigmaData.LoadData();
            prestigeData.LoadData();
        }

        internal static ArtifactsData Read(ReaderContext context)
        {
            var data = new ArtifactsData();
            data.enigmaData = EnigmaData.Read(context);
            data.prestigeData = PrestigeData.Read(context);

            return data;
        }
        
        internal void Write(WriterContext context)
        {
            enigmaData.Write(context);
            prestigeData.Write(context);
        }
    }
}
