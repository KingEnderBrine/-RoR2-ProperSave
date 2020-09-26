using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class ItemData
    {
        [DataMember(Name = "i")]
        public int itemIndex;
        [DataMember(Name = "c")]
        public int count;
    }
}
