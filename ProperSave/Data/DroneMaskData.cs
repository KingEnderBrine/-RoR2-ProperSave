using System.Linq;
using RoR2;

namespace ProperSave.Data
{
    public class DroneMaskData
    {
        public bool[] array;

        public DroneMaskData(DroneMask mask)
        {
            array = mask.array.ToArray();
        }

        public void LoadDataOut(out DroneMask mask)
        {
            mask = new DroneMask() { array = array.ToArray() };
        }
    }
}
