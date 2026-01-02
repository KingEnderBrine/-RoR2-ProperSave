using ProperSave.Data;
using ProperSave.Utils;
using RoR2;
using RoR2.CharacterAI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace ProperSave.SaveData
{
    public class MinionData
    {
        public MasterCatalog.MasterIndex masterIndex;

        public CharacterMasterData master;

        public DevotedLemurianData devotedLemurianData;

        public DroneRepairData droneRepairData;

        internal static MinionData Create(CharacterMaster master)
        {
            var data = new MinionData();
            data.masterIndex = master.masterIndex;
            data.master = CharacterMasterData.Create(master);
            if (master.TryGetComponent<DevotedLemurianController>(out var devotedLemurianController))
            {
                data.devotedLemurianData = DevotedLemurianData.Create(devotedLemurianController);
            }
            if (master.TryGetComponent<DroneRepairMaster>(out var droneRepairMaster))
            {
                data.droneRepairData = DroneRepairData.Create(droneRepairMaster);
            }

            return data;
        }

        //Loads minion after scene was populated 
        //so that minion's AI won't throw exceptions because it can't navigate 
        internal void LoadMinion(CharacterMaster playerMaster)
        {
            if (masterIndex == MasterCatalog.MasterIndex.none)
            {
                return;
            }
            SceneDirector.onPostPopulateSceneServer += SpawnMinion;

            void SpawnMinion(SceneDirector obj)
            {
                SceneDirector.onPostPopulateSceneServer -= SpawnMinion;

                var masterPrefab = MasterCatalog.GetMasterPrefab(masterIndex);

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

        internal static MinionData Read(ReaderContext context)
        {
            var data = new MinionData();
            var reader = context.Reader;

            data.masterIndex = SharedIndexHelpers.ResolveMaster(reader.ReadInt32(), context);
            data.master = CharacterMasterData.Read(context);
            data.devotedLemurianData = DevotedLemurianData.Read(context);
            data.droneRepairData = DroneRepairData.Read(context);

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(SharedIndexHelpers.FromMaster(masterIndex, context));
            master.Write(context);
            devotedLemurianData.Write(context);
            droneRepairData.Write(context);
        }
    }
}
