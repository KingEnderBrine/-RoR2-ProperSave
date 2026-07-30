using System;
using System.Text;
using System.Threading;
using ProperSave.Utils;

namespace ProperSave.Data
{
    public struct TypeName
    {
        public string Namespace;
        public string Name;
        public string AssemblyName;
        public TypeName[] GenereicArguments;

        public readonly void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(context.SharedStrings.AddOrIndexOf(Namespace));
            writer.WritePacked(context.SharedStrings.AddOrIndexOf(Name));
            writer.WritePacked(context.SharedStrings.AddOrIndexOf(AssemblyName));
            writer.WritePacked(GenereicArguments.Length);
            for (var i = 0; i < GenereicArguments.Length; i++)
            {
                GenereicArguments[i].Write(context);
            }
        }

        public static TypeName Read(ReaderContext context)
        {
            var data = new TypeName();
            var reader = context.Reader;

            data.Namespace = context.SharedStrings.GetSafe(reader.ReadPackedInt32());
            data.Name = context.SharedStrings.GetSafe(reader.ReadPackedInt32());
            data.AssemblyName = context.SharedStrings.GetSafe(reader.ReadPackedInt32());
            var argumentsCount = reader.ReadPackedInt32();
            data.GenereicArguments = new TypeName[argumentsCount];
            for (var i = 0; i < argumentsCount; i++)
            {
                data.GenereicArguments[i] = Read(context);
            }

            return data;
        }

        internal static TypeName FromType(Type type)
        {
            var data = new TypeName();
            var builder = new StringBuilder();
            GetNamespace(type, builder);
            data.Namespace = builder.ToString();
            data.Name = type.Name;

            if (type.IsGenericType)
            {
                var args = type.GenericTypeArguments;
                data.GenereicArguments = new TypeName[args.Length];
                for (var i = 0; i < args.Length; i++)
                {
                    data.GenereicArguments[i] = FromType(args[i]);
                }
            }
            else
            {
                data.GenereicArguments = Array.Empty<TypeName>();
            }

            var assemblyName = type.Assembly.FullName;
            data.AssemblyName = assemblyName.Substring(0, assemblyName.IndexOf(','));

            return data;
        }

        internal readonly string Build()
        {
            var builder = new StringBuilder();
            Build(builder);
            return builder.ToString();
        }

        private readonly void Build(StringBuilder builder)
        {
            builder.Append(Namespace);
            builder.Append(Name);

            if (GenereicArguments.Length > 0)
            {
                builder.Append('[');
                for (var i = 0; i < GenereicArguments.Length; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(',');
                    }

                    builder.Append('[');
                    GenereicArguments[i].Build(builder);
                    builder.Append(']');
                }
                builder.Append(']');
            }

            builder.Append(", ").Append(AssemblyName);
        }

        private static void GetNamespace(Type type, StringBuilder builder)
        {
            if (type.DeclaringType is null)
            {
                if (!string.IsNullOrEmpty(type.Namespace))
                {
                    builder.Append(type.Namespace).Append('.');
                }
            }
            else
            {
                GetNamespace(type.DeclaringType, builder);
                builder.Append(type.DeclaringType.Name);
                builder.Append('+');
            }
        }
    }
}
