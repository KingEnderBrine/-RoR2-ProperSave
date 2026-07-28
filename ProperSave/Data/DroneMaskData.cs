using System.Collections.Generic;
using ProperSave.Utils;
using RoR2;

namespace ProperSave.Data
{
    public class DroneMaskData
    {
        public List<DroneIndex> enabledItems = new List<DroneIndex>();

        public static DroneMaskData Create(DroneMask mask)
        {
            var data = new DroneMaskData();

            for (var i = 0; i < mask.array.Length; i++)
            {
                if (!mask.array[i])
                {
                    continue;
                }

                data.enabledItems.Add((DroneIndex)i);
            }

            return data;
        }

        public void LoadData(DroneMask mask)
        {
            for (var i = 0; i < mask.array.Length; i++)
            {
                mask.array[i] = false;
            }

            foreach (var enabledItem in enabledItems)
            {
                if (enabledItem == DroneIndex.None)
                {
                    continue;
                }

                mask.array[(int)enabledItem] = true;
            }
        }

        internal static DroneMaskData Read(ReaderContext context)
        {
            var data = new DroneMaskData();
            var reader = context.Reader;
            var version = context.Version;

            var enabledItemsCount = version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32();
            data.enabledItems = new List<DroneIndex>(enabledItemsCount);
            for (var i = 0; i < enabledItemsCount; i++)
            {
                data.enabledItems.Add(SharedIndexHelpers.ResolveDrone(version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32(), context));
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(enabledItems.Count);
            for (var i = 0; i < enabledItems.Count; i++)
            {
                writer.WritePacked(SharedIndexHelpers.FromDrone(enabledItems[i], context));
            }
        }
    }
}
