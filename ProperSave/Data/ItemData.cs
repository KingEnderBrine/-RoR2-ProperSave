using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class ItemData
    {
        [DataMember(Name = "i")]
        public int itemIndex;
        [DataMember(Name = "cp")]
        public int countPerm;
        [DataMember(Name = "tr")]
        public float countTemp;

    }
}
