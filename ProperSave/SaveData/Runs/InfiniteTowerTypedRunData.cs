using ProperSave.Data;
using RoR2;
using System.Runtime.Serialization;

namespace ProperSave.SaveData.Runs
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

        internal InfiniteTowerTypedRunData()
        {
            var run = Run.instance as InfiniteTowerRun;
            waveIndex = run.waveIndex;
            waveRng = new RngData(run.waveRng);
            enemyItemRng = new RngData(run.enemyItemRng);
            safeWardRng = Saving.PreStageInfiniteTowerSafeWardRng;
            enemyItemPatternIndex = run.enemyItemPatternIndex;
            enemyInventory = new InventoryData(run.enemyInventory);
        }

        void ITypedRunData.Load()
        {
            var run = Run.instance as InfiniteTowerRun;
            run._waveIndex = waveIndex;
            waveRng.LoadDataRef(ref run.waveRng);
            enemyItemRng.LoadDataRef(ref run.enemyItemRng);
            safeWardRng.LoadDataRef(ref run.safeWardRng);
            run.enemyItemPatternIndex = enemyItemPatternIndex;
            enemyInventory.LoadInventory(run.enemyInventory);
        }
    }
}
