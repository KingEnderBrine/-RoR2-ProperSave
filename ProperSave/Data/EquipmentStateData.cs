using ProperSave.Utils;
using RoR2;

namespace ProperSave.Data
{
    public class EquipmentStateData
    {
        public EquipmentIndex index;
        public byte charges;
        public float chargeFinishTime;
        public bool isDisabled;

        public static EquipmentStateData Create(EquipmentState state)
        {
            var data = new EquipmentStateData();

            data.index = state.equipmentIndex;
            data.charges = state.charges;
            data.chargeFinishTime = state.chargeFinishTime.t;
            data.isDisabled = state.isDisabled;

            return data;
        }

        public void LoadEquipment(Inventory inventory, EquipmentLocation location)
        {
            var state = new EquipmentState(
                index,
                new Run.FixedTimeStamp(chargeFinishTime),
                charges,
                isDisabled
                );
            inventory.SetEquipment(state, location);
        }

        internal static EquipmentStateData Read(ReaderContext context)
        {
            var data = new EquipmentStateData();
            var reader = context.Reader;
            var version = context.Version;

            data.index = SharedIndexHelpers.ResolveEquipment(version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32(), context);
            data.charges = reader.ReadByte();
            data.chargeFinishTime = reader.ReadSingle();

            if (version > 2)
            {
                data.isDisabled = reader.ReadBoolean();
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(SharedIndexHelpers.FromEquipment(index, context));
            writer.Write(charges);
            writer.Write(chargeFinishTime);
            writer.Write(isDisabled);
        }
    }
}
