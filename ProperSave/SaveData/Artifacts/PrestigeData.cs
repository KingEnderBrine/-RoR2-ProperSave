using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using ProperSave.Utils;
using RoR2;
using RoR2.Artifacts;

namespace ProperSave.SaveData.Artifacts
{
    public class PrestigeData
    {
        public int mountainShrineCount;

        internal static PrestigeData Create()
        {
            var data = new PrestigeData();
            if (Run.instance.TryGetComponent<PrestigeBulwarkManager>(out var manager))
            {
                data.mountainShrineCount = manager.mountainShrineCount;
            }

            return data;
        }

        internal void LoadData()
        {
            if (!Run.instance.TryGetComponent<PrestigeBulwarkManager>(out var manager))
            {
                return;
            }

            manager.mountainShrineCount = mountainShrineCount;
        }

        internal static PrestigeData Read(ReaderContext context)
        {
            var data = new PrestigeData();
            data.mountainShrineCount = context.Reader.ReadInt32();

            return data;
        }

        internal void Write(WriterContext context)
        {
            context.Writer.Write(mountainShrineCount);
        }
    }
}
