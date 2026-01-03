using System.Runtime.Serialization;
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

            data.itemIndex = SharedIndexHelpers.ResolveItem(reader.ReadInt32(), context);
            data.count = reader.ReadInt32();
            data.channeledCount = reader.ReadInt32();
            data.tempCount = reader.ReadInt32();
            data.tempFixedTime = reader.ReadSingle();

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(SharedIndexHelpers.FromItem(itemIndex, context));
            writer.Write(count);
            writer.Write(channeledCount);
            writer.Write(tempCount);
            writer.Write(tempFixedTime);
        }
    }
}
