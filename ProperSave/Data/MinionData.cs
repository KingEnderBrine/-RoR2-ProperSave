using RoR2;
using RoR2.CharacterAI;
using System.Collections;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace ProperSave.Data
{
    public class MinionData
    {
        [DataMember(Name = "mi")]
        public int masterIndex;
        [DataMember(Name = "i")]
        public InventoryData inventory;

        public MinionData(CharacterMaster master)
        {
            masterIndex = (int)master.masterIndex;
            inventory = new InventoryData(master);
        }

        //Loads minion after scene was populated 
        //so that minion's AI won't throw exceptions because it can't navigate 
        public void LoadMinion(CharacterMaster playerMaster)
        {
            SceneDirector.onPostPopulateSceneServer += SpawnMinion;

            void SpawnMinion(SceneDirector obj)
            {
                SceneDirector.onPostPopulateSceneServer -= SpawnMinion;

                var masterPrefab = MasterCatalog.GetMasterPrefab((MasterCatalog.MasterIndex)masterIndex);

                var minionGameObject = Object.Instantiate(masterPrefab);
                CharacterMaster minionMaster = minionGameObject.GetComponent<CharacterMaster>();
                minionMaster.teamIndex = TeamIndex.Player;

                //MinionOwnership
                var newOwnerMaster = playerMaster;
                if (newOwnerMaster.minionOwnership.ownerMaster != null)
                    newOwnerMaster = newOwnerMaster.minionOwnership.ownerMaster;
                minionMaster.minionOwnership.SetOwner(newOwnerMaster);

                //AIOwnership
                var aiOwnership = minionGameObject.GetComponent<AIOwnership>();
                aiOwnership.ownerMaster = playerMaster;

                BaseAI baseAI = minionGameObject.GetComponent<BaseAI>();
                baseAI.leader.gameObject = playerMaster.gameObject;

                NetworkServer.Spawn(minionGameObject);

                inventory.LoadInventory(minionMaster);
            }
        }
    }
}
