using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ProperSave.Utils;

namespace ProperSave.Data
{
    public class FieldData
    {
        public string Name { get; set; }
        public TypeData Type { get; set; }
        public PropertyInfo Property { get; set; }
        public FieldInfo Field { get; set; }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;
            writer.WritePacked(context.SharedStrings.AddOrIndexOf(Name));
            writer.WritePacked(Type.TypeIndex);
        }

        internal static FieldData Read(ReaderContext context)
        {
            var data = new FieldData();
            var reader = context.Reader;

            data.Name = context.SharedStrings.GetSafe(reader.ReadPackedInt32());
            data.Type = context.Types[reader.ReadPackedInt32()];

            return data;
        }

        internal object GetValue(object obj)
        {
            if (Field != null)
            {
                return Field.GetValue(obj);
            }
            else if (Property != null)
            {
                return Property.GetValue(obj);
            }

            throw new NotSupportedException("GetValue on invalid type");
        }

        internal void SetValue(object obj, object value)
        {
            if (Field != null)
            {
                Field.SetValue(obj, value);
            }
            else if (Property != null)
            {
                Property.SetValue(obj, value);
            }
        }
    }
}
