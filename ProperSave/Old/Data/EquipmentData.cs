using RoR2;
using System.Runtime.Serialization;

namespace ProperSave.Old.Data
{
    public class EquipmentData
    {
        [DataMember(Name = "i")]
        public int index;
        [DataMember(Name = "c")]
        public byte charges;
        [DataMember(Name = "cft")]
        public float chargeFinishTime;

        internal ProperSave.Data.EquipmentData Migrate()
        {
            return new ProperSave.Data.EquipmentData
            {
                chargeFinishTime = chargeFinishTime,
                charges = charges,
                index = (EquipmentIndex)index,
            };
        }
    }
}
