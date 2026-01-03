using ProperSave.Old.Data;
using ProperSave.Old.SaveData.Runs;
using ProperSave.TinyJson;
using RoR2;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace ProperSave.Old.SaveData
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

        internal ProperSave.SaveData.RunData Migrate()
        {
            return new ProperSave.SaveData.RunData
            {
                difficultyIndex = (DifficultyIndex)difficulty,
                droneMask = droneMask.Migrate(),
                equipmentMask = equipmentMask.Migrate(),
                eventFlags = eventFlags,
                fixedTime = fixedTime,
                isPaused = isPaused,
                itemMask = itemMask.Migrate(),
                loopClearCount = loopClearCount,
                nextSceneName = nextSceneName,
                offsetFromFixedTime = offsetFromFixedTime,
                prestigeArtifactMountainValue = prestigeArtifactMountainValue,
                previousSceneName = previousSceneName,
                ruleBook = ruleBook.Migrate(),
                runRng = runRng.Migrate(),
                sceneName = sceneName,
                seed = seed,
                shopPortalCount = shopPortalCount,
                stageClearCount = stageClearCount,
                stageClearCountAtLoopStart = stageClearCountAtLoopStart,
                time = time,
                trialArtifactIndex = (ArtifactIndex)trialArtifact,
                typedRunData = typedRunData?.Migrate(),
            };
        }
    }
}
