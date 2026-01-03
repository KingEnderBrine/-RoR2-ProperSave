using ProperSave.Old.Data;
using RoR2;
using System.Runtime.Serialization;

namespace ProperSave.Old.SaveData.Runs
{
    public class InfiniteTowerTypedRunData : ITypedRunData
    {
        [DataMember(Name = "wi")]
        public int waveIndex;
        [DataMember(Name = "wr")]
        public RngData waveRng;
        [DataMember(Name = "eir")]
        public RngData enemyItemRng;
        [DataMember(Name = "swr")]
        public RngData safeWardRng;
        [DataMember(Name = "eipi")]
        public int enemyItemPatternIndex;
        [DataMember(Name = "ei")]
        public InventoryData enemyInventory;

        public ProperSave.SaveData.Runs.ITypedRunData Migrate()
        {
            return new ProperSave.SaveData.Runs.InfiniteTowerTypedRunData
            {
                enemyInventory = enemyInventory.Migrate(),
                enemyItemPatternIndex = enemyItemPatternIndex,
                enemyItemRng = enemyItemRng.Migrate(),
                safeWardRng = safeWardRng.Migrate(),
                waveIndex = waveIndex,
                waveRng = waveRng.Migrate(),
            };
        }
    }
}
