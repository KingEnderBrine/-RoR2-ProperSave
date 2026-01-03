using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Old.Data
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

        internal ProperSave.Data.InventoryData Migrate()
        {
            return new ProperSave.Data.InventoryData
            {
                activeEquipmentSet = activeEquipmentSet,
                activeEquipmentSlot = activeEquipmentSlot,
                beadAppliedDamage = beadAppliedDamage,
                beadAppliedHealth = beadAppliedHealth,
                beadAppliedShield = beadAppliedShield,
                beadAppliedRegen = beadAppliedRegen,
                equipmentDisabled = equipmentDisabled,
                equipments = equipments.Select(e => e.Select(e => e.Migrate()).ToArray()).ToArray(),
                infusionBonus = infusionBonus,
                items = items.Select(i => i.Migrate()).ToList(),
                lastExtraEquipmentCount = lastExtraEquipmentCount,
                tempStorageDecayDuration = tempStorageDecayDuration,
                tempStorageInvDecayDuration = tempStorageInvDecayDuration,
            };
        }
    }
}
