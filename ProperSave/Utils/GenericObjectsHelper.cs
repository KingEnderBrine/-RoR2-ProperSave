using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using ProperSave.Data;

namespace ProperSave.Utils
{
    internal static class GenericObjectsHelper
    {
        public static void ReadTypesAndObjects(ReaderContext context)
        {
            var reader = context.Reader;
            var types = context.Types = new TypeData[reader.ReadPackedInt32()];
            for (var i = 0; i < types.Length; i++)
            {
                types[i] = new TypeData
                {
                    TypeIndex = i
                };
            }
            for (var i = 0; i < types.Length; i++)
            {
                types[i].Read(context);
            }
            for (var i = 0; i < types.Length; i++)
            {
                var data = types[i];
                if (data.IsPrimitive)
                {
                    data.Type = GetPrimitiveType(data.ObjectType);
                    continue;
                }

                var typeName = data.Name.Build();
                data.Type = Type.GetType(typeName, false);
                if (data.Type is null)
                {
                    ProperSavePlugin.InstanceLogger.LogWarning($"Type {typeName} was not found");
                }
                else if (data.ObjectType == ObjectType.ValueType || data.ObjectType == ObjectType.Object || data.ObjectType == ObjectType.Class)
                {
                    for (var j = 0; j < data.Fields.Count; j++)
                    {
                        var field = data.Fields[j];
                        field.Field = data.Type.GetField(field.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                        if (field.Field == null)
                        {
                            field.Property = data.Type.GetProperty(field.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                            if (field.Property != null && !(field.Property.CanRead || field.Property.CanWrite))
                            {
                                field.Property = null;
                            }
                        }
                    }
                }
                else if (data.ObjectType == ObjectType.Collection)
                {
                    var interfaces = data.Type.GetInterfaces();
                    var collectionInterface = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
                    data.AddCollectionElementMethod = collectionInterface?.GetMethod("Add");
                }
                else if (data.ObjectType == ObjectType.KeyValuePair)
                {
                    data.Constructor = data.Type.GetConstructors()[0];
                }
            }

            var objects = context.Objects = new (object obj, TypeData type)[reader.ReadPackedInt32()];
            for (var i = 0; i < objects.Length; i++)
            {
                var typeData = types[reader.ReadPackedInt32()];
                var type = typeData.Type;
                if (typeData.ObjectType == ObjectType.Array)
                {
                    var lengths = new int[typeData.ArrayRank];
                    for (var j = 0; j < typeData.ArrayRank; j++)
                    {
                        lengths[j] = reader.ReadPackedInt32();
                    }

                    var elementType = typeData.CollectionArgumentType.Type;
                    if (type != null && elementType != null)
                    {
                        objects[i] = (Array.CreateInstance(elementType, lengths), typeData);
                    }
                    else
                    {
                        objects[i] = (null, typeData);
                    }
                }
                else if (type != null)
                {
                    if (typeData.IsCollection)
                    {
                        try
                        {
                            objects[i] = (Activator.CreateInstance(type), typeData);
                        }
                        catch (Exception ex)
                        {
                            ProperSavePlugin.InstanceLogger.LogWarning(ex);
                            objects[i] = (null, typeData);
                        }
                    }
                    else
                    {
                        objects[i] = (RuntimeHelpers.GetUninitializedObject(type), typeData);
                    }
                }
                else
                {
                    objects[i] = (null, typeData);
                }
            }
        }

        internal static void ReadObjectsData(ReaderContext context)
        {
            for (var i = 0; i < context.Objects.Length; i++)
            {
                var (obj, type) = context.Objects[i];
                ReadObject(obj, type, context);
            }
        }

        private static void ReadObject(object obj, TypeData type, ReaderContext context)
        {
            if (type.IsCollection)
            {
                ReadCollection(obj, type, context);
                return;
            }

            foreach (var field in type.Fields)
            {
                field.SetValue(obj, ReadValue(field.Type.ObjectType, context));
            }
        }

        private static object ReadValue(ObjectType objectType, ReaderContext context)
        {
            var reader = context.Reader;

            if (objectType == ObjectType.Object)
            {
                objectType = (ObjectType)reader.ReadByte();
            }

            switch (objectType)
            {
                case ObjectType.Sbyte:
                {
                    return reader.ReadSByte();
                }
                case ObjectType.Byte:
                {
                    return reader.ReadByte();
                }
                case ObjectType.Short:
                {
                    return reader.ReadPackedInt16();
                }
                case ObjectType.UShort:
                {
                    return reader.ReadPackedUInt16();
                }
                case ObjectType.Int:
                {
                    return reader.ReadPackedInt32();
                }
                case ObjectType.UInt:
                {
                    return reader.ReadPackedUInt32();
                }
                case ObjectType.Long:
                {
                    return reader.ReadPackedInt64();
                }
                case ObjectType.ULong:
                {
                    return reader.ReadPackedUInt64();
                }
                case ObjectType.Float:
                {
                    return reader.ReadSingle();
                }
                case ObjectType.Double:
                {
                    return reader.ReadDouble();
                }
                case ObjectType.Boolean:
                {
                    return reader.ReadBoolean();
                }
                case ObjectType.String:
                {
                    return context.SharedStrings[reader.ReadPackedInt32()];
                }
                case ObjectType.KeyValuePair:
                {
                    var typeIndex = reader.ReadPackedInt32();
                    var typeData = context.Types[typeIndex];
                    var args = ObjectBuffer.Buffers[2];
                    for (int i = 0; i < typeData.Fields.Count; i++)
                    {
                        var field = typeData.Fields[i];
                        args[i] = ReadValue(field.Type.ObjectType, context);
                    }

                    return typeData.CreateObject(args);
                }
                case ObjectType.ValueType:
                {
                    var typeIndex = reader.ReadPackedInt32();
                    var typeData = context.Types[typeIndex];
                    var obj = typeData.Type is null ? null : RuntimeHelpers.GetUninitializedObject(typeData.Type);
                    foreach (var field in typeData.Fields)
                    {
                        field.SetValue(obj, ReadValue(field.Type.ObjectType, context));
                    }

                    return obj;
                }
                case ObjectType.Array:
                case ObjectType.Collection:
                case ObjectType.Object:
                case ObjectType.Class:
                {
                    var index = reader.ReadPackedInt32();
                    if (index == -1)
                    {
                        return null;
                    }

                    return context.Objects[index].obj;
                }
            }

            throw new NotSupportedException();
        }

        private static void ReadCollection(object obj, TypeData type, ReaderContext context)
        {
            var reader = context.Reader;
            switch (type.ObjectType)
            {
                case ObjectType.Array:
                {
                    var list = obj as Array;
                    var fullLength = reader.ReadPackedInt32();
                    var itemTypeData = type.CollectionArgumentType;
                    if (list is null)
                    {
                        for (var i = 0; i < fullLength; i++)
                        {
                            ReadValue(itemTypeData.ObjectType, context);
                        }
                    }
                    else
                    {
                        var arrayRank = type.ArrayRank;
                        var lengths = new int[arrayRank];
                        var indices = new int[arrayRank];
                        for (var i = 0; i < arrayRank; i++)
                        {
                            lengths[i] = list.GetLength(i);
                        }

                        if (lengths.All(l => l > 0))
                        {
                            do
                            {
                                list.SetValue(ReadValue(itemTypeData.ObjectType, context), indices);
                            }
                            while (IncrementIndices(indices, lengths));
                        }
                    }
                    break;
                }
                case ObjectType.Collection:
                {
                    var itemTypeData = type.CollectionArgumentType;
                    var count = reader.ReadPackedInt32();
                    for (var i = 0; i < count; i++)
                    {
                        var value = ReadValue(itemTypeData.ObjectType, context);
                        type.AddCollectionElement(obj, value);
                    }
                    break;
                }
            }
        }

        private static bool IncrementIndices(int[] indices, int[] lengths)
        {
            for (var i = lengths.Length - 1; i >= 0; i--)
            {
                var index = ++indices[i];
                if (index == lengths[i])
                {
                    indices[i] = 0;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }


        private static Type GetPrimitiveType(ObjectType objectType)
        {
            return objectType switch
            {
                ObjectType.Sbyte => typeof(sbyte),
                ObjectType.Byte => typeof(byte),
                ObjectType.Short => typeof(short),
                ObjectType.UShort => typeof(ushort),
                ObjectType.Int => typeof(int),
                ObjectType.UInt => typeof(uint),
                ObjectType.Long => typeof(long),
                ObjectType.ULong => typeof(ulong),
                ObjectType.Float => typeof(float),
                ObjectType.Double => typeof(double),
                ObjectType.Boolean => typeof(bool),
                ObjectType.String => typeof(string),
                _ => throw new NotSupportedException(),
            };
        }

        internal static void WriteObjectsData(WriterContext context)
        {
            while (context.ObjectsQueue.Count > 0)
            {
                WriteObject(context.ObjectsQueue.Dequeue(), context);
            }
        }

        internal static void WriteTypesAndObjects(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(context.Types.Count);
            foreach (var (_, data) in context.Types.OrderBy(kvp => kvp.Value.TypeIndex))
            {
                data.Write(context);
            }

            writer.WritePacked(context.Objects.Count);
            foreach (var (obj, (_, typeData)) in context.Objects.OrderBy(kvp => kvp.Value.index))
            {
                writer.WritePacked(typeData.TypeIndex);
                if (typeData.ObjectType == ObjectType.Array)
                {
                    for (var i = 0; i < typeData.ArrayRank; i++)
                    {
                        writer.WritePacked(((Array)obj).GetLength(i));
                    }
                }
            }
        }

        private static TypeData GetTypeData(Type type, WriterContext context)
        {
            if (!context.Types.TryGetValue(type, out var data))
            {
                context.Types[type] = data = new TypeData
                {
                    Type = type,
                    Name = TypeName.FromType(type),
                    TypeIndex = context.Types.Count,
                };

                if (type == typeof(sbyte))
                {
                    data.ObjectType = ObjectType.Sbyte;
                }
                else if (type == typeof(byte))
                {
                    data.ObjectType = ObjectType.Byte;
                }
                else if (type == typeof(short))
                {
                    data.ObjectType = ObjectType.Short;
                }
                else if (type == typeof(ushort))
                {
                    data.ObjectType = ObjectType.UShort;
                }
                else if (type == typeof(int))
                {
                    data.ObjectType = ObjectType.Int;
                }
                else if (type == typeof(uint))
                {
                    data.ObjectType = ObjectType.UInt;
                }
                else if (type == typeof(long))
                {
                    data.ObjectType = ObjectType.Long;
                }
                else if (type == typeof(ulong))
                {
                    data.ObjectType = ObjectType.ULong;
                }
                else if (type == typeof(float))
                {
                    data.ObjectType = ObjectType.Float;
                }
                else if (type == typeof(double))
                {
                    data.ObjectType = ObjectType.Double;
                }
                else if (type == typeof(bool))
                {
                    data.ObjectType = ObjectType.Boolean;
                }
                else if (type == typeof(string))
                {
                    data.ObjectType = ObjectType.String;
                }
                else if (type == typeof(object))
                {
                    data.ObjectType = ObjectType.Object;
                    data.Fields = new List<FieldData>();
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    data.ObjectType = ObjectType.KeyValuePair;
                    data.Fields = new List<FieldData>();
                    var keyProperty = type.GetProperty("Key");
                    data.Fields.Add(new FieldData { Name = keyProperty.Name, Property = keyProperty, Type = GetTypeData(keyProperty.PropertyType, context) });
                    var valueProperty = type.GetProperty("Value");
                    data.Fields.Add(new FieldData { Name = valueProperty.Name, Property = valueProperty, Type = GetTypeData(valueProperty.PropertyType, context) });
                }
                else if (type.IsArray)
                {
                    var elementType = GetTypeData(type.GetElementType(), context);
                    data.ArrayRank = type.GetArrayRank();
                    data.CollectionArgumentType = elementType;
                    data.ObjectType = ObjectType.Array;
                }
                else
                {
                    var interfaces = type.GetInterfaces();
                    var collectionInterface = interfaces.FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>));
                    if (collectionInterface != null)
                    {
                        var elementType = GetTypeData(collectionInterface.GetGenericArguments()[0], context);
                        data.CollectionArgumentType = elementType;
                        data.CollectionCountProperty = collectionInterface.GetProperty("Count");
                        data.ObjectType = ObjectType.Collection;
                    }
                    else
                    {
                        if (type.IsValueType)
                        {
                            data.ObjectType = ObjectType.ValueType;
                        }
                        else
                        {
                            data.ObjectType = ObjectType.Class;
                        }

                        var fields = type
                            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                            .Where(i => !i.IsDefined(typeof(IgnoreDataMemberAttribute), true));
                        var properties = type
                            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
                            .Where(i => i.CanRead && i.CanWrite && !i.IsDefined(typeof(IgnoreDataMemberAttribute), true));
                        data.Fields = new List<FieldData>();
                        data.Fields.AddRange(fields.Select(f =>
                            new FieldData { Name = f.Name, Field = f, Type = GetTypeData(f.FieldType, context) }
                        ));
                        data.Fields.AddRange(properties.Select(p =>
                            new FieldData { Name = p.Name, Property = p, Type = GetTypeData(p.PropertyType, context) }
                        ));
                    }
                }
            }

            return data;
        }

        internal static int GetReferenceIndex(object data, WriterContext context)
        {
            if (data is null)
            {
                return -1;
            }

            if (!context.Objects.TryGetValue(data, out var objKvp))
            {
                var typeData = GetTypeData(data.GetType(), context);
                context.Objects[data] = objKvp = (context.Objects.Count, typeData);
                context.ObjectsQueue.Enqueue(data);
            }

            return objKvp.index;
        }

        private static void WriteObject(object obj, WriterContext context)
        {
            if (obj is null)
            {
                throw new NotSupportedException("Null written as object");
            }

            var data = GetTypeData(obj.GetType(), context);

            if (data.IsCollection)
            {
                WriteCollection(obj, data, context);
                return;
            }

            foreach (var field in data.Fields)
            {
                WriteValue(field.GetValue(obj), field.Type, context);
            }
        }

        private static void WriteValue(object value, TypeData type, WriterContext context)
        {
            var writer = context.Writer;

            if (type.ObjectType == ObjectType.Object)
            {
                type = GetTypeData(value?.GetType() ?? typeof(object), context);
                writer.Write((byte)type.ObjectType);
            }

            switch (type.ObjectType)
            {
                case ObjectType.Sbyte:
                {
                    writer.Write((sbyte)value);
                    break;
                }
                case ObjectType.Byte:
                {
                    writer.Write((byte)value);
                    break;
                }
                case ObjectType.Short:
                {
                    writer.WritePacked((short)value);
                    break;
                }
                case ObjectType.UShort:
                {
                    writer.WritePacked((ushort)value);
                    break;
                }
                case ObjectType.Int:
                {
                    writer.WritePacked((int)value);
                    break;
                }
                case ObjectType.UInt:
                {
                    writer.WritePacked((uint)value);
                    break;
                }
                case ObjectType.Long:
                {
                    writer.WritePacked((long)value);
                    break;
                }
                case ObjectType.ULong:
                {
                    writer.WritePacked((ulong)value);
                    break;
                }
                case ObjectType.Float:
                {
                    writer.Write((float)value);
                    break;
                }
                case ObjectType.Double:
                {
                    writer.Write((double)value);
                    break;
                }
                case ObjectType.Boolean:
                {
                    writer.Write((bool)value);
                    break;
                }
                case ObjectType.String:
                {
                    writer.WritePacked(context.SharedStrings.AddOrIndexOf((string)value));
                    break;
                }
                case ObjectType.KeyValuePair:
                case ObjectType.ValueType:
                {
                    writer.WritePacked(type.TypeIndex);
                    foreach (var field in type.Fields)
                    {
                        var fieldValue = field.GetValue(value);
                        WriteValue(fieldValue, field.Type, context);
                    }
                    break;
                }
                case ObjectType.Array:
                case ObjectType.Collection:
                case ObjectType.Object:
                case ObjectType.Class:
                {
                    writer.WritePacked(GetReferenceIndex(value, context));
                    break;
                }
            }
        }

        private static void WriteCollection(object value, TypeData type, WriterContext context)
        {
            var writer = context.Writer;

            switch (type.ObjectType)
            {
                case ObjectType.Array:
                {
                    var list = (Array)value;
                    writer.WritePacked(list.Length);
                    var itemTypeData = type.CollectionArgumentType;
                    foreach (var item in list)
                    {
                        WriteValue(item, itemTypeData, context);
                    }
                    break;
                }
                case ObjectType.Collection:
                {
                    var list = (IEnumerable)value;
                    var itemTypeData = type.CollectionArgumentType;
                    writer.WritePacked(type.CollectionCount(value));
                    foreach (var item in list)
                    {
                        WriteValue(item, itemTypeData, context);
                    }
                    break;
                }
            }
        }
    }
}
