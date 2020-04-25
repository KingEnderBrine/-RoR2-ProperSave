using R2API.Utils;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace ProperSave.Data
{
    public class RunData
    {
        [DataMember(Name = "s")]
        public string seed;
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

        public RunData()
        {
            var run = Run.instance;
            seed = run.seed.ToString();
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

            eventFlags = run.GetFieldValue<HashSet<string>>("eventFlags").ToArray();

            trialArtifact = (int)(ArtifactTrialMissionController.trialArtifact?.artifactIndex ?? ArtifactIndex.None);
        }

        //Upgraded copy of Run.Start
        public void LoadData()
        {
            if (trialArtifact != -1)
            {
                ArtifactTrialMissionController.trialArtifact = ArtifactCatalog.GetArtifactDef((ArtifactIndex)trialArtifact);
            }

            var instance = Run.instance;

            instance.seed = ulong.Parse(seed);
            instance.selectedDifficulty = (DifficultyIndex)difficulty;
            instance.fixedTime = fixedTime;
            instance.shopPortalCount = shopPortalCount;

            var stopwatch = instance.GetFieldValue<Run.RunStopwatch>("runStopwatch");
            stopwatch.offsetFromFixedTime = offsetFromFixedTime;
            stopwatch.isPaused = isPaused;

            instance.SetFieldValue("runStopwatch", stopwatch);

            if (NetworkServer.active)
            {
                runRng.LoadData(instance);
                instance.InvokeMethod("GenerateStageRNG");
                typeof(Run).GetMethod("PopulateValidStages", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, null);

            }

            instance.SetFieldValue("allowNewParticipants", true);
            UnityEngine.Object.DontDestroyOnLoad(instance.gameObject);

            var onlyInstancesList = NetworkUser.readOnlyInstancesList;
            for (int index = 0; index < onlyInstancesList.Count; ++index)
            {
                instance.OnUserAdded(onlyInstancesList[index]);
            }
            instance.SetFieldValue("allowNewParticipants", false);

            if (NetworkServer.active)
            {
                instance.nextStageScene = SceneCatalog.GetSceneDefFromSceneName(nextSceneName);
                NetworkManager.singleton.ServerChangeScene(sceneName);
                ProperSave.Instance.StartCoroutine(WaitForSceneChange());
            }

            itemMask.LoadData(out instance.availableItems);
            equipmentMask.LoadData(out instance.availableEquipment);

            instance.InvokeMethod("BuildUnlockAvailability");
            instance.BuildDropTable();

            foreach (var flag in eventFlags)
            {
                instance.SetEventFlag(flag);
            }

            if (typeof(Run).GetField("onRunStartGlobal", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) is MulticastDelegate onRunStartGlobal && onRunStartGlobal != null)
            {
                foreach (var handler in onRunStartGlobal.GetInvocationList())
                {
                    handler.Method.Invoke(handler.Target, new object[] { instance });
                }
            }
        }

        private IEnumerator WaitForSceneChange()
        {
            yield return new WaitUntil(() => SceneCatalog.GetSceneDefForCurrentScene().baseSceneName == sceneName);
            Run.instance.stageClearCount = stageClearCount;
        }
    }
}
