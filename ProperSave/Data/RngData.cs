using System.Runtime.Serialization;
using ProperSave.Utils;

namespace ProperSave.Data
{
    public class RngData
    {
        public ulong state0;
        public ulong state1;

        public static RngData Create(Xoroshiro128Plus rng)
        {
            return new RngData
            {
                state0 = rng.state0,
                state1 = rng.state1,
            };
        }

        public void LoadDataOut(out Xoroshiro128Plus rng)
        {
            rng = FormatterServices.GetUninitializedObject(typeof(Xoroshiro128Plus)) as Xoroshiro128Plus;
            LoadDataRef(ref rng);
        }

        public void LoadDataRef(ref Xoroshiro128Plus rng)
        {
            rng.state0 = state0;
            rng.state1 = state1;
        }

        internal static RngData Read(ReaderContext context)
        {
            var data = new RngData();
            data.state0 = context.Reader.ReadUInt64();
            data.state1 = context.Reader.ReadUInt64();

            return data;
        }

        internal void Write(WriterContext context)
        {
            context.Writer.Write(state0);
            context.Writer.Write(state1);
        }
    }
}
