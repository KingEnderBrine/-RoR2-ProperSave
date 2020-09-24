using R2API.Utils;
using RoR2;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public partial class LoadoutData
    {
        [DataMember(Name = "ml")]
        public LoadoutBodyData[] modifiedLoadouts;

        public LoadoutData(CharacterMaster master)
        {
            var modifiedBodyLoadouts = master.loadout.bodyLoadoutManager.modifiedBodyLoadouts;
            modifiedLoadouts = modifiedBodyLoadouts.Select(el => new LoadoutBodyData(el)).ToArray();
        }

        public void LoadData(CharacterMaster master)
        {
            master.loadout.Clear();

            var manager = master.loadout.bodyLoadoutManager;
            manager.modifiedBodyLoadouts = modifiedLoadouts.Select(el => el.Load()).ToArray();
        }
    }
}
