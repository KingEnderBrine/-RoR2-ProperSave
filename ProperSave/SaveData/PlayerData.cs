using ProperSave.Data;
using ProperSave.Utils;
using RoR2;
using RoR2.Stats;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProperSave.SaveData
{
    public class PlayerData
    {
        public UserIDData userId;
        public List<StatFieldData> statsFields = new List<StatFieldData>();
        public List<UnlockableIndex> statsUnlockables = new List<UnlockableIndex>();
        public uint lunarCoins;
        public float lunarCoinChanceMultiplier;
        public BodyIndex preferredBodyIndex;
        public CharacterMasterData master;

        internal static PlayerData Create(PlayerCharacterMasterController player, LostNetworkUser lostNetworkUser = null)
        {
            var data = new PlayerData();
            var networkUser = player.networkUser;

            if (lostNetworkUser != null)
            {
                data.userId = UserIDData.Create(lostNetworkUser.userID);
                data.lunarCoins = lostNetworkUser.lunarCoins;
                data.preferredBodyIndex = lostNetworkUser.bodyIndexPreference;
            }
            else
            {
                data.userId = UserIDData.Create(networkUser.id);
                data.lunarCoins = networkUser.lunarCoins;
                data.preferredBodyIndex = networkUser.bodyIndexPreference;
            }
            data.lunarCoinChanceMultiplier = player.lunarCoinChanceMultiplier;


            data.master = CharacterMasterData.Create(player.master);

            var stats = player.GetComponent<PlayerStatsComponent>().currentStats;
            for (var i = 0; i < stats.fields.Length; i++)
            {
                var field = stats.fields[i];
                if (!field.IsDefault())
                {
                    data.statsFields.Add(new StatFieldData
                    {
                        index = field.statDef.index,
                        value = field.ulongValue,
                    });
                }
            }
            for (var i = 0; i < stats.unlockables.Length; i++)
            {
                var unlockableIndex = stats.GetUnlockableIndex(i);
                data.statsUnlockables.Add(unlockableIndex);
            }

            return data;
        }

        internal void LoadPlayer(NetworkUser player)
        {
            master.LoadMaster(player.master, false);

            if (preferredBodyIndex != BodyIndex.None)
            {
                player.SetBodyPreference(preferredBodyIndex);
            }
            player.masterController.lunarCoinChanceMultiplier = lunarCoinChanceMultiplier;
            var stats = player.masterController.GetComponent<PlayerStatsComponent>().currentStats;
            foreach (var statField in statsFields)
            {
                var statIndex = statField.index;
                if (statIndex == -1)
                {
                    continue;
                }
                stats.fields[statIndex].ulongValue = statField.value;
            }
            foreach (var unlockableIndex in statsUnlockables)
            {
                if (unlockableIndex == UnlockableIndex.None)
                {
                    continue;
                }

                stats.AddUnlockable(unlockableIndex);
            }
        }

        internal static PlayerData Read(ReaderContext context)
        {
            var data = new PlayerData();
            var reader = context.Reader;

            data.userId = UserIDData.Read(context);
            var statsFieldsCount = reader.ReadInt32();
            data.statsFields = new List<StatFieldData>(statsFieldsCount);
            for (var i = 0; i < statsFieldsCount; i++)
            {
                data.statsFields[i] = StatFieldData.Read(context);
            }
            var statsUnlockablesCount = reader.ReadInt32();
            data.statsUnlockables = new List<UnlockableIndex>(statsUnlockablesCount);
            for (var i = 0; i < statsUnlockablesCount; i++)
            {
                data.statsUnlockables[i] = SharedIndexHelpers.ResolveUnlockable(reader.ReadInt32(), context);
            }
            data.lunarCoins = reader.ReadUInt32();
            data.lunarCoinChanceMultiplier = reader.ReadSingle();
            data.preferredBodyIndex = SharedIndexHelpers.ResolveBody(reader.ReadInt32(), context);
            data.master = CharacterMasterData.Read(context);

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;
            userId.Write(context);
            writer.Write(statsFields.Count);
            for (var i = 0; i < statsFields.Count; i++)
            {
                statsFields[i].Write(context);
            }
            writer.Write(statsUnlockables.Count);
            for (var i = 0; i < statsUnlockables.Count; i++)
            {
                writer.Write(SharedIndexHelpers.FromUnlockable(statsUnlockables[i], context));
            }
            writer.Write(lunarCoins);
            writer.Write(lunarCoinChanceMultiplier);
            writer.Write(SharedIndexHelpers.FromBody(preferredBodyIndex, context));
            master.Write(context);
        }
    }
}
