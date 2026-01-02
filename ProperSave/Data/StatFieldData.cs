using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
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

            data.index = SharedIndexHelpers.ResolveStatField(reader.ReadInt32(), context);
            data.value = reader.ReadUInt64();

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(SharedIndexHelpers.FromStatField(index, context));
            writer.Write(value);
        }
    }
}
