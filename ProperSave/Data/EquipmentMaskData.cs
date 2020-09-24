using RoR2;
using System.Linq;

namespace ProperSave.Data
{
    public class EquipmentMaskData
    {
        public bool[] array;

        public EquipmentMaskData(EquipmentMask mask)
        {
            array = mask.array.ToArray();
        }

        public void LoadData(out EquipmentMask mask)
        {
            mask = new EquipmentMask() { array = array.ToArray() };
        }
    }
}
