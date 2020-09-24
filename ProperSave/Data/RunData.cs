using R2API.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace ProperSave.Data
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

        [IgnoreDataMember]
        private static readonly FieldInfo onRunStartGlobalDelegate = typeof(Run).GetField("onRunStartGlobal", BindingFlags.NonPublic | BindingFlags.Static);
        
        public RunData()
        {
            var run = Run.instance;
            seed = run.seed;
            difficulty = (int)run.selectedDifficulty;

            var stopWatch = run.GetFieldValue<Run.RunStopwatch>("runStopwatch");
            isPaused = stopWatch.isPaused;
            offsetFromFixedTime = stopWatch.offsetFromFixedTime;
            fixedTime = run.fixedTime;

            stageClearCount = run.stageClearCount;
            sceneName = SceneManager.GetActiveScene().name;
            nextSceneName = run.nextStageScene.ChooseSceneName();

            shopPortalCount = run.shopPortalCount;

            itemMask = new ItemMaskData(run.availableItems);
            equipmentMask = new EquipmentMaskData(run.availableEquipment);

            runRng = ProperSave.PreStageRng;

            eventFlags = run.eventFlags.ToArray();

            var artifactController = UnityEngine.Object.FindObjectOfType<ArtifactTrialMissionController>();
            trialArtifact = artifactController?.currentArtifactIndex ?? -1;
        }

        //Upgraded copy of Run.Start
        public void LoadData()
        {
            if (ModSupport.IsSSLoaded)
            {
                ShareSuiteMapTransion();
            }
            if (trialArtifact != -1)
            {
                ArtifactTrialMissionController.trialArtifact = ArtifactCatalog.GetArtifactDef((ArtifactIndex)trialArtifact);
            }

            var instance = Run.instance;

            instance.OnRuleBookUpdated(instance.networkRuleBookComponent);

            if (NetworkServer.active)
            {
                instance.seed = seed;
                instance.selectedDifficulty = (DifficultyIndex)difficulty;
                instance.fixedTime = fixedTime;
                instance.shopPortalCount = shopPortalCount;

                var stopwatch = instance.GetFieldValue<Run.RunStopwatch>("runStopwatch");
                stopwatch.offsetFromFixedTime = offsetFromFixedTime;
                stopwatch.isPaused = isPaused;

                instance.SetFieldValue("runStopwatch", stopwatch);

                runRng.LoadData(instance);
                instance.GenerateStageRNG();
            }

            instance.SetFieldValue("allowNewParticipants", true);
            UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);

            var onlyInstancesList = NetworkUser.readOnlyInstancesList;
            for (int index = 0; index < onlyInstancesList.Count; ++index)
            {
                instance.OnUserAdded(onlyInstancesList[index]);
            }
            instance.SetFieldValue("allowNewParticipants", false);

            instance.stageClearCount = stageClearCount;
            if (NetworkServer.active)
            {
                instance.nextStageScene = SceneCatalog.GetSceneDefFromSceneName(nextSceneName);
                NetworkManager.singleton.ServerChangeScene(sceneName);
            }

            itemMask.LoadData(out instance.availableItems);
            equipmentMask.LoadData(out instance.availableEquipment);

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

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private void ShareSuiteMapTransion()
        {
            ShareSuite.MoneySharingHooks.MapTransitionActive = true;
        }
    }
}
