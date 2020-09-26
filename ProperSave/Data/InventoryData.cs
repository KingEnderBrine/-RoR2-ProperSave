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
        public int infusionBonus;
        [DataMember(Name = "i")]
        public List<ItemData> items;

        [DataMember(Name = "e")]
        public EquipmentData[] equipments;
        [DataMember(Name = "aes")]
        public byte activeEquipmentSlot;

        private static FieldInfo onInventoryChangedDelagate = typeof(Inventory).GetField("onInventoryChanged", BindingFlags.NonPublic | BindingFlags.Instance);

        public InventoryData(Inventory inventory)
        {
            infusionBonus = (int)inventory.infusionBonus;

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
            var itemStacks = inventory.itemStacks;
            var itemAcquisitionOrder = inventory.itemAcquisitionOrder;

            foreach (var item in items)
            {
                itemStacks[item.itemIndex] = item.count;
                itemAcquisitionOrder.Add((ItemIndex)item.itemIndex);
            }

            if (onInventoryChangedDelagate.GetValue(inventory) is MulticastDelegate onInventoryChanged)
            {
                foreach (var handler in onInventoryChanged.GetInvocationList())
                {
                    handler.Method.Invoke(handler.Target, Array.Empty<object>());
                }
            }

            for (byte i = 0; i < equipments.Length; i++)
            {
                equipments[i].LoadEquipment(inventory, i);
            }
            inventory.SetActiveEquipmentSlot(activeEquipmentSlot);

            inventory.AddInfusionBonus((uint)infusionBonus);
        }
    }
}
