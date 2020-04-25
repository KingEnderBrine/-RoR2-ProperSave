using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProperSave.Data
{
    public class EquipmentMaskData
    {
        public ulong a;

        public EquipmentMaskData(EquipmentMask mask)
        {
            a = mask.a;
        }

        public void LoadData(out EquipmentMask mask)
        {
            mask = new EquipmentMask() { a = a };
        }
    }
}
