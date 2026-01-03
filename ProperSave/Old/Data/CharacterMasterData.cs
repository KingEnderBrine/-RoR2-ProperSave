using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using ProperSave.Old.SaveData;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace ProperSave.Old.Data
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

        [DataMember(Name = "ebmmr")]
        public uint extraBossMissileMoneyRemainder;

        [DataMember(Name = "ms")]
        public MinionData[] minions;

        internal ProperSave.Data.CharacterMasterData Migrate()
        {
            return new ProperSave.Data.CharacterMasterData
            {
                beadExperience = beadExpirience,
                beadXPNeededForCurrentLevel = beadXPNeededForCurrentLevel,
                bodyIndex = BodyCatalog.FindBodyIndex(bodyName),
                cloverVoidRng = cloverVoidRng?.Migrate(),
                devotionInventory = devotionInventory?.Migrate(),
                extraBossMissileMoneyRemainder = extraBossMissileMoneyRemainder,
                inventory = inventory.Migrate(),
                loadout = loadout.Migrate(),
                minions = minions.Select(m => m.Migrate()).ToList(),
                money = money,
                newBeadLevel = newBeadLevel,
                numberOfBeadStatsGained = numberOfBeadStatsGained,
                oldBeadLevel = oldBeadLevel,
                trackedFreeUnlocks = trackedFreeUnlocks,
                trackedMissileCount = trackedMissileCount,
                voidCoins = voidCoins,
            };
        }
    }
}
