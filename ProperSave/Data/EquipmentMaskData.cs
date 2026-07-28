using ProperSave.Utils;
using RoR2;
using System.Collections.Generic;

namespace ProperSave.Data
{
    public class EquipmentMaskData
    {
        public List<EquipmentIndex> enabledItems = new List<EquipmentIndex>();

        public static EquipmentMaskData Create(EquipmentMask mask)
        {
            var data = new EquipmentMaskData();
            for (var i = 0; i < mask.array.Length; i++)
            {
                if (!mask.array[i])
                {
                    continue;
                }

                data.enabledItems.Add((EquipmentIndex)i);
            }

            return data;
        }

        public void LoadData(EquipmentMask mask)
        {
            for (var i = 0; i < mask.array.Length; i++)
            {
                mask.array[i] = false;
            }

            foreach (var enabledItem in enabledItems)
            {
                if (enabledItem == EquipmentIndex.None)
                {
                    continue;
                }

                mask.array[(int)enabledItem] = true;
            }
        }

        internal static EquipmentMaskData Read(ReaderContext context)
        {
            var data = new EquipmentMaskData();
            var reader = context.Reader;
            var version = context.Version;

            var enabledItemsCount = version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32();
            data.enabledItems = new List<EquipmentIndex>(enabledItemsCount);
            for (var i = 0; i < enabledItemsCount; i++)
            {
                data.enabledItems.Add(SharedIndexHelpers.ResolveEquipment(version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32(), context));
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(enabledItems.Count);
            for (var i = 0; i < enabledItems.Count; i++)
            {
                writer.WritePacked(SharedIndexHelpers.FromEquipment(enabledItems[i], context));
            }
        }
    }
}
