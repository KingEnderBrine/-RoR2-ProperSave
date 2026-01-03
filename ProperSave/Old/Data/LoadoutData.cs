using RoR2;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Old.Data
{
    public partial class LoadoutData
    {
        [DataMember(Name = "ml")]
        public LoadoutBodyData[] modifiedLoadouts;

        internal ProperSave.Data.LoadoutData Migrate()
        {
            return new ProperSave.Data.LoadoutData
            {
                modifiedLoadouts = modifiedLoadouts.Select(l => l.Migrate()).ToList(),
            };
        }
    }
}
