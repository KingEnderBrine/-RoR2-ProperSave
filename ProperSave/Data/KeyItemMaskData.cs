using System.Collections.Generic;
using ProperSave.Utils;
using RoR2;

namespace ProperSave.Data
{
    public class KeyItemMaskData
    {
        public List<KeyItemIndex> enabledItems = new List<KeyItemIndex>();

        public static KeyItemMaskData Create(KeyItemMask mask)
        {
            var data = new KeyItemMaskData();

            for (var i = 0; i < mask.array.Length; i++)
            {
                if (!mask.array[i])
                {
                    continue;
                }

                data.enabledItems.Add((KeyItemIndex)i);
            }

            return data;
        }

        public void LoadData(KeyItemMask mask)
        {
            for (var i = 0; i < mask.array.Length; i++)
            {
                mask.array[i] = false;
            }

            foreach (var enabledItem in enabledItems)
            {
                if (enabledItem == KeyItemIndex.None)
                {
                    continue;
                }

                mask.array[(int)enabledItem] = true;
            }
        }

        internal static KeyItemMaskData Read(ReaderContext context)
        {
            var data = new KeyItemMaskData();
            var reader = context.Reader;
            var version = context.Version;

            if (version > 2)
            {
                var enabledItemsCount = reader.ReadPackedInt32();
                data.enabledItems = new List<KeyItemIndex>(enabledItemsCount);
                for (var i = 0; i < enabledItemsCount; i++)
                {
                    data.enabledItems.Add(SharedIndexHelpers.ResolveKeyItem(reader.ReadPackedInt32(), context));
                }
            }
            else
            {
                data.enabledItems = new List<KeyItemIndex>();
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(enabledItems.Count);
            for (var i = 0; i < enabledItems.Count; i++)
            {
                writer.WritePacked(SharedIndexHelpers.FromKeyItem(enabledItems[i], context));
            }
        }
    }
}
