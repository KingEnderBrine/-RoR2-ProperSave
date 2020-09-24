using RoR2;
using System;
using System.Reflection;
using System.Runtime.Serialization;
using static RoR2.Run;

namespace ProperSave.Data
{
    public class EquipmentData
    {
        [DataMember(Name = "i")]
        public int index;
        [DataMember(Name = "c")]
        public byte charges;
        [DataMember(Name = "cft")]
        public float chargeFinishTime;

        public EquipmentData(EquipmentState state)
        {
            index = (int)state.equipmentIndex;
            charges = state.charges;
            chargeFinishTime = state.chargeFinishTime.t;
        }

        public void LoadEquipment(CharacterMaster player, byte equipmentSlot)
        {
            var inventory = player.inventory;
            var chargeTime = new FixedTimeStamp(chargeFinishTime);
            var state = new EquipmentState(
                (EquipmentIndex)index,
                chargeTime,
                charges
                );
            inventory.SetEquipment(state, equipmentSlot);
        }
    }
}
