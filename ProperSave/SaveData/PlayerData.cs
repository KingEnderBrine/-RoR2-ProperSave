using ProperSave.Data;
using RoR2;
using RoR2.Stats;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProperSave.SaveData
{
    public class PlayerData {
        [DataMember(Name = "si")]
        public UserIDData userId;

        [DataMember(Name = "m")]
        public uint money;

        [DataMember(Name = "sf")]
        public string[] statsFields;
        [DataMember(Name = "su")]
        public int[] statsUnlockables;

        [DataMember(Name = "ms")]
        public MinionData[] minions;

        [DataMember(Name = "i")]
        public InventoryData inventory;

        [DataMember(Name = "cbn")]
        public string characterBodyName;

        [DataMember(Name = "l")]
        public LoadoutData loadout;

        [DataMember(Name = "lccm")]
        public float lunarCoinChanceMultiplier;

        [DataMember(Name = "lc")]
        public uint lunarCoins;

        [DataMember(Name = "vc")]
        public uint voidCoins;

        [DataMember(Name = "cvrng")]
        public RngData cloverVoidRng;

        internal PlayerData(PlayerCharacterMasterController player, LostNetworkUser lostNetworkUser = null) {
            var master = player.master;
            var networkUser = player.networkUser;

            if (lostNetworkUser != null)
            {
                userId = new UserIDData(lostNetworkUser.userID);
                lunarCoins = lostNetworkUser.lunarCoins;
            }
            else
            {
                userId = new UserIDData(networkUser.id);
                lunarCoins = networkUser.lunarCoins;
            }

            money = master.money;
            voidCoins = master.voidCoins;
            inventory = new InventoryData(master.inventory);
            loadout = new LoadoutData(master.loadout);
            
            characterBodyName = player.master.bodyPrefab.name;
            lunarCoinChanceMultiplier = player.lunarCoinChanceMultiplier;

            var tmpMinions = new List<MinionData>();
            foreach (var instance in CharacterMaster.readOnlyInstancesList)
            {
                var ownerMaster = instance.minionOwnership.ownerMaster;
                if (ownerMaster != null && ownerMaster.netId == player.master.netId)
                {
                    tmpMinions.Add(new MinionData(instance));
                }
            }
            minions = new MinionData[tmpMinions.Count];
            for (var i = 0; i < tmpMinions.Count; i++)
            {
                minions[i] = tmpMinions[i];
            }

            var stats = player.GetComponent<PlayerStatsComponent>().currentStats;
            statsFields = new string[stats.fields.Length];
            for (var i = 0; i < stats.fields.Length; i++)
            {
                var field = stats.fields[i];
                statsFields[i] = field.ToString();
            }
            statsUnlockables = new int[stats.GetUnlockableCount()];
            for (var i = 0; i < stats.GetUnlockableCount(); i++)
            {
                var unlockable = stats.GetUnlockableIndex(i);
                statsUnlockables[i] = (int)unlockable;
            }

            if (master.cloverVoidRng != null)
            {
                cloverVoidRng = new RngData(master.cloverVoidRng);
            }
        }

        internal void LoadPlayer(NetworkUser player) {
            var master = player.master;
            foreach(var minion in minions)
            {
                minion.LoadMinion(master);
            }

            var bodyPrefab = BodyCatalog.FindBodyPrefab(characterBodyName);

            master.bodyPrefab = bodyPrefab;

            loadout.LoadData(master.loadout);

            inventory.LoadInventory(master.inventory);

            ModSupport.LoadShareSuiteMoney(money);

            master.money = money;
            master.voidCoins = voidCoins;

            player.masterController.lunarCoinChanceMultiplier = lunarCoinChanceMultiplier;
            var stats = player.masterController.GetComponent<PlayerStatsComponent>().currentStats;
            for (var i = 0; i < statsFields.Length; i++)
            {
                var fieldValue = statsFields[i];
                stats.SetStatValueFromString(StatDef.allStatDefs[i], fieldValue);
            }
            for (var i = 0; i < statsUnlockables.Length; i++)
            {
                var unlockableIndex = statsUnlockables[i];
                stats.AddUnlockable((UnlockableIndex)unlockableIndex);
            }

            cloverVoidRng?.LoadDataOut(out master.cloverVoidRng);
        }
    }
}
