using ProperSave.Utils;
using RoR2;

namespace ProperSave.Data
{
    public class EquipmentData
    {
        public EquipmentIndex index;
        public byte charges;
        public float chargeFinishTime;

        public static EquipmentData Create(EquipmentState state)
        {
            var data = new EquipmentData();

            data.index = state.equipmentIndex;
            data.charges = state.charges;
            data.chargeFinishTime = state.chargeFinishTime.t;

            return data;
        }

        public void LoadEquipment(Inventory inventory, uint equipmentSlot, uint equipmentSet)
        {

            var state = new EquipmentState(
                index,
                new Run.FixedTimeStamp(chargeFinishTime),
                charges
                );
            inventory.SetEquipment(state, equipmentSlot, equipmentSet);
        }

        internal static EquipmentData Read(ReaderContext context)
        {
            var data = new EquipmentData();
            var reader = context.Reader;
            var version = context.Version;

            data.index = SharedIndexHelpers.ResolveEquipment(version > 1 ? reader.ReadPackedInt32() : reader.ReadInt32(), context);
            data.charges = reader.ReadByte();
            data.chargeFinishTime = reader.ReadSingle();

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(SharedIndexHelpers.FromEquipment(index, context));
            writer.Write(charges);
            writer.Write(chargeFinishTime);
        }
    }
}
