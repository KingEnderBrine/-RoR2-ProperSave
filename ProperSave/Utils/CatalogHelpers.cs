using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoR2;

namespace ProperSave.Utils
{
    public static class CatalogHelpers
    {
        private static Dictionary<string, DroneIndex> DroneNameToIndex { get; set; }
        public static DifficultyIndex FindDifficultyIndex(string nameToken)
        {
            try
            {
                if (ModCompat.IsR2APIDifficultyLoaded)
                {
                    return ModCompat.FindR2APIDifficultyIndex(nameToken);
                }
            }
            catch (Exception ex)
            {
                ProperSavePlugin.InstanceLogger.LogError(ex);
            }

            for (var i = 0; i < DifficultyCatalog.difficultyDefs.Length; i++)
            {
                var def = DifficultyCatalog.difficultyDefs[i];
                if (def.nameToken == nameToken)
                {
                    return (DifficultyIndex)i;
                }
            }

            return DifficultyIndex.Invalid;
        }

        public static DroneIndex FindDroneIndex(string name)
        {
            DroneNameToIndex ??= DroneCatalog.droneDefs.ToDictionary((d) => d.name, d => d.droneIndex);
            if (DroneNameToIndex.TryGetValue(name, out var index))
            {
                return index;
            }

            return DroneIndex.None;
        }
    }
}
