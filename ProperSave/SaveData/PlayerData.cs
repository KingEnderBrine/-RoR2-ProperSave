using ProperSave.Data;
using RoR2;
using RoR2.Stats;
using System.Runtime.Serialization;

namespace ProperSave.SaveData
{
    public class PlayerData
    {
        [DataMember(Name = "si")]
        public UserIDData userId;

        [DataMember(Name = "sf")]
        public string[] statsFields;
        [DataMember(Name = "su")]
        public int[] statsUnlockables;

        [DataMember(Name = "lc")]
        public uint lunarCoins;

        [DataMember(Name = "lccm")]
        public float lunarCoinChanceMultiplier;

        [DataMember(Name = "pbi")]
        public int preferredBodyIndex;

        [DataMember(Name = "m")]
        public CharacterMasterData master;

        internal PlayerData(PlayerCharacterMasterController player, LostNetworkUser lostNetworkUser = null) {
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
            lunarCoinChanceMultiplier = player.lunarCoinChanceMultiplier;

            preferredBodyIndex = (int)networkUser.bodyIndexPreference;

            master = new CharacterMasterData(player.master);

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
        }

        internal void LoadPlayer(NetworkUser player)
        {
            master.LoadMaster(player.master, false);

            player.SetBodyPreference((BodyIndex)preferredBodyIndex);
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
        }
    }
}
