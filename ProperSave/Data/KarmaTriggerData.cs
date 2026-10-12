using System;
using System.Collections.Generic;
using System.Text;
using ProperSave.Utils;
using RoR2.Karma;

namespace ProperSave.Data
{
    public class KarmaTriggerData
    {
        public string identifier;
        public int timesFlipped;
        public int karmaReward;
        public bool isComplete;

        public static KarmaTriggerData Create(BaseKarmaTrigger karmaTrigger)
        {
            var data = new KarmaTriggerData();

            data.identifier = karmaTrigger.karmaTriggerDef.identifierCapsSafe;
            data.timesFlipped = karmaTrigger.timesFlipped;
            data.karmaReward = karmaTrigger.karmaReward;
            data.isComplete = karmaTrigger.isComplete;

            return data;
        }

        public void Load(BaseKarmaTrigger karmaTrigger)
        {
            karmaTrigger.timesFlipped = timesFlipped;
            karmaTrigger.karmaReward = karmaReward;
            karmaTrigger.isComplete = isComplete;

            if (isComplete)
            {
                karmaTrigger.Deactivate();
            }
        }

        internal static KarmaTriggerData Read(ReaderContext context)
        {
            var reader = context.Reader;
            var data = new KarmaTriggerData();

            data.identifier = context.SharedStrings[reader.ReadPackedInt32()];
            data.timesFlipped = reader.ReadPackedInt32();
            data.karmaReward = reader.ReadPackedInt32();
            data.isComplete = reader.ReadBoolean();

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(context.SharedStrings.AddOrIndexOf(identifier));
            writer.WritePacked(timesFlipped);
            writer.WritePacked(karmaReward);
            writer.Write(isComplete);
        }
    }
}
