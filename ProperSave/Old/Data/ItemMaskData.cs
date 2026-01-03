using RoR2;
using System.Linq;

namespace ProperSave.Old.Data
{
    public class ItemMaskData
    {
        public bool[] array;

        internal ProperSave.Data.ItemMaskData Migrate()
        {
            return new ProperSave.Data.ItemMaskData
            {
                enabledItems = array
                .Select((e, i) => e ? (ItemIndex)i : ItemIndex.None)
                .Where(i => i != ItemIndex.None)
                .ToList()
            };
        }
    }
}
