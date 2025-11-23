using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class ItemData
    {
        [DataMember(Name = "i")]
        public int itemIndex;
        [DataMember(Name = "c")]
        public int count;
        [DataMember(Name = "cc")]
        public int channeledCount;
        [DataMember(Name = "tc")]
        public int tempCount;
        [DataMember(Name = "tft")]
        public float tempFixedTime;
    }
}
