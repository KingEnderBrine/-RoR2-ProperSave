using RoR2;
using System.Collections.Generic;
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

        [DataMember(Name = "e")]
        public EquipmentData[][] equipments;
        [DataMember(Name = "aes")]
        public byte activeEquipmentSlot; 

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
                items.Add(new ItemData { itemIndex = (int)item, countPerm = inventory.GetItemCountPermanent(item), countTemp = inventory.GetTempItemRawValue(item) });
            }

            equipments = new EquipmentData[inventory.GetEquipmentSlotCount()][];
            // equipment list isnt square so have to set both lengths individually
            for (var slot = 0; slot < inventory.GetEquipmentSlotCount(); slot++)
            {
                equipments[slot] = new EquipmentData[inventory.GetEquipmentSetCount((uint)slot)];
                for (var set = 0; set < inventory.GetEquipmentSetCount((uint)slot); set++)
                {
                    equipments[slot][set] = new EquipmentData(inventory.GetEquipment((uint)slot, (uint)set));
                    // slot is for multi retool
                    // set is for Functional Coupler item
                }
            }

            activeEquipmentSlot = inventory.activeEquipmentSlot;

        }

        public void LoadInventory(Inventory inventory)
        {
            inventory.beadAppliedShield = beadAppliedShield;
            inventory.beadAppliedRegen = beadAppliedRegen;
            inventory.beadAppliedHealth = beadAppliedHealth;
            inventory.beadAppliedDamage = beadAppliedDamage;
            inventory.equipmentDisabled = equipmentDisabled;

            inventory.itemAcquisitionOrder.Clear();
            foreach (var item in items)
            {
                //Seems to preserve pickup order without explicitly setting it.
                inventory.GiveItemPermanent((ItemIndex)item.itemIndex, item.countPerm);
                inventory.GiveItemTemp((ItemIndex)item.itemIndex, item.countTemp);
            }

            inventory.HandleInventoryChanged();
            inventory.AddInfusionBonus(infusionBonus);
            
            for (byte slot = 0; slot < equipments.Length; slot++)
            {
                for (byte set = 0; set < equipments[slot].Length; set++)
                    equipments[slot][set].LoadEquipment(inventory, slot, set);
            }
            inventory.SetActiveEquipmentSlot(activeEquipmentSlot); //yes this causes errors. No I dont want to fix it because using multi-retool causes the same errors so I am going to blame gearbox

            // removes duplicate equipment sets. Without this each time you load with the Functional Coupler in your inventory you get an extra equipment slot
            inventory.UpdateEquipment();
        }
    }
}
