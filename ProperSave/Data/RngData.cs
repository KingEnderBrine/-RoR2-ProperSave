using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class RngData
    {
        [DataMember(Name = "s0")]
        public ulong state0;
        [DataMember(Name = "s1")]
        public ulong state1;

        public RngData(Xoroshiro128Plus rng)
        {
            state0 = rng.state0;
            state1 = rng.state1;
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
    }
}
