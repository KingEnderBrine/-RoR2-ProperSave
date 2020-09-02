using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
