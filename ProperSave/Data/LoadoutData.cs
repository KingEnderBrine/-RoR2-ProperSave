using R2API.Utils;
using RoR2;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class LoadoutData
    {
        [DataMember(Name = "ml")]
        public LoadoutBodyData[] modifiedLoadouts;

        public LoadoutData(CharacterMaster master)
        {
            var modifiedBodyLoadouts = master.loadout.bodyLoadoutManager.GetFieldValue<object[]>("modifiedBodyLoadouts");

            modifiedLoadouts = new LoadoutBodyData[modifiedBodyLoadouts.Length];
            for(var i = 0; i < modifiedBodyLoadouts.Length; i++)
            {
                var loadoutBody = modifiedBodyLoadouts[i];
                modifiedLoadouts[i] = new LoadoutBodyData()
                {
                    bodyIndex = loadoutBody.GetFieldValue<int>("bodyIndex"),
                    skinPreference = loadoutBody.GetFieldValue<uint>("skinPreference"),
                    skillPreferences = loadoutBody.GetFieldValue<uint[]>("skillPreferences").ToArray()
                };
            }
        }

        public void LoadData(CharacterMaster master)
        {
            master.loadout.Clear();

            var manager = master.loadout.bodyLoadoutManager;
            var bodyLoadoutType = typeof(Loadout.BodyLoadoutManager).GetNestedType("BodyLoadout", BindingFlags.NonPublic);
            var bodyLoadoutArrayType = bodyLoadoutType.MakeArrayType();

            var modifiedBodyLoadouts = Activator.CreateInstance(bodyLoadoutArrayType, new object[] { modifiedLoadouts.Length }) as object[];

            for (var i = 0; i < modifiedLoadouts.Length; i++)
            {
                var modifiedLoadout = modifiedLoadouts[i];
                
                var bodyLoadout = FormatterServices.GetUninitializedObject(bodyLoadoutType);
                bodyLoadout.SetFieldValue("bodyIndex", modifiedLoadout.bodyIndex);
                bodyLoadout.SetFieldValue("skinPreference", modifiedLoadout.skinPreference);
                bodyLoadout.SetFieldValue("skillPreferences", modifiedLoadout.skillPreferences.ToArray());

                modifiedBodyLoadouts[i] = bodyLoadout;
            }
            typeof(Loadout.BodyLoadoutManager).GetField("modifiedBodyLoadouts", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(manager, modifiedBodyLoadouts);
        }

        public class LoadoutBodyData
        {
            [DataMember(Name = "bi")]
            public int bodyIndex;
            [DataMember(Name = "sp")]
            public uint skinPreference;
            [DataMember(Name = "sps")]
            public uint[] skillPreferences;
        }
    }
}
