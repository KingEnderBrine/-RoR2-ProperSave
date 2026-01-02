using ProperSave.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public partial class LoadoutData
    {
        public List<LoadoutBodyData> modifiedLoadouts = new List<LoadoutBodyData>();

        public static LoadoutData Create(Loadout loadout)
        {
            var data = new LoadoutData();
            var modifiedBodyLoadouts = loadout.bodyLoadoutManager.modifiedBodyLoadouts;
            data.modifiedLoadouts.AddRange(modifiedBodyLoadouts.Select(LoadoutBodyData.Create));

            return data;
        }

        public void LoadData(Loadout loadout)
        {
            loadout.Clear();

            var manager = loadout.bodyLoadoutManager;
            manager.modifiedBodyLoadouts = modifiedLoadouts
                .Select(el => el.Load())
                .Where(l => l != null)
                .ToArray();
        }

        internal static LoadoutData Read(ReaderContext context)
        {
            var data = new LoadoutData();
            var reader = context.Reader;

            var modifiedLoadoutsCount = reader.ReadInt32();
            data.modifiedLoadouts = new List<LoadoutBodyData>(modifiedLoadoutsCount);
            for (var i = 0; i < modifiedLoadoutsCount; i++)
            {
                data.modifiedLoadouts.Add(LoadoutBodyData.Read(context));
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(modifiedLoadouts.Count);
            for (var i = 0; i < modifiedLoadouts.Count; i++)
            {
                modifiedLoadouts[i].Write(context);
            }
        }
    }
}
