using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using ProperSave.Utils;
using RoR2;

namespace ProperSave.Data
{
    public class DevotedLemurianData
    {
        public ItemIndex itemIndex;
        public int devotedEvolutionLevel;

        public static DevotedLemurianData Create(DevotedLemurianController controller)
        {
            return new DevotedLemurianData
            {
                itemIndex = controller.DevotionItem,
                devotedEvolutionLevel = controller.DevotedEvolutionLevel,
            };
        }

        public void LoadData(DevotedLemurianController controller)
        {
            controller._devotionItem = itemIndex;
            controller._devotedEvolutionLevel = devotedEvolutionLevel;
        }

        internal static DevotedLemurianData Read(ReaderContext context)
        {
            var data = new DevotedLemurianData();
            var reader = context.Reader;

            data.itemIndex = SharedIndexHelpers.ResolveItem(reader.ReadInt32(), context);
            data.devotedEvolutionLevel = reader.ReadInt32();

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(SharedIndexHelpers.FromItem(itemIndex, context));
            writer.Write(devotedEvolutionLevel);
        }
    }
}
