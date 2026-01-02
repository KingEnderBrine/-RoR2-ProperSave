using ProperSave.Data;
using ProperSave.SaveData.Runs;
using ProperSave.TinyJson;
using ProperSave.Utils;
using RoR2;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace ProperSave.SaveData
{
    public class RunData
    {
        public ulong seed;
        public DifficultyIndex difficultyIndex;
        public float fixedTime;
        public float time;
        public bool isPaused;
        public float offsetFromFixedTime;
        public int stageClearCount;
        public int stageClearCountAtLoopStart;
        public int loopClearCount;
        public string sceneName;
        public string nextSceneName;
        public string previousSceneName;
        public int prestigeArtifactMountainValue;

        public ItemMaskData itemMask;
        public EquipmentMaskData equipmentMask;
        public DroneMaskData droneMask;
        public int shopPortalCount;
        public string[] eventFlags;
        public RunRngData runRng;
        public ArtifactIndex trialArtifactIndex;
        public RuleBookData ruleBook;
        public ITypedRunData typedRunData;

        private static readonly FieldInfo onRunStartGlobalDelegate = typeof(Run).GetField(nameof(Run.onRunStartGlobal), BindingFlags.NonPublic | BindingFlags.Static);
        
        internal static RunData Create()
        {
            var data = new RunData();
            var run = Run.instance;
            data.seed = run.seed;
            data.difficultyIndex = run.selectedDifficulty;

            var stopWatch = run.runStopwatch;
            data.isPaused = stopWatch.isPaused;
            data.offsetFromFixedTime = stopWatch.offsetFromFixedTime;
            data.fixedTime = run.fixedTime;
            data.time = run.time;

            data.stageClearCount = run.stageClearCount;
            data.stageClearCountAtLoopStart = run.stageClearCountAtLoopStart;
            data.loopClearCount = run._loopClearCount;
            data.sceneName = SceneManager.GetActiveScene().name;
            data.nextSceneName = run.nextStageScene.cachedName;
            data.previousSceneName = Saving.PreStageSceneName;

            data.shopPortalCount = run.shopPortalCount;
            data.prestigeArtifactMountainValue = run.prestiegeArtifactMountainValue;

            data.itemMask = ItemMaskData.Create(run.availableItems);
            data.equipmentMask = EquipmentMaskData.Create(run.availableEquipment);
            data.droneMask = DroneMaskData.Create(run.availableDrones);

            data.runRng = Saving.PreStageRng;

            data.eventFlags = run.eventFlags.ToArray();

            var artifactController = UnityEngine.Object.FindObjectOfType<ArtifactTrialMissionController>();
            data.trialArtifactIndex = (ArtifactIndex)(artifactController?.currentArtifactIndex ?? -1);

            data.ruleBook = RuleBookData.Create(run.ruleBook);

            if (run is InfiniteTowerRun)
            {
                data.typedRunData = InfiniteTowerTypedRunData.Create();
            }

            return data;
        }

        //Upgraded copy of Run.Start
        internal void LoadData()
        {
            ModCompat.ShareSuiteMapTransition();

            if (trialArtifactIndex != ArtifactIndex.None)
            {
                ArtifactTrialMissionController.trialArtifact = ArtifactCatalog.GetArtifactDef(trialArtifactIndex);
            }

            var instance = Run.instance;

            instance.SetRuleBook(ruleBook.Load());
            instance.OnRuleBookUpdated(instance.networkRuleBookComponent);

            instance.seed = seed;
            if (difficultyIndex == DifficultyIndex.Invalid)
            {
                instance.selectedDifficulty = DifficultyIndex.Easy;
            }
            else
            {
                instance.selectedDifficulty = difficultyIndex;
            }

            instance.shopPortalCount = shopPortalCount;
            instance.prestiegeArtifactMountainValue = prestigeArtifactMountainValue;

            runRng.LoadData(instance);
            typedRunData?.Load();

            instance.allowNewParticipants = true;
            UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);

            var onlyInstancesList = NetworkUser.readOnlyInstancesList;
            for (int index = 0; index < onlyInstancesList.Count; ++index)
            {
                instance.OnUserAdded(onlyInstancesList[index]);
            }
            instance.allowNewParticipants = false;

            instance.stageClearCount = stageClearCount;
            instance.stageClearCountAtLoopStart = stageClearCountAtLoopStart;
            instance._loopClearCount = loopClearCount;
            instance.RecalculateDifficultyCoefficent();

            instance.nextStageScene = SceneCatalog.GetSceneDefFromSceneName(nextSceneName);
            if (stageClearCount == stageClearCountAtLoopStart)
            {
                instance.OnLoopBeginServer(SceneCatalog.GetSceneDefFromSceneName(previousSceneName));
            }
            //Stage rng is normally generated earlier, but when scene is changed GenerateLoopRNG is executed before GenerateStageRNG,
            //hoping that no one will want to use stage rng in onLoopBeginServer event
            instance.GenerateStageRNG();

            NetworkManager.singleton.ServerChangeScene(sceneName);

            itemMask.LoadData(instance.availableItems);
            equipmentMask.LoadData(instance.availableEquipment);
            droneMask.LoadData(instance.availableDrones);

            instance.BuildUnlockAvailability();
            instance.BuildDropTable();

            foreach (var flag in eventFlags)
            {
                instance.SetEventFlag(flag);
            }

            instance.isRunning = true;

            if (onRunStartGlobalDelegate.GetValue(null) is MulticastDelegate onRunStartGlobal && onRunStartGlobal != null)
            {
                foreach (var handler in onRunStartGlobal.GetInvocationList())
                {
                    handler.Method.Invoke(handler.Target, new object[] { instance });
                }
            }

            instance.fixedTime = fixedTime;
            instance.time = time;
            instance.runStopwatch = new Run.RunStopwatch
            {
                offsetFromFixedTime = offsetFromFixedTime,
                isPaused = isPaused
            };
        }

        internal static RunData Read(ReaderContext context)
        {
            var data = new RunData();
            var reader = context.Reader;

            data.seed = reader.ReadUInt64();
            data.difficultyIndex = SharedIndexHelpers.ResolveDifficulty(reader.ReadInt32(), context);
            data.fixedTime = reader.ReadSingle();
            data.time = reader.ReadSingle();
            data.isPaused = reader.ReadBoolean();
            data.offsetFromFixedTime = reader.ReadSingle();
            data.stageClearCount = reader.ReadInt32();
            data.stageClearCountAtLoopStart = reader.ReadInt32();
            data.loopClearCount = reader.ReadInt32();
            data.sceneName = reader.ReadString();
            data.nextSceneName = reader.ReadString();
            data.previousSceneName = reader.ReadString();
            data.prestigeArtifactMountainValue = reader.ReadInt32();
            data.itemMask = ItemMaskData.Read(context);
            data.equipmentMask = EquipmentMaskData.Read(context);
            data.droneMask = DroneMaskData.Read(context);
            data.shopPortalCount = reader.ReadInt32();
            data.eventFlags = new string[reader.ReadInt32()];
            for (var i = 0; i < data.eventFlags.Length; i++)
            {
                data.eventFlags[i] = reader.ReadString();
            }
            data.runRng = RunRngData.Read(context);
            data.trialArtifactIndex = SharedIndexHelpers.ResolveArtifact(reader.ReadInt32(), context);
            data.ruleBook = RuleBookData.Read(context);
            var typedRunDataType = reader.ReadString();
            if (typedRunDataType != "")
            {
                var type = Type.GetType(typedRunDataType, false);
                data.typedRunData = (ITypedRunData)type.GetMethod("Read", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { context });
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(seed);
            writer.Write(SharedIndexHelpers.FromDifficulty(difficultyIndex, context));
            writer.Write(fixedTime);
            writer.Write(time);
            writer.Write(isPaused);
            writer.Write(offsetFromFixedTime);
            writer.Write(stageClearCount);
            writer.Write(stageClearCountAtLoopStart);
            writer.Write(loopClearCount);
            writer.Write(sceneName);
            writer.Write(nextSceneName);
            writer.Write(previousSceneName);
            writer.Write(prestigeArtifactMountainValue);
            itemMask.Write(context);
            equipmentMask.Write(context);
            droneMask.Write(context);
            writer.Write(shopPortalCount);
            writer.Write(eventFlags.Length);
            for (var i = 0; i < eventFlags.Length; i++)
            {
                writer.Write(eventFlags[i]);
            }
            runRng.Write(context);
            writer.Write(SharedIndexHelpers.FromArtifact(trialArtifactIndex, context));
            ruleBook.Write(context);
            writer.Write(typedRunData?.GetType().AssemblyQualifiedName ?? "");
            if (typedRunData != null)
            {
                typedRunData.Write(context);
            }
        }
    }
}
