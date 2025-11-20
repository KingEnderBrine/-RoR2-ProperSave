using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using ProperSave.SaveData;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProperSave.Data
{
    public class CharacterMasterData
    {
        [DataMember(Name = "bn")]
        public string bodyName;

        [DataMember(Name = "m")]
        public uint money;

        [DataMember(Name = "i")]
        public InventoryData inventory;

        [DataMember(Name = "l")]
        public LoadoutData loadout;

        [DataMember(Name = "vc")]
        public uint voidCoins;

        [DataMember(Name = "cvrng")]
        public RngData cloverVoidRng;

        [DataMember(Name = "di")]
        public InventoryData devotionInventory;

        [DataMember(Name = "be")]
        public ulong beadExpirience;

        [DataMember(Name = "nobsg")]
        public int numberOfBeadStatsGained;

        [DataMember(Name = "obl")]
        public uint oldBeadLevel;

        [DataMember(Name = "nbl")]
        public uint newBeadLevel;

        [DataMember(Name = "bxpnfcl")]
        public ulong beadXPNeededForCurrentLevel;

        [DataMember(Name = "tfu")]
        public uint trackedFreeUnlocks;

        [DataMember(Name = "tmc")]
        public int trackedMissileCount;

        [DataMember(Name = "cosc")]
        public uint costOfSmallChest;

        [DataMember(Name = "ebmmr")]
        public uint extraBossMissileMoneyRemainder;

        [DataMember(Name = "ms")]
        public MinionData[] minions;

        internal CharacterMasterData(CharacterMaster master)
        {
            money = master.money;
            voidCoins = master.voidCoins;
            beadExpirience = master.beadExperience;
            numberOfBeadStatsGained = master.numberOfBeadStatsGained_XPGainNerf;
            oldBeadLevel = master.oldBeadLevel;
            newBeadLevel = master.newBeadLevel;
            beadXPNeededForCurrentLevel = master.beadXPNeededForCurrentLevel;
            trackedFreeUnlocks = master.trackedFreeUnlocks;
            trackedMissileCount = master.trackedMissileCount;
            costOfSmallChest = CharacterMaster.costOfSmallChest;
            extraBossMissileMoneyRemainder = master.ExtraBossMissileMoneyRemainder;

            inventory = new InventoryData(master.inventory);
            loadout = new LoadoutData(master.loadout);
            
            bodyName = master.bodyPrefab.name;

            if (master.cloverVoidRng != null)
            {
                cloverVoidRng = new RngData(master.cloverVoidRng);
            }

            if (RunArtifactManager.instance.IsArtifactEnabled(CU8Content.Artifacts.Devotion))
            {
                var devotionInventoryController = GetDevotionInventoryController(master);
                if (devotionInventoryController)
                {
                    devotionInventory = new InventoryData(devotionInventoryController._devotionMinionInventory);
                }
            }

            var tmpMinions = new List<MinionData>();
            foreach (var instance in CharacterMaster.readOnlyInstancesList)
            {
                var ownerMaster = instance.minionOwnership.ownerMaster;
                if (ownerMaster != null && ownerMaster.netId == master.netId)
                {
                    tmpMinions.Add(new MinionData(instance));
                }
            }
            minions = new MinionData[tmpMinions.Count];
            for (var i = 0; i < tmpMinions.Count; i++)
            {
                minions[i] = tmpMinions[i];
            }
        }
        
        internal void LoadMaster(CharacterMaster master, bool delayedInventory)
        {
            if (devotionInventory != null)
            {
                var devotionInventoryController = CreateDevotionInventoryController(master);
                devotionInventory.LoadInventory(devotionInventoryController._devotionMinionInventory);
            }

            foreach (var minion in minions)
            {
                minion.LoadMinion(master);
            }

            var bodyPrefab = BodyCatalog.FindBodyPrefab(bodyName);

            if (bodyPrefab)
            {
                master.bodyPrefab = bodyPrefab;
            }

            ModSupport.LoadShareSuiteMoney(money);

            master.money = money;
            master.voidCoins = voidCoins;
            master.beadExperience = beadExpirience;
            master.numberOfBeadStatsGained_XPGainNerf = numberOfBeadStatsGained;
            master.oldBeadLevel = oldBeadLevel;
            master.newBeadLevel = newBeadLevel;
            master.beadXPNeededForCurrentLevel = beadXPNeededForCurrentLevel;
            master.trackedFreeUnlocks = trackedFreeUnlocks;
            master.trackedMissileCount = trackedMissileCount;
            CharacterMaster.costOfSmallChest = costOfSmallChest;
            master.ExtraBossMissileMoneyRemainder = extraBossMissileMoneyRemainder;

            loadout.LoadData(master.loadout);

            cloverVoidRng?.LoadDataOut(out master.cloverVoidRng);
            if (delayedInventory)
            {
                master.StartCoroutine(LoadInventoryCoroutine(master, inventory));
            }
            else
            {
                inventory.LoadInventory(master.inventory);
            }
        }

        internal static DevotionInventoryController GetDevotionInventoryController(CharacterMaster master)
        {
            foreach (var controller in DevotionInventoryController.InstanceList)
            {
                if (controller.SummonerMaster == master)
                {
                    return controller;
                }
            }

            return null;
        }

        private static DevotionInventoryController CreateDevotionInventoryController(CharacterMaster master)
        {
            var prefab = Addressables.LoadAssetAsync<GameObject>("RoR2/CU8/LemurianEgg/DevotionMinionInventory.prefab").WaitForCompletion();
            var gameObject = GameObject.Instantiate(prefab);

            var inventoryController = gameObject.GetComponent<DevotionInventoryController>();
            inventoryController.GetComponent<TeamFilter>().teamIndex = TeamIndex.Player;
            inventoryController._summonerMaster = master;

            NetworkServer.Spawn(gameObject);

            return inventoryController;
        }

        private static IEnumerator LoadInventoryCoroutine(CharacterMaster minionMaster, InventoryData inventory)
        {
            //Waiting 2 frames for game to give items in some components Start to override them
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            inventory.LoadInventory(minionMaster.inventory);
        }
    }
}
