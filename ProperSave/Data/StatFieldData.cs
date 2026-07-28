using ProperSave.Utils;

namespace ProperSave.Data
{
    public class StatFieldData
    {
        public int index;
        public ulong value;

        internal static StatFieldData Read(ReaderContext context)
        {
            var data = new StatFieldData();
            var reader = context.Reader;
            var version = context.Version;

            data.index = SharedIndexHelpers.ResolveStatField(version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32(), context);
            data.value = reader.ReadUInt64();

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(SharedIndexHelpers.FromStatField(index, context));
            writer.Write(value);
        }
    }
}
