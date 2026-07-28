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
            var reader = context.Reader;
            var version = context.Version;
            data.mountainShrineCount = version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32();

            return data;
        }

        internal void Write(WriterContext context)
        {
            context.Writer.WritePacked(mountainShrineCount);
        }
    }
}
