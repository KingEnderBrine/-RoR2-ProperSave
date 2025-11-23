using ProperSave.Data;
using RoR2;
using RoR2.CharacterAI;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace ProperSave.SaveData
{
    public class MinionData
    {
        [DataMember(Name = "mi")]
        public int masterIndex;

        [DataMember(Name = "m")]
        public CharacterMasterData master;

        [DataMember(Name = "dld")]
        public DevotedLemurianData devotedLemurianData;

        [DataMember(Name = "drd")]
        public DroneRepairData droneRepairData;

        internal MinionData(CharacterMaster master)
        {
            masterIndex = (int)master.masterIndex;
            this.master = new CharacterMasterData(master);
            if (master.TryGetComponent<DevotedLemurianController>(out var devotedLemurianController))
            {
                devotedLemurianData = new DevotedLemurianData(devotedLemurianController);
            }
            if (master.TryGetComponent<DroneRepairMaster>(out var droneRepairMaster))
            {
                droneRepairData = new DroneRepairData(droneRepairMaster);
            }
        }

        //Loads minion after scene was populated 
        //so that minion's AI won't throw exceptions because it can't navigate 
        internal void LoadMinion(CharacterMaster playerMaster)
        {
            SceneDirector.onPostPopulateSceneServer += SpawnMinion;

            void SpawnMinion(SceneDirector obj)
            {
                SceneDirector.onPostPopulateSceneServer -= SpawnMinion;

                var masterPrefab = MasterCatalog.GetMasterPrefab((MasterCatalog.MasterIndex)masterIndex);

                var minionGameObject = Object.Instantiate(masterPrefab);
                CharacterMaster minionMaster = minionGameObject.GetComponent<CharacterMaster>();
                minionMaster.teamIndex = TeamIndex.Player;
                master.LoadMaster(minionMaster, true);

                //MinionOwnership
                var newOwnerMaster = playerMaster;
                if (newOwnerMaster.minionOwnership.ownerMaster != null)
                    newOwnerMaster = newOwnerMaster.minionOwnership.ownerMaster;
                minionMaster.minionOwnership.SetOwner(newOwnerMaster);

                //AIOwnership
                var aiOwnership = minionGameObject.GetComponent<AIOwnership>();
                aiOwnership.ownerMaster = playerMaster;

                var baseAI = minionGameObject.GetComponent<BaseAI>();
                baseAI.leader.gameObject = playerMaster.gameObject;

                if (devotedLemurianData != null)
                {
                    var devotedLemurianController = minionMaster.GetComponent<DevotedLemurianController>();
                    devotedLemurianData.LoadData(devotedLemurianController);
                    devotedLemurianController._lemurianMaster = minionMaster;
                    devotedLemurianController._devotionInventoryController = CharacterMasterData.GetDevotionInventoryController(playerMaster);
                }

                if (droneRepairData != null)
                {
                    var droneRepairMaster = minionMaster.GetComponent<DroneRepairMaster>();
                    droneRepairData.LoadData(droneRepairMaster);
                }

                NetworkServer.Spawn(minionGameObject);
            }
        }
    }
}
