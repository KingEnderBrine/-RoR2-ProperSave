using System;
using System.Collections.Generic;
using System.Reflection;
using ProperSave.Utils;

namespace ProperSave.Data
{
    public class TypeData
    {
        //For future custom read/write support
        public int Version { get; set; }
        public TypeName Name { get; set; }
        public int TypeIndex { get; set; }
        public ObjectType ObjectType { get; set; }
        public List<FieldData> Fields { get; set; }
        public TypeData CollectionArgumentType { get; set; }
        public Type Type { get; set; }
        public MethodInfo AddCollectionElementMethod { get; set; }
        public PropertyInfo CollectionCountProperty { get; set; }
        public ConstructorInfo Constructor { get; set; }
        public int ArrayRank { get; set; }

        public bool IsPrimitive => ObjectType < ObjectType.Array;
        public bool IsCollection => ObjectType.String < ObjectType && ObjectType < ObjectType.Class;

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;
            writer.Write((byte)ObjectType);
            if (IsPrimitive)
            {
                return;
            }

            writer.WritePacked(Version);
            Name.Write(context);

            switch (ObjectType)
            {
                case ObjectType.Array:
                {
                    writer.WritePacked(CollectionArgumentType.TypeIndex);
                    writer.WritePacked(ArrayRank);
                    break;
                }
                case ObjectType.Collection:
                {
                    writer.WritePacked(CollectionArgumentType.TypeIndex);
                    break;
                }
                case ObjectType.KeyValuePair:
                case ObjectType.Object:
                case ObjectType.ValueType:
                case ObjectType.Class:
                {
                    writer.WritePacked(Fields.Count);
                    for (var i = 0; i < Fields.Count; i++)
                    {
                        Fields[i].Write(context);
                    }
                    break;
                }
            }
        }

        internal void Read(ReaderContext context)
        {
            var reader = context.Reader;
            ObjectType = (ObjectType)reader.ReadByte();
            if (IsPrimitive)
            {
                return;
            }

            Version = reader.ReadPackedInt32();
            Name = TypeName.Read(context);

            switch (ObjectType)
            {
                case ObjectType.Array:
                {
                    CollectionArgumentType = context.Types[reader.ReadPackedInt32()];
                    ArrayRank = reader.ReadPackedInt32();
                    break;
                }
                case ObjectType.Collection:
                {
                    CollectionArgumentType = context.Types[reader.ReadPackedInt32()];
                    break;
                }
                case ObjectType.KeyValuePair:
                case ObjectType.Object:
                case ObjectType.ValueType:
                case ObjectType.Class:
                {
                    var count = reader.ReadPackedInt32();
                    Fields = new List<FieldData>(count);
                    for (var i = 0; i < count; i++)
                    {
                        Fields.Add(FieldData.Read(context));
                    }
                    break;
                }
            }
        }

        internal void AddCollectionElement(object obj, object value)
        {
            var buffer = ObjectBuffer.Buffers[1];
            buffer[0] = value;
            AddCollectionElementMethod?.Invoke(obj, buffer);
        }

        internal int CollectionCount(object obj)
        {
            return (int)CollectionCountProperty.GetValue(obj);
        }

        internal object CreateObject(object[] parameters)
        {
            return Constructor?.Invoke(parameters);
        }
    }
}
