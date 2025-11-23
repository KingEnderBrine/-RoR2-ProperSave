using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using RoR2;

namespace ProperSave.Data
{
    public class DroneRepairData
    {
        [DataMember(Name = "rbsl")]
        public string requestedBySlotName;

        public DroneRepairData(DroneRepairMaster droneRepairMaster)
        {
            requestedBySlotName = droneRepairMaster.requestedBySlotName;
        }

        internal void LoadData(DroneRepairMaster droneRepairMaster)
        {
            droneRepairMaster.requestedBySlotName = requestedBySlotName;
        }
    }
}
