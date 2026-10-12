using System;
using System.Collections.Generic;
using System.Text;
using ProperSave.Data;
using ProperSave.Utils;
using RoR2;

namespace ProperSave.SaveData
{
    public class DLC4ProgressionData
    {
        public bool hasSpawnedMajorCatastrophe;
        public int catastrophesSpawnedThisRun;
        public bool shouldDoMajorCatastrophe;

        public bool reaperBossDefeated;
        public bool hasAnyPlayerHadHexes;
        public bool hasWakeSpawned;
        public int lastSpawnedWakeLoopIndex;
        public RngData rng;

        public float trialTimerLeft;

        public static DLC4ProgressionData Create()
        {
            var data = new DLC4ProgressionData();

            var controller = DLC4ProgressionController.instance;
            data.reaperBossDefeated = controller.reaperBossDefeated;
            data.hasAnyPlayerHadHexes = controller.hasAnyPlayerHadHexes;
            data.hasWakeSpawned = controller.hasWakeSpawned;
            data.lastSpawnedWakeLoopIndex = controller.lastSpawnedWakeLoopIndex;
            data.rng = RngData.Create(controller.rng);

            var trialSelector = controller.trialSelector;
            data.trialTimerLeft = trialSelector._trialTimerLeft;

            var catastropheProvider = controller.catastropheProvider;
            data.hasSpawnedMajorCatastrophe = catastropheProvider.hasSpawnedMajorCatastrophe;
            data.catastrophesSpawnedThisRun = catastropheProvider.catastrophesSpawnedThisRun;
            data.shouldDoMajorCatastrophe = catastropheProvider.shouldDoMajorCatastrophe;

            return data;
        }

        public void LoadData()
        {
            var controller = DLC4ProgressionController.instance;
            controller.NetworkreaperBossDefeated = reaperBossDefeated;
            controller.hasAnyPlayerHadHexes = hasAnyPlayerHadHexes;
            controller.hasWakeSpawned = hasWakeSpawned;
            controller.lastSpawnedWakeLoopIndex = lastSpawnedWakeLoopIndex;
            rng.LoadDataOut(out var loadedRng);
            controller.rng = loadedRng;

            var trialSelector = controller.trialSelector;
            trialSelector.Network_trialTimerLeft = trialTimerLeft;

            var catastropheProvider = controller.catastropheProvider;
            catastropheProvider.NetworkhasSpawnedMajorCatastrophe = hasSpawnedMajorCatastrophe;
            catastropheProvider.catastrophesSpawnedThisRun = catastrophesSpawnedThisRun;
            catastropheProvider.NetworkshouldDoMajorCatastrophe = shouldDoMajorCatastrophe;
        }

        internal static DLC4ProgressionData Read(ReaderContext context)
        {
            var reader = context.Reader;
            var version = context.Version;
            var data = new DLC4ProgressionData();
            if (version < 3)
            {
                data.rng = new RngData();
                return data;
            }

            data.hasSpawnedMajorCatastrophe = reader.ReadBoolean();
            data.catastrophesSpawnedThisRun = reader.ReadPackedInt32();
            data.shouldDoMajorCatastrophe = reader.ReadBoolean();

            data.reaperBossDefeated = reader.ReadBoolean();
            data.hasAnyPlayerHadHexes = reader.ReadBoolean();
            data.hasWakeSpawned = reader.ReadBoolean();
            data.lastSpawnedWakeLoopIndex = reader.ReadPackedInt32();
            data.rng = RngData.Read(context);

            data.trialTimerLeft = reader.ReadSingle();

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(hasSpawnedMajorCatastrophe);
            writer.WritePacked(catastrophesSpawnedThisRun);
            writer.Write(shouldDoMajorCatastrophe);

            writer.Write(reaperBossDefeated);
            writer.Write(hasAnyPlayerHadHexes);
            writer.Write(hasWakeSpawned);
            writer.WritePacked(lastSpawnedWakeLoopIndex);
            rng.Write(context);

            writer.Write(trialTimerLeft);
        }
    }
}
