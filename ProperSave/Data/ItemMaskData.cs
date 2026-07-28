using ProperSave.Utils;
using RoR2;
using System.Collections.Generic;

namespace ProperSave.Data
{
    public class ItemMaskData
    {
        public List<ItemIndex> enabledItems = new List<ItemIndex>();

        public static ItemMaskData Create(ItemMask mask)
        {
            var data = new ItemMaskData();
            for (var i = 0; i < mask.array.Length; i++)
            {
                if (!mask.array[i])
                {
                    continue;
                }

                data.enabledItems.Add((ItemIndex)i);
            }

            return data;
        }

        public void LoadData(ItemMask mask)
        {
            for (var i = 0; i < mask.array.Length; i++)
            {
                mask.array[i] = false;
            }

            foreach (var enabledItem in enabledItems)
            {
                if (enabledItem == ItemIndex.None)
                {
                    continue;
                }

                mask.array[(int)enabledItem] = true;
            }
        }

        internal static ItemMaskData Read(ReaderContext context)
        {
            var data = new ItemMaskData();
            var reader = context.Reader;
            var version = context.Version;

            var enabledItemsCount = version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32();
            data.enabledItems = new List<ItemIndex>(enabledItemsCount);
            for (var i = 0; i < enabledItemsCount; i++)
            {
                data.enabledItems.Add(SharedIndexHelpers.ResolveItem(version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32(), context));
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(enabledItems.Count);
            for (var i = 0; i < enabledItems.Count; i++)
            {
                writer.WritePacked(SharedIndexHelpers.FromItem(enabledItems[i], context));
            }
        }
    }
}
