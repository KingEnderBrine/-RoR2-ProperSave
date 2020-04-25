using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProperSave.Data
{
    public class ItemMaskData
    {
        public ulong a;
        public ulong b;

        public ItemMaskData(ItemMask mask)
        {
            a = mask.a;
            b = mask.b;
        }

        public void LoadData(out ItemMask mask)
        {
            mask = new ItemMask() { a = a, b = b };
        }
    }
}
