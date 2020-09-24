using RoR2;
using System.Linq;

namespace ProperSave.Data
{
    public class ItemMaskData
    {
        public bool[] array;

        public ItemMaskData(ItemMask mask)
        {
            array = mask.array.ToArray();
        }

        public void LoadData(out ItemMask mask)
        {
            mask = new ItemMask() { array = array.ToArray() };
        }
    }
}
