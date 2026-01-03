using ProperSave.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class InventoryData
    {
        public uint infusionBonus;
        public bool equipmentDisabled;
        public float beadAppliedHealth;
        public float beadAppliedShield;
        public float beadAppliedRegen;
        public float beadAppliedDamage;
        public List<ItemData> items = new List<ItemData>();
        public float tempStorageDecayDuration;
        public float tempStorageInvDecayDuration;
        public EquipmentData[][] equipments;
        public byte activeEquipmentSlot;
        public byte[] activeEquipmentSet;
        public int lastExtraEquipmentCount;

        public static InventoryData Create(Inventory inventory)
        {
            var data = new InventoryData();
            data.infusionBonus = inventory.infusionBonus;
            data.equipmentDisabled = inventory.equipmentDisabled;
            data.beadAppliedDamage = inventory.beadAppliedDamage;
            data.beadAppliedHealth = inventory.beadAppliedHealth;
            data.beadAppliedRegen = inventory.beadAppliedRegen;
            data.beadAppliedShield = inventory.beadAppliedShield;

            foreach (var item in inventory.itemAcquisitionOrder)
            {
                data.items.Add(new ItemData
                {
                    itemIndex = item,
                    count = inventory.GetItemCountPermanent(item),
                    channeledCount = inventory.GetItemCountChanneled(item),
                    tempCount = inventory.GetItemCountTemp(item),
                    tempFixedTime = inventory.tempItemsStorage.decayToZeroTimeStamps.GetValue((SparseIndex)item).t
                });
            }
            data.tempStorageDecayDuration = inventory.tempItemsStorage.decayDuration;
            data.tempStorageInvDecayDuration = inventory.tempItemsStorage.invDecayDuration;

            data.equipments = new EquipmentData[inventory.GetEquipmentSlotCount()][];
            for (var i = 0; i < data.equipments.Length; i++)
            {
                var slotEquipments = data.equipments[i] = new EquipmentData[inventory.GetEquipmentSetCount((uint)i)];
                for (var j = 0; j < slotEquipments.Length; j++)
                {
                    slotEquipments[j] = EquipmentData.Create(inventory.GetEquipment((uint)i, (uint)j));
                }
            }
            data.activeEquipmentSlot = inventory.activeEquipmentSlot;
            data.activeEquipmentSet = inventory.activeEquipmentSet.ToArray();
            data.lastExtraEquipmentCount = inventory._lastExtraEquipmentCount;

            return data;
        }

        public void LoadInventory(Inventory inventory)
        {
            inventory.beadAppliedShield = beadAppliedShield;
            inventory.beadAppliedRegen = beadAppliedRegen;
            inventory.beadAppliedHealth = beadAppliedHealth;
            inventory.beadAppliedDamage = beadAppliedDamage;
            inventory.equipmentDisabled = equipmentDisabled;

            inventory.itemAcquisitionOrder.Clear();
            for (var i = 0; i < inventory.itemAcquisitionSet.Length; i++)
            {
                inventory.itemAcquisitionSet[i] = false;
            }

            inventory.tempItemsStorage.decayDuration = tempStorageDecayDuration;
            inventory.tempItemsStorage.invDecayDuration = tempStorageInvDecayDuration;

            foreach (var item in items)
            {
                var itemIndex = item.itemIndex;
                if (itemIndex == ItemIndex.None)
                {
                    continue;
                }

                inventory.permanentItemStacks.SetStackValue(itemIndex, item.count);
                inventory.channeledItemStacks.SetStackValue(itemIndex, item.channeledCount);
                if (item.tempFixedTime > 0)
                {
                    inventory.tempItemsStorage.decayToZeroTimeStamps.SetValue((SparseIndex)itemIndex, new Run.FixedTimeStamp(item.tempFixedTime));
                    inventory.tempItemsStorage.tempItemStacks.SetStackValue(itemIndex, item.tempCount);
                }
                inventory.UpdateEffectiveItemStacks(itemIndex);
            }

            inventory._lastExtraEquipmentCount = lastExtraEquipmentCount;
            inventory.HandleInventoryChanged();
            inventory.AddInfusionBonus(infusionBonus);

            for (uint i = 0; i < equipments.Length; i++)
            {
                var slotEquipments = equipments[i];
                for (uint j = 0; j < slotEquipments.Length; j++)
                {
                    slotEquipments[j].LoadEquipment(inventory, i, j);
                }
            }

            inventory.activeEquipmentSet = activeEquipmentSet;
            if (activeEquipmentSlot > 0)
            {
                inventory.SetActiveEquipmentSlot(activeEquipmentSlot);
            }
            inventory.SetDirtyBit(uint.MaxValue);
        }

        internal static InventoryData Read(ReaderContext context)
        {
            var data = new InventoryData();
            var reader = context.Reader;

            data.infusionBonus = reader.ReadUInt32();
            data.equipmentDisabled = reader.ReadBoolean();
            data.beadAppliedHealth = reader.ReadSingle();
            data.beadAppliedShield = reader.ReadSingle();
            data.beadAppliedRegen = reader.ReadSingle();
            data.beadAppliedDamage = reader.ReadSingle();
            var itemsCount = reader.ReadInt32();
            data.items = new List<ItemData>(itemsCount);
            for (var i = 0; i < itemsCount; i++) {
                data.items.Add(ItemData.Read(context));
            }
            data.tempStorageDecayDuration = reader.ReadSingle();
            data.tempStorageInvDecayDuration = reader.ReadSingle();
            data.equipments = new EquipmentData[reader.ReadInt32()][];
            for (var i = 0; i < data.equipments.Length; i++)
            {
                var slotEquipment = data.equipments[i] = new EquipmentData[reader.ReadInt32()];
                for (var j = 0; j < slotEquipment.Length; j++)
                {
                    slotEquipment[j] = EquipmentData.Read(context);
                }
            }
            data.activeEquipmentSlot = reader.ReadByte();
            data.activeEquipmentSet = new byte[reader.ReadInt32()];
            for (var i = 0; i < data.activeEquipmentSet.Length; i++)
            {
                data.activeEquipmentSet[i] = reader.ReadByte();
            }
            data.lastExtraEquipmentCount = reader.ReadInt32();

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(infusionBonus);
            writer.Write(equipmentDisabled);
            writer.Write(beadAppliedHealth);
            writer.Write(beadAppliedShield);
            writer.Write(beadAppliedRegen);
            writer.Write(beadAppliedDamage);
            writer.Write(items.Count);
            for (var i = 0; i < items.Count; i++) {
                items[i].Write(context);
            }
            writer.Write(tempStorageDecayDuration);
            writer.Write(tempStorageInvDecayDuration);
            writer.Write(equipments.Length);
            for (var i = 0; i < equipments.Length; i++)
            {
                var slotEquipment = equipments[i];
                writer.Write(slotEquipment.Length);
                for (var j = 0; j < slotEquipment.Length; j++)
                {
                    slotEquipment[j].Write(context);
                }
            }
            writer.Write(activeEquipmentSlot);
            writer.Write(activeEquipmentSet.Length);
            for (var i = 0; i < activeEquipmentSet.Length; i++)
            {
                writer.Write(activeEquipmentSet[i]);
            }
            writer.Write(lastExtraEquipmentCount);
        }
    }
}
