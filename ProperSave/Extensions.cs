using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace ProperSave
{
    public static class Extensions
    {
        public static int DifferenceCount<T>(this IEnumerable<T> collection, IEnumerable<T> second)
        {
            var secondCopy = second.ToList();
            var count = 0;
            foreach (var element in collection)
            {
                if (!secondCopy.Remove(element))
                {
                    count++;
                }
            }
            count += secondCopy.Count;

            return count;
        }

        public static string GetSafe(this string[] list, int index)
        {
            if ((uint)index > list.Length)
            {
                return null;
            }

            return list[index];
        }

        public static int AddOrIndexOf(this List<string> list, string value)
        {
            if (value is null)
            {
                return -1;
            }

            var index = list.IndexOf(value);
            if (index < 0)
            {
                list.Add(value);
                return list.Count - 1;
            }

            return index;
        }

        public static T GetSafe<T>(this List<T> list, int index)
        {
            if (list is null || list.Count < (uint)index)
            {
                return default;
            }

            return list[index];
        }

        public static void WritePacked(this BinaryWriter writer, short value)
        {
            WritePackedSigned(writer, (Union)value);
        }

        public static void WritePacked(this BinaryWriter writer, ushort value)
        {
            WritePacked(writer, (Union)value);
        }

        public static void WritePacked(this BinaryWriter writer, int value)
        {
            WritePackedSigned(writer, (Union)value);
        }

        public static void WritePacked(this BinaryWriter writer, uint value)
        {
            WritePacked(writer, (Union)value);
        }

        public static void WritePacked(this BinaryWriter writer, long value)
        {
            WritePackedSigned(writer, (Union)value);
        }

        public static void WritePacked(this BinaryWriter writer, ulong value)
        {
            WritePacked(writer, (Union)value);
        }

        private static void WritePackedSigned(BinaryWriter writer, Union union)
        {
            var sign = union.negative ? 0UL : 0b10000000;
            var value = union.@ulong;
            if (value <= 112UL)
            {
                writer.Write((byte)(value | sign));
            }
            else if (value <= 2047UL + 112UL)
            {
                writer.Write((byte)(((value - 112UL) / 256UL + 113UL) | sign));
                writer.Write((byte)((value - 112UL) % 256UL));
            }
            else if (value <= 65535UL + 2047UL + 112UL)
            {
                writer.Write((byte)(121 | sign));
                writer.Write((byte)((value - 2160UL) / 256UL));
                writer.Write((byte)((value - 2160UL) % 256UL));
            }
            else if (value <= 16777215UL)
            {
                writer.Write((byte)(122 | sign));
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
            }
            else if (value <= (ulong)uint.MaxValue)
            {
                writer.Write((byte)(123 | sign));
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 24 & (ulong)byte.MaxValue));
            }
            else if (value <= 1099511627775UL)
            {
                writer.Write((byte)(124 | sign));
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 24 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 32 & (ulong)byte.MaxValue));
            }
            else if (value <= 281474976710655UL)
            {
                writer.Write((byte)(125 | sign));
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 24 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 32 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 40 & (ulong)byte.MaxValue));
            }
            else if (value <= 72057594037927935UL)
            {
                writer.Write((byte)(126 | sign));
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 24 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 32 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 40 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 48 & (ulong)byte.MaxValue));
            }
            else
            {
                writer.Write((byte)(127 | sign));
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 24 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 32 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 40 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 48 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 56 & (ulong)byte.MaxValue));
            }
        }

        private static void WritePacked(BinaryWriter writer, Union union)
        {
            var value = union.@ulong;
            if (value <= 240UL)
            {
                writer.Write((byte)value);
            }
            else if (value <= 2287UL)
            {
                writer.Write((byte)((value - 240UL) / 256UL + 241UL));
                writer.Write((byte)((value - 240UL) % 256UL));
            }
            else if (value <= 67823UL)
            {
                writer.Write((byte)249);
                writer.Write((byte)((value - 2288UL) / 256UL));
                writer.Write((byte)((value - 2288UL) % 256UL));
            }
            else if (value <= 16777215UL)
            {
                writer.Write((byte)250);
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
            }
            else if (value <= (ulong)uint.MaxValue)
            {
                writer.Write((byte)251);
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 24 & (ulong)byte.MaxValue));
            }
            else if (value <= 1099511627775UL)
            {
                writer.Write((byte)252);
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 24 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 32 & (ulong)byte.MaxValue));
            }
            else if (value <= 281474976710655UL)
            {
                writer.Write((byte)253);
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 24 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 32 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 40 & (ulong)byte.MaxValue));
            }
            else if (value <= 72057594037927935UL)
            {
                writer.Write((byte)254);
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 24 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 32 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 40 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 48 & (ulong)byte.MaxValue));
            }
            else
            {
                writer.Write((byte)255);
                writer.Write((byte)(value & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 8 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 16 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 24 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 32 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 40 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 48 & (ulong)byte.MaxValue));
                writer.Write((byte)(value >> 56 & (ulong)byte.MaxValue));
            }
        }

        public static short ReadPackedInt16(this BinaryReader reader)
        {
            return ReadPackedSigned(reader).@short;
        }

        public static ushort ReadPackedUInt16(this BinaryReader reader)
        {
            return ReadPacked(reader).@ushort;
        }

        public static int ReadPackedInt32(this BinaryReader reader)
        {
            return ReadPackedSigned(reader).@int;
        }

        public static uint ReadPackedUInt32(this BinaryReader reader)
        {
            return ReadPacked(reader).@uint;
        }

        public static long ReadPackedInt64(this BinaryReader reader)
        {
            return ReadPackedSigned(reader).@long;
        }

        public static ulong ReadPackedUInt64(this BinaryReader reader)
        {
            return ReadPacked(reader).@ulong;
        }

        private static Union ReadPacked(BinaryReader reader)
        {
            byte num1 = reader.ReadByte();
            if (num1 < (byte)241)
                return (ulong)num1;
            byte num2 = reader.ReadByte();
            if (num1 >= (byte)241 && num1 <= (byte)248)
                return (ulong)(240L + 256L * ((long)num1 - 241L)) + (ulong)num2;
            byte num3 = reader.ReadByte();
            if (num1 == (byte)249)
                return (ulong)(2288L + 256L * (long)num2) + (ulong)num3;
            byte num4 = reader.ReadByte();
            if (num1 == (byte)250)
                return (ulong)((long)num2 + ((long)num3 << 8) + ((long)num4 << 16));
            byte num5 = reader.ReadByte();
            if (num1 == (byte)251)
                return (ulong)((long)num2 + ((long)num3 << 8) + ((long)num4 << 16) + ((long)num5 << 24));
            byte num6 = reader.ReadByte();
            if (num1 == (byte)252)
                return (ulong)((long)num2 + ((long)num3 << 8) + ((long)num4 << 16) + ((long)num5 << 24) + ((long)num6 << 32));
            byte num7 = reader.ReadByte();
            if (num1 == (byte)253)
                return (ulong)((long)num2 + ((long)num3 << 8) + ((long)num4 << 16) + ((long)num5 << 24) + ((long)num6 << 32) + ((long)num7 << 40));
            byte num8 = reader.ReadByte();
            if (num1 == (byte)254)
                return (ulong)((long)num2 + ((long)num3 << 8) + ((long)num4 << 16) + ((long)num5 << 24) + ((long)num6 << 32) + ((long)num7 << 40) + ((long)num8 << 48));
            byte num9 = reader.ReadByte();
            if (num1 == byte.MaxValue)
                return (ulong)((long)num2 + ((long)num3 << 8) + ((long)num4 << 16) + ((long)num5 << 24) + ((long)num6 << 32) + ((long)num7 << 40) + ((long)num8 << 48) + ((long)num9 << 56));
            throw new IndexOutOfRangeException("ReadPacked() failure: " + num1.ToString());
        }

        private static Union ReadPackedSigned(BinaryReader reader)
        {
            byte num1 = reader.ReadByte();
            var negative = (num1 & 0b10000000) == 0;
            num1 &= 0b01111111;
            if (num1 <= 112)
                return new Union(num1, negative);
            byte num2 = reader.ReadByte();
            if (num1 >= 113 && num1 <= 120)
                return new Union(112UL + 256UL * (num1 - 113UL) + num2, negative);
            byte num3 = reader.ReadByte();
            if (num1 == 121)
                return new Union(2160UL + 256UL * num2 + num3, negative);
            byte num4 = reader.ReadByte();
            if (num1 == 122)
                return new Union(num2 + ((ulong)num3 << 8) + ((ulong)num4 << 16), negative);
            byte num5 = reader.ReadByte();
            if (num1 == 123)
                return new Union(num2 + ((ulong)num3 << 8) + ((ulong)num4 << 16) + ((ulong)num5 << 24), negative);
            byte num6 = reader.ReadByte();
            if (num1 == 124)
                return new Union(num2 + ((ulong)num3 << 8) + ((ulong)num4 << 16) + ((ulong)num5 << 24) + ((ulong)num6 << 32), negative);
            byte num7 = reader.ReadByte();
            if (num1 == 125)
                return new Union(num2 + ((ulong)num3 << 8) + ((ulong)num4 << 16) + ((ulong)num5 << 24) + ((ulong)num6 << 32) + ((ulong)num7 << 40), negative);
            byte num8 = reader.ReadByte();
            if (num1 == 126)
                return new Union(num2 + ((ulong)num3 << 8) + ((ulong)num4 << 16) + ((ulong)num5 << 24) + ((ulong)num6 << 32) + ((ulong)num7 << 40) + ((ulong)num8 << 48), negative);
            byte num9 = reader.ReadByte();
            if (num1 == 127)
                return new Union(num2 + ((ulong)num3 << 8) + ((ulong)num4 << 16) + ((ulong)num5 << 24) + ((ulong)num6 << 32) + ((ulong)num7 << 40) + ((ulong)num8 << 48) + ((ulong)num9 << 56), negative);
            throw new IndexOutOfRangeException("ReadPackedSigned() failure: " + num1.ToString());
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct Union
        {
            [FieldOffset(0)]
            public ushort @ushort;
            [FieldOffset(0)]
            public uint @uint;
            [FieldOffset(0)]
            public ulong @ulong;
            [FieldOffset(8)]
            public bool negative;

            public readonly short @short => negative ? (short)-@ushort : (short)@ushort;
            public readonly int @int => negative ? -(int)@uint : (int)@uint;
            public readonly long @long => negative ? -(long)@ulong : (long)@ulong;

            public Union(ulong value, bool negative)
            {
                @ushort = 0;
                @uint = 0;
                @ulong = value;
                this.negative = negative;
            }

            public static implicit operator Union(short value) => new Union { @ushort = (ushort)Math.Abs(value), negative = value < 0 };
            public static implicit operator Union(ushort value) => new Union { @ushort = value };
            public static implicit operator Union(int value) => new Union { @uint = (uint)Math.Abs(value), negative = value < 0 };
            public static implicit operator Union(uint value) => new Union { @uint = value };
            public static implicit operator Union(long value) => new Union { @ulong = (ulong)Math.Abs(value), negative = value < 0 };
            public static implicit operator Union(ulong value) => new Union { @ulong = value };
        }
    }
}
