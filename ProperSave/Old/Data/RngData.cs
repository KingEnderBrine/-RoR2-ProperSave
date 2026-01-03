using System.Runtime.Serialization;

namespace ProperSave.Old.Data
{
    public class RngData
    {
        [DataMember(Name = "s0")]
        public ulong state0;
        [DataMember(Name = "s1")]
        public ulong state1;

        internal ProperSave.Data.RngData Migrate()
        {
            return new ProperSave.Data.RngData
            {
                state0 = state0,
                state1 = state1,
            };
        }
    }
}
