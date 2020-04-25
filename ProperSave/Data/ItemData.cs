using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class ItemData
    {
        [DataMember(Name = "i")]
        public int itemIndex;
        [DataMember(Name = "c")]
        public int count;

        public ItemData() { }
        public ItemData(int itemIndex, int count)
        {
            this.itemIndex = itemIndex;
            this.count = count;
        }
    }
}
