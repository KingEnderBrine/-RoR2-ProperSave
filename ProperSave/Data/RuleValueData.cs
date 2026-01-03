using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
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

            data.index = SharedIndexHelpers.ResolveRule(reader.ReadInt32(), context);
            data.value = reader.ReadByte();

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(SharedIndexHelpers.FromRule(index, context));
            writer.Write(value);
        }
    }
}
