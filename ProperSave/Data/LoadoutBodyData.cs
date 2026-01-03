using ProperSave.Utils;
using RoR2;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class LoadoutBodyData
    {
        public BodyIndex bodyIndex;
        public uint skinPreference;
        public uint[] skillPreferences;

        public static LoadoutBodyData Create(Loadout.BodyLoadoutManager.BodyLoadout bodyLoadout)
        {
            return new LoadoutBodyData
            {
                bodyIndex = bodyLoadout.bodyIndex,
                skinPreference = bodyLoadout.skinPreference,
                skillPreferences = bodyLoadout.skillPreferences,
            };
        }

        public Loadout.BodyLoadoutManager.BodyLoadout Load()
        {
            if (bodyIndex == BodyIndex.None)
            {
                return null;
            }

            return new Loadout.BodyLoadoutManager.BodyLoadout
            {
                bodyIndex = bodyIndex,
                skinPreference = skinPreference,
                skillPreferences = skillPreferences
            };
        }

        internal static LoadoutBodyData Read(ReaderContext context)
        {
            var data = new LoadoutBodyData();
            var reader = context.Reader;

            data.bodyIndex = SharedIndexHelpers.ResolveBody(reader.ReadInt32(), context);
            data.skinPreference = reader.ReadUInt32();
            data.skillPreferences = new uint[reader.ReadInt32()];
            for (var i = 0; i < data.skillPreferences.Length; i++)
            {
                data.skillPreferences[i] = reader.ReadUInt32();
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(SharedIndexHelpers.FromBody(bodyIndex, context));
            writer.Write(skinPreference);
            writer.Write(skillPreferences.Length);
            for (var i = 0; i < skillPreferences.Length; i++)
            {
                writer.Write(skillPreferences[i]);
            }
        }
    }
}
