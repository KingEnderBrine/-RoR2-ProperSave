using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class InventoryData
    {
        [DataMember(Name = "ib")]
        public uint infusionBonus;
        [DataMember(Name = "i")]
        public List<ItemData> items;

        [DataMember(Name = "e")]
        public EquipmentData[] equipments;
        [DataMember(Name = "aes")]
        public byte activeEquipmentSlot;

        public InventoryData(Inventory inventory)
        {
            infusionBonus = inventory.infusionBonus;

            items = new List<ItemData>();
            foreach (var item in inventory.itemAcquisitionOrder)
            {
                items.Add(new ItemData { itemIndex = (int)item, count = inventory.GetItemCount(item) });
            }

            equipments = new EquipmentData[inventory.GetEquipmentSlotCount()];
            for (var i = 0; i < equipments.Length; i++)
            {
                equipments[i] = new EquipmentData(inventory.GetEquipment((uint)i));
            }
            activeEquipmentSlot = inventory.activeEquipmentSlot;
        }

        public void LoadInventory(Inventory inventory)
        {
            inventory.itemAcquisitionOrder.Clear();
            foreach (var item in items)
            {
                inventory.itemStacks[item.itemIndex] = item.count;
                inventory.itemAcquisitionOrder.Add((ItemIndex)item.itemIndex);
            }

            inventory.HandleInventoryChanged();

            for (byte i = 0; i < equipments.Length; i++)
            {
                equipments[i].LoadEquipment(inventory, i);
            }
            inventory.SetActiveEquipmentSlot(activeEquipmentSlot);

            inventory.AddInfusionBonus(infusionBonus);
        }
    }
}
