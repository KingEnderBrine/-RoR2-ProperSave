using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using ProperSave.SaveData;
using ProperSave.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProperSave.Data
{
    public class CharacterMasterData
    {
        public BodyIndex bodyIndex;
        public uint money;
        public InventoryData inventory;
        public LoadoutData loadout;
        public uint voidCoins;
        public RngData cloverVoidRng;
        public InventoryData devotionInventory;
        public ulong beadExperience;
        public int numberOfBeadStatsGained;
        public uint oldBeadLevel;
        public uint newBeadLevel;
        public ulong beadXPNeededForCurrentLevel;
        public uint trackedFreeUnlocks;
        public int trackedMissileCount;
        public uint extraBossMissileMoneyRemainder;
        public List<MinionData> minions = new List<MinionData>();

        internal static CharacterMasterData Create(CharacterMaster master)
        {
            var data = new CharacterMasterData();
            data.money = master.money;
            data.voidCoins = master.voidCoins;
            data.beadExperience = master.beadExperience;
            data.numberOfBeadStatsGained = master.numberOfBeadStatsGained_XPGainNerf;
            data.oldBeadLevel = master.oldBeadLevel;
            data.newBeadLevel = master.newBeadLevel;
            data.beadXPNeededForCurrentLevel = master.beadXPNeededForCurrentLevel;
            data.trackedFreeUnlocks = master.trackedFreeUnlocks;
            data.trackedMissileCount = master.trackedMissileCount;
            data.extraBossMissileMoneyRemainder = master.ExtraBossMissileMoneyRemainder;

            data.inventory = InventoryData.Create(master.inventory);
            data.loadout = LoadoutData.Create(master.loadout);
            
            data.bodyIndex = master.bodyPrefab.GetComponent<CharacterBody>().bodyIndex;

            if (master.cloverVoidRng != null)
            {
                data.cloverVoidRng = RngData.Create(master.cloverVoidRng);
            }

            if (RunArtifactManager.instance.IsArtifactEnabled(CU8Content.Artifacts.Devotion))
            {
                var devotionInventoryController = GetDevotionInventoryController(master);
                if (devotionInventoryController)
                {
                    data.devotionInventory = InventoryData.Create(devotionInventoryController._devotionMinionInventory);
                }
            }

            foreach (var instance in CharacterMaster.readOnlyInstancesList)
            {
                var ownerMaster = instance.minionOwnership.ownerMaster;
                if (ownerMaster != null && ownerMaster.netId == master.netId)
                {
                    data.minions.Add(MinionData.Create(instance));
                }
            }

            return data;
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

            var body = bodyIndex;
            if (body != BodyIndex.None)
            {
                var bodyPrefab = BodyCatalog.GetBodyPrefab(body);
                if (bodyPrefab)
                {
                    master.bodyPrefab = bodyPrefab;
                }
            }
            ModCompat.LoadShareSuiteMoney(money);

            master.money = money;
            master.voidCoins = voidCoins;
            master.beadExperience = beadExperience;
            master.numberOfBeadStatsGained_XPGainNerf = numberOfBeadStatsGained;
            master.oldBeadLevel = oldBeadLevel;
            master.newBeadLevel = newBeadLevel;
            master.beadXPNeededForCurrentLevel = beadXPNeededForCurrentLevel;
            master.trackedFreeUnlocks = trackedFreeUnlocks;
            master.trackedMissileCount = trackedMissileCount;
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

        internal static CharacterMasterData Read(ReaderContext context)
        {
            var data = new CharacterMasterData();
            var reader = context.Reader;

            data.bodyIndex = SharedIndexHelpers.ResolveBody(reader.ReadInt32(), context);
            data.money = reader.ReadUInt32();
            data.inventory = InventoryData.Read(context);
            data.loadout = LoadoutData.Read(context);
            data.voidCoins = reader.ReadUInt32();
            if (reader.ReadBoolean())
            {
                data.cloverVoidRng = RngData.Read(context);
            }
            if (reader.ReadBoolean())
            {
                data.devotionInventory = InventoryData.Read(context);
            }
            data.beadExperience = reader.ReadUInt64();
            data.numberOfBeadStatsGained = reader.ReadInt32();
            data.oldBeadLevel = reader.ReadUInt32();
            data.newBeadLevel = reader.ReadUInt32();
            data.beadXPNeededForCurrentLevel = reader.ReadUInt64();
            data.trackedFreeUnlocks = reader.ReadUInt32();
            data.trackedMissileCount = reader.ReadInt32();
            data.extraBossMissileMoneyRemainder = reader.ReadUInt32();
            var minionCount = reader.ReadInt32();
            data.minions = new List<MinionData>(minionCount);
            for (var i = 0; i < minionCount; i++)
            {
                data.minions.Add(MinionData.Read(context));
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(SharedIndexHelpers.FromBody(bodyIndex, context));
            writer.Write(money);
            inventory.Write(context);
            loadout.Write(context);
            writer.Write(voidCoins);
            writer.Write(cloverVoidRng != null);
            cloverVoidRng?.Write(context);
            writer.Write(devotionInventory != null);
            devotionInventory?.Write(context);
            writer.Write(beadExperience);
            writer.Write(numberOfBeadStatsGained);
            writer.Write(oldBeadLevel);
            writer.Write(newBeadLevel);
            writer.Write(beadXPNeededForCurrentLevel);
            writer.Write(trackedFreeUnlocks);
            writer.Write(trackedMissileCount);
            writer.Write(extraBossMissileMoneyRemainder);
            writer.Write(minions.Count);
            for (var i = 0; i < minions.Count; i++)
            {
                minions[i].Write(context);
            }
        }
    }
}
