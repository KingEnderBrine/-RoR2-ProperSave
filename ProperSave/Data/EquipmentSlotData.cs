using System;
using System.Collections.Generic;
using System.Text;
using ProperSave.Utils;
using RoR2;

namespace ProperSave.Data
{
    public class EquipmentSlotData
    {
        public EquipmentStateData[] sets;
        public byte activeSetIndex;

        public static EquipmentSlotData Create(Inventory inventory, int slotIndex)
        {
            var data = new EquipmentSlotData();
            data.sets = new EquipmentStateData[inventory._equipmentSlots[slotIndex]._setCount];
            for (var j = 0; j < data.sets.Length; j++)
            {
                data.sets[j] = EquipmentStateData.Create(inventory.GetEquipment(new EquipmentLocation { slot = (uint)slotIndex, set = (uint)j }));
            }

            return data;
        }

        public void LoadSlot(Inventory inventory, uint i)
        {
            for (uint j = 0; j < sets.Length; j++)
            {
                sets[j].LoadEquipment(inventory, new EquipmentLocation { slot = i, set = j });
            }
            inventory._equipmentSlots[i].activeSetIndex = activeSetIndex;
        }

        internal static EquipmentSlotData Read(ReaderContext context)
        {
            var data = new EquipmentSlotData();
            var reader = context.Reader;
            var version = context.Version;

            data.sets = new EquipmentStateData[reader.ReadPackedInt32()];
            for (var j = 0; j < data.sets.Length; j++)
            {
                data.sets[j] = EquipmentStateData.Read(context);
            }

            if (version > 2)
            {
                data.activeSetIndex = reader.ReadByte();
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;
            writer.WritePacked(sets.Length);
            for (var j = 0; j < sets.Length; j++)
            {
                sets[j].Write(context);
            }
            writer.Write(activeSetIndex);
        }
    }
}
