using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using ProperSave.Utils;
using RoR2;

namespace ProperSave.Data
{
    public class DroneRepairData
    {
        public string requestedBySlotName;

        public static DroneRepairData Create(DroneRepairMaster droneRepairMaster)
        {
            return new DroneRepairData
            {
                requestedBySlotName = droneRepairMaster.requestedBySlotName,
            };
        }

        internal void LoadData(DroneRepairMaster droneRepairMaster)
        {
            droneRepairMaster.requestedBySlotName = requestedBySlotName;
        }

        internal static DroneRepairData Read(ReaderContext context)
        {
            var data = new DroneRepairData();

            data.requestedBySlotName = context.Reader.ReadString();

            return data;
        }

        internal void Write(WriterContext context)
        {
            context.Writer.Write(requestedBySlotName);
        }
    }
}
