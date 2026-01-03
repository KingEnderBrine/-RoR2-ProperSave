using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using RoR2;

namespace ProperSave.Old.Data
{
    public class DroneRepairData
    {
        [DataMember(Name = "rbsl")]
        public string requestedBySlotName;

        internal ProperSave.Data.DroneRepairData Migrate()
        {
            return new ProperSave.Data.DroneRepairData
            {
                requestedBySlotName = requestedBySlotName,
            };
        }
    }
}
