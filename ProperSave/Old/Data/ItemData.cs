using System.Runtime.Serialization;
using RoR2;

namespace ProperSave.Old.Data
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

        internal ProperSave.Data.ItemData Migrate()
        {
            return new ProperSave.Data.ItemData
            {
                channeledCount = channeledCount,
                tempCount = tempCount,
                tempFixedTime = tempFixedTime,
                count = count,
                itemIndex = (ItemIndex)itemIndex,
            };
        }
    }
}
