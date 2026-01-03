using ProperSave.Data;
using ProperSave.Utils;
using RoR2;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProperSave.SaveData.Runs
{
    public class InfiniteTowerTypedRunData : ITypedRunData
    {
        public int waveIndex;
        public RngData waveRng;
        public RngData enemyItemRng;
        public RngData safeWardRng;
        public int enemyItemPatternIndex;
        public InventoryData enemyInventory;

        internal static InfiniteTowerTypedRunData Create()
        {
            var run = Run.instance as InfiniteTowerRun;
            return new InfiniteTowerTypedRunData
            {
                waveIndex = run.waveIndex,
                waveRng = RngData.Create(run.waveRng),
                enemyItemRng = RngData.Create(run.enemyItemRng),
                safeWardRng = Saving.PreStageInfiniteTowerSafeWardRng,
                enemyItemPatternIndex = run.enemyItemPatternIndex,
                enemyInventory = InventoryData.Create(run.enemyInventory),
            };
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

        internal static InfiniteTowerTypedRunData Read(ReaderContext context)
        {
            var data = new InfiniteTowerTypedRunData();
            var reader = context.Reader;
            data.waveIndex = reader.ReadInt32();
            data.waveRng = RngData.Read(context);
            data.enemyItemRng = RngData.Read(context);
            data.safeWardRng = RngData.Read(context);
            data.enemyItemPatternIndex = reader.ReadInt32();
            data.enemyInventory = InventoryData.Read(context);

            return data;
        }

        void ITypedRunData.Write(WriterContext context) => Write(context);

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(waveIndex);
            waveRng.Write(context);
            enemyItemRng.Write(context);
            safeWardRng.Write(context);
            writer.Write(enemyItemPatternIndex);
            enemyInventory.Write(context);
        }
    }
}
