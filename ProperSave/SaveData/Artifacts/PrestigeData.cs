using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using RoR2;
using RoR2.Artifacts;

namespace ProperSave.SaveData.Artifacts
{
    public class PrestigeData
    {
        [DataMember(Name = "msc")]
        public int mountainShrineCount;

        internal PrestigeData()
        {
            if (!Run.instance.TryGetComponent<PrestigeBulwarkManager>(out var manager))
            {
                return;
            }

            mountainShrineCount = manager.mountainShrineCount;
        }

        internal void LoadData()
        {
            if (!Run.instance.TryGetComponent<PrestigeBulwarkManager>(out var manager))
            {
                return;
            }

            manager.mountainShrineCount = mountainShrineCount;
        }
    }
}
