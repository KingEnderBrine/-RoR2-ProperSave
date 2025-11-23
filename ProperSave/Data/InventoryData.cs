using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class InventoryData
    {
        [DataMember(Name = "ib")]
        public uint infusionBonus;
        [DataMember(Name = "ed")]
        public bool equipmentDisabled;
        [DataMember(Name = "bah")]
        public float beadAppliedHealth;
        [DataMember(Name = "bas")]
        public float beadAppliedShield;
        [DataMember(Name = "bar")]
        public float beadAppliedRegen;
        [DataMember(Name = "bad")]
        public float beadAppliedDamage;
        [DataMember(Name = "i")]
        public List<ItemData> items;
        [DataMember(Name = "tsdd")]
        public float tempStorageDecayDuration;
        [DataMember(Name = "tsidd")]
        public float tempStorageInvDecayDuration;

        [DataMember(Name = "e")]
        public EquipmentData[][] equipments;
        [DataMember(Name = "aesl")]
        public byte activeEquipmentSlot;
        [DataMember(Name = "aese")]
        public byte[] activeEquipmentSet;
        [DataMember(Name = "leec")]
        public int lastExtraEquipmentCount;

        public InventoryData(Inventory inventory)
        {
            infusionBonus = inventory.infusionBonus;
            equipmentDisabled = inventory.equipmentDisabled;
            beadAppliedDamage = inventory.beadAppliedDamage;
            beadAppliedHealth = inventory.beadAppliedHealth;
            beadAppliedRegen = inventory.beadAppliedRegen;
            beadAppliedShield = inventory.beadAppliedShield;

            items = new List<ItemData>();
            foreach (var item in inventory.itemAcquisitionOrder)
            {
                items.Add(new ItemData
                {
                    itemIndex = (int)item,
                    count = inventory.GetItemCountPermanent(item),
                    channeledCount = inventory.GetItemCountChanneled(item),
                    tempCount = inventory.GetItemCountTemp(item),
                    tempFixedTime = inventory.tempItemsStorage.decayToZeroTimeStamps.GetValue((SparseIndex)item).t
                });
            }
            tempStorageDecayDuration = inventory.tempItemsStorage.decayDuration;
            tempStorageInvDecayDuration = inventory.tempItemsStorage.invDecayDuration;

            equipments = new EquipmentData[inventory.GetEquipmentSlotCount()][];
            for (var i = 0; i < equipments.Length; i++)
            {
                var slotEquipments = equipments[i] = new EquipmentData[inventory.GetEquipmentSetCount((uint)i)];
                for (var j = 0; j < slotEquipments.Length; j++)
                {
                    slotEquipments[j] = new EquipmentData(inventory.GetEquipment((uint)i, (uint)j));
                }
            }
            activeEquipmentSlot = inventory.activeEquipmentSlot;
            activeEquipmentSet = inventory.activeEquipmentSet.ToArray();
            lastExtraEquipmentCount = inventory._lastExtraEquipmentCount;
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
                inventory.permanentItemStacks.SetStackValue((ItemIndex)item.itemIndex, item.count);
                inventory.channeledItemStacks.SetStackValue((ItemIndex)item.itemIndex, item.channeledCount);
                if (item.tempFixedTime > 0)
                {
                    inventory.tempItemsStorage.decayToZeroTimeStamps.SetValue((SparseIndex)item.itemIndex, new Run.FixedTimeStamp(item.tempFixedTime));
                    inventory.tempItemsStorage.tempItemStacks.SetStackValue((ItemIndex)item.itemIndex, item.tempCount);
                }
                inventory.UpdateEffectiveItemStacks((ItemIndex)item.itemIndex);
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
    }
}
