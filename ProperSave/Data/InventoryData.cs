using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

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

        public InventoryData(CharacterMaster master)
        {
            var inventory = master.inventory;
            infusionBonus = (int)inventory.infusionBonus;

            items = new List<ItemData>();
            foreach (var item in inventory.itemAcquisitionOrder)
            {
                items.Add(new ItemData((int)item, inventory.GetItemCount(item)));
            }

            equipments = new EquipmentData[inventory.GetEquipmentSlotCount()];
            for (var i = 0; i < equipments.Length; i++)
            {
                equipments[i] = new EquipmentData(inventory.GetEquipment((uint)i));
            }
            activeEquipmentSlot = inventory.activeEquipmentSlot;
        }

        public void LoadInventory(CharacterMaster master)
        {
            var inventory = master.inventory;
            var itemStacks = inventory.GetFieldValue<int[]>("itemStacks");
            var itemAcquisitionOrder = inventory.itemAcquisitionOrder;

            foreach (var item in items)
            {
                itemStacks[item.itemIndex] = item.count;
                itemAcquisitionOrder.Add((ItemIndex)item.itemIndex);
            }

            if (typeof(Inventory).GetField("onInventoryChanged", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(inventory) is MulticastDelegate onInventoryChanged)
            {
                foreach (var handler in onInventoryChanged.GetInvocationList())
                {
                    handler.Method.Invoke(handler.Target, new object[0]);
                }
            }

            for (byte i = 0; i < equipments.Length; i++)
            {
                equipments[i].LoadEquipment(master, i);
            }
            inventory.SetActiveEquipmentSlot(activeEquipmentSlot);

            inventory.AddInfusionBonus((uint)infusionBonus);
        }
    }
}
