using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProperSave.Data;
using ProperSave.Utils;
using RoR2;

namespace ProperSave.SaveData
{
    public class KarmaManagerData
    {
        public uint actualKarma;
        public List<KarmaTriggerData> runTriggers;

        public static KarmaManagerData Create()
        {
            var data = new KarmaManagerData();

            var manager = KarmaManager.instance;
            data.actualKarma = manager.actualKarma;
            data.runTriggers = new List<KarmaTriggerData>(manager.runTriggers.Count);
            for (var i = 0; i < manager.runTriggers.Count; i++)
            {
                data.runTriggers.Add(KarmaTriggerData.Create(manager.runTriggers[i]));
            }

            return data;
        }

        public void LoadData()
        {
            var manager = KarmaManager.instance;
            manager.SetKarmaServer((int)actualKarma);
            for (var i = 0; i < manager.runTriggers.Count; i++)
            {
                var trigger = manager.runTriggers[i];
                var identifier = trigger.karmaTriggerDef.identifierCapsSafe;
                var triggerData = runTriggers.FirstOrDefault(t => t.identifier == identifier);
                if (triggerData == null)
                {
                    trigger.Deactivate();
                }
                else
                {
                    triggerData.Load(trigger);
                }
            }
        }

        internal static KarmaManagerData Read(ReaderContext context)
        {
            var reader = context.Reader;
            var version = context.Version;
            var data = new KarmaManagerData();
            if (version < 3)
            {
                data.runTriggers = new List<KarmaTriggerData>();
                return data;
            }

            data.actualKarma = reader.ReadPackedUInt32();
            var triggersCount = reader.ReadPackedInt32();
            data.runTriggers = new List<KarmaTriggerData>(triggersCount);
            for (var i = 0; i < triggersCount; i++)
            {
                data.runTriggers.Add(KarmaTriggerData.Read(context));
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.WritePacked(actualKarma);
            writer.WritePacked(runTriggers.Count);
            for (var i = 0; i < runTriggers.Count; i++)
            {
                runTriggers[i].Write(context);
            }
        }
    }
}
