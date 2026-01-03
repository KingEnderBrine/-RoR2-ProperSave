using System.Linq;
using RoR2;

namespace ProperSave.Old.Data
{
    public class DroneMaskData
    {
        public bool[] array;

        internal ProperSave.Data.DroneMaskData Migrate()
        {
            return new ProperSave.Data.DroneMaskData
            {
                enabledItems = array
                .Select((e, i) => e ? (DroneIndex)i : DroneIndex.None)
                .Where(i => i != DroneIndex.None)
                .ToList()
            };
        }
    }
}
