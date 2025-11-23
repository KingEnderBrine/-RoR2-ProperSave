using ProperSave.Data;
using ProperSave.SaveData.Runs;
using ProperSave.TinyJson;
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
        [DataMember(Name = "s")]
        public ulong seed;
        [DataMember(Name = "d")]
        public int difficulty;
        [DataMember(Name = "ft")]
        public float fixedTime;
        [DataMember(Name = "t")]
        public float time;
        [DataMember(Name = "ip")]
        public bool isPaused;
        [DataMember(Name = "offt")]
        public float offsetFromFixedTime;
        [DataMember(Name = "scc")]
        public int stageClearCount;
        [DataMember(Name = "sccls")]
        public int stageClearCountAtLoopStart;
        [DataMember(Name = "lcc")]
        public int loopClearCount;
        [DataMember(Name = "sn")]
        public string sceneName;
        [DataMember(Name = "nsn")]
        public string nextSceneName;
        [DataMember(Name = "psn")]
        public string previousSceneName;
        [DataMember(Name = "pamv")]
        public int prestigeArtifactMountainValue;

        [DataMember(Name = "im")]
        public ItemMaskData itemMask;
        [DataMember(Name = "em")]
        public EquipmentMaskData equipmentMask;
        [DataMember(Name = "dm")]
        public DroneMaskData droneMask;
        [DataMember(Name = "spc")]
        public int shopPortalCount;
        [DataMember(Name = "ef")]
        public string[] eventFlags;
        [DataMember(Name = "rr")]
        public RunRngData runRng;
        [DataMember(Name = "ta")]
        public int trialArtifact;
        [DataMember(Name = "rb")]
        public RuleBookData ruleBook;
        [DataMember(Name = "trdt")]
        public string typeRunDataType;
        [DataMember(Name = "trd")]
        [DiscoverObjectType(nameof(typeRunDataType))]
        public ITypedRunData typedRunData;

        private static readonly FieldInfo onRunStartGlobalDelegate = typeof(Run).GetField(nameof(Run.onRunStartGlobal), BindingFlags.NonPublic | BindingFlags.Static);
        
        internal RunData()
        {
            var run = Run.instance;
            seed = run.seed;
            difficulty = (int)run.selectedDifficulty;

            var stopWatch = run.runStopwatch;
            isPaused = stopWatch.isPaused;
            offsetFromFixedTime = stopWatch.offsetFromFixedTime;
            fixedTime = run.fixedTime;
            time = run.time;

            stageClearCount = run.stageClearCount;
            stageClearCountAtLoopStart = run.stageClearCountAtLoopStart;
            loopClearCount = run._loopClearCount;
            sceneName = SceneManager.GetActiveScene().name;
            nextSceneName = run.nextStageScene.cachedName;
            previousSceneName = Saving.PreStageSceneName;

            shopPortalCount = run.shopPortalCount;
            prestigeArtifactMountainValue = run.prestiegeArtifactMountainValue;

            itemMask = new ItemMaskData(run.availableItems);
            equipmentMask = new EquipmentMaskData(run.availableEquipment);
            droneMask = new DroneMaskData(run.availableDrones);

            runRng = Saving.PreStageRng;

            eventFlags = run.eventFlags.ToArray();

            var artifactController = UnityEngine.Object.FindObjectOfType<ArtifactTrialMissionController>();
            trialArtifact = artifactController?.currentArtifactIndex ?? -1;

            ruleBook = new RuleBookData(run.ruleBook);

            if (run is InfiniteTowerRun)
            {
                typedRunData = new InfiniteTowerTypedRunData();
                typeRunDataType = typeof(InfiniteTowerTypedRunData).AssemblyQualifiedName;
            }
        }

        //Upgraded copy of Run.Start
        internal void LoadData()
        {
            ModSupport.ShareSuiteMapTransition();

            if (trialArtifact != -1)
            {
                ArtifactTrialMissionController.trialArtifact = ArtifactCatalog.GetArtifactDef((ArtifactIndex)trialArtifact);
            }

            var instance = Run.instance;

            //If ruleBook length doesn't match RuleCatalog, this means something changed in the game since the save, for example content mods added/removed
            //Logging error and keeping ruleBook from starting run, which will result in loaded game being not the same as saved
            //At least this will not block user from loading runs and potentially not being able to start a run at all
            if (ruleBook.ruleValues.Length != RuleCatalog.ruleCount)
            {
                ProperSavePlugin.InstanceLogger.LogError("RuleCatalog mismatch with saved ruleBook data, fallback to starting Run ruleBook");
            }
            else
            {
                instance.SetRuleBook(ruleBook.Load());
            }
            instance.OnRuleBookUpdated(instance.networkRuleBookComponent);

            instance.seed = seed;
            instance.selectedDifficulty = (DifficultyIndex)difficulty;
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
            //hoping that no one will want to use stage rng in onnLoopBeginServer event
            instance.GenerateStageRNG();

            NetworkManager.singleton.ServerChangeScene(sceneName);

            itemMask.LoadDataOut(out instance.availableItems);
            equipmentMask.LoadDataOut(out instance.availableEquipment);
            droneMask.LoadDataOut(out instance.availableDrones);

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
    }
}
