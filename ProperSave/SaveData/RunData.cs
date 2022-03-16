using ProperSave.Data;
using ProperSave.SaveData.Runs;
using ProperSave.TinyJson;
using RoR2;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        [DataMember(Name = "ip")]
        public bool isPaused;
        [DataMember(Name = "offt")]
        public float offsetFromFixedTime;
        [DataMember(Name = "scc")]
        public int stageClearCount;
        [DataMember(Name = "sn")]
        public string sceneName;
        [DataMember(Name = "nsn")]
        public string nextSceneName;

        [DataMember(Name = "im")]
        public ItemMaskData itemMask;
        [DataMember(Name = "em")]
        public EquipmentMaskData equipmentMask;
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

            stageClearCount = run.stageClearCount;
            sceneName = SceneManager.GetActiveScene().name;
            nextSceneName = run.nextStageScene.cachedName;

            shopPortalCount = run.shopPortalCount;

            itemMask = new ItemMaskData(run.availableItems);
            equipmentMask = new EquipmentMaskData(run.availableEquipment);

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

            instance.SetRuleBook(ruleBook.Load());

            if (NetworkServer.active)
            {
                instance.seed = seed;
                instance.selectedDifficulty = (DifficultyIndex)difficulty;
                instance.fixedTime = fixedTime;
                instance.shopPortalCount = shopPortalCount;

                instance.runStopwatch = new Run.RunStopwatch
                {
                    offsetFromFixedTime = offsetFromFixedTime,
                    isPaused = isPaused
                };

                runRng.LoadData(instance);
                instance.GenerateStageRNG();
                typedRunData?.Load();
            }

            instance.allowNewParticipants = true;
            UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);

            var onlyInstancesList = NetworkUser.readOnlyInstancesList;
            for (int index = 0; index < onlyInstancesList.Count; ++index)
            {
                instance.OnUserAdded(onlyInstancesList[index]);
            }
            instance.allowNewParticipants = false;

            instance.stageClearCount = stageClearCount;
            if (NetworkServer.active)
            {
                instance.nextStageScene = SceneCatalog.GetSceneDefFromSceneName(nextSceneName);
                NetworkManager.singleton.ServerChangeScene(sceneName);
            }

            itemMask.LoadDataOut(out instance.availableItems);
            equipmentMask.LoadDataOut(out instance.availableEquipment);

            instance.BuildUnlockAvailability();
            instance.BuildDropTable();

            foreach (var flag in eventFlags)
            {
                instance.SetEventFlag(flag);
            }

            if (onRunStartGlobalDelegate.GetValue(null) is MulticastDelegate onRunStartGlobal && onRunStartGlobal != null)
            {
                foreach (var handler in onRunStartGlobal.GetInvocationList())
                {
                    handler.Method.Invoke(handler.Target, new object[] { instance });
                }
            }
        }
    }
}
