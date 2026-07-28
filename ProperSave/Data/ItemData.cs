using ProperSave.Utils;
using RoR2;

namespace ProperSave.Data
{
    public class ItemData
    {
        public ItemIndex itemIndex;
        public int count;
        public int channeledCount;
        public int tempCount;
        public float tempFixedTime;

        internal static ItemData Read(ReaderContext context)
        {
            var data = new ItemData();
            var reader = context.Reader;
            var version = context.Version;

            data.itemIndex = SharedIndexHelpers.ResolveItem(version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32(), context);
            data.count = version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32();
            data.channeledCount = version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32();
            data.tempCount = version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32();
            data.tempFixedTime = reader.ReadSingle();

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(SharedIndexHelpers.FromItem(itemIndex, context));
            writer.WritePacked(count);
            writer.WritePacked(channeledCount);
            writer.WritePacked(tempCount);
            writer.Write(tempFixedTime);
        }
    }
}
