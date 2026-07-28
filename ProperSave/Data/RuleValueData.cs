using ProperSave.Utils;

namespace ProperSave.Data
{
    public class RuleValueData
    {
        public int index;
        public byte value;

        internal static RuleValueData Read(ReaderContext context)
        {
            var data = new RuleValueData();
            var reader = context.Reader;
            var version = context.Version;

            data.index = SharedIndexHelpers.ResolveRule(version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32(), context);
            data.value = reader.ReadByte();

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(SharedIndexHelpers.FromRule(index, context));
            writer.Write(value);
        }
    }
}
