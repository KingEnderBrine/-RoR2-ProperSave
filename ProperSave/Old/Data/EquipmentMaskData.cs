using RoR2;
using System.Linq;

namespace ProperSave.Old.Data
{
    public class EquipmentMaskData
    {
        public bool[] array;

        internal ProperSave.Data.EquipmentMaskData Migrate()
        {
            return new ProperSave.Data.EquipmentMaskData
            {
                enabledItems = array
                .Select((e, i) => e ? (EquipmentIndex)i : EquipmentIndex.None)
                .Where(i => i != EquipmentIndex.None)
                .ToList()
            };
        }
    }
}
