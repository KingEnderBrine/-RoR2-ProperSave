using RoR2;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public partial class LoadoutData
    {
        [DataMember(Name = "ml")]
        public LoadoutBodyData[] modifiedLoadouts;

        public LoadoutData(Loadout loadout)
        {
            var modifiedBodyLoadouts = loadout.bodyLoadoutManager.modifiedBodyLoadouts;
            modifiedLoadouts = modifiedBodyLoadouts.Select(el => new LoadoutBodyData(el)).ToArray();
        }

        public void LoadData(Loadout loadout)
        {
            loadout.Clear();

            var manager = loadout.bodyLoadoutManager;
            manager.modifiedBodyLoadouts = modifiedLoadouts.Select(el => el.Load()).ToArray();
        }
    }
}
