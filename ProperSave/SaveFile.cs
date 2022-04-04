using ProperSave.Data;
using ProperSave.SaveData;
using RoR2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave
{
    public class SaveFile {
        [DataMember(Name = "r")]
        public RunData RunData { get; set; }
        [DataMember(Name = "t")]
        public TeamData TeamData { get; set; }
        [DataMember(Name = "ra")]
        public RunArtifactsData RunArtifactsData { get; set; }
        [DataMember(Name = "a")]
        public ArtifactsData ArtifactsData { get; set; }
        [DataMember(Name = "p")]
        public List<PlayerData> PlayersData { get; set; }
        [DataMember(Name = "md")]
        public Dictionary<string, ModdedData> ModdedData { get; set; }

        [IgnoreDataMember]
        public SaveFileMetadata SaveFileMeta { get; set; }

        public static event Action<Dictionary<string, object>> OnGatherSaveData;
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use OnGatherSaveData without a typo", true)]
        public static event Action<Dictionary<string, object>> OnGatgherSaveData
        {
            add => OnGatherSaveData += value;
            remove => OnGatherSaveData -= value;
        }

        internal SaveFile() 
        {
            RunData = new RunData();
            TeamData = new TeamData();
            RunArtifactsData = new RunArtifactsData();
            ArtifactsData = new ArtifactsData();
            PlayersData = new List<PlayerData>();

            foreach (var item in PlayerCharacterMasterController.instances) {
                LostNetworkUser lostUser = null;
                if (!item.networkUser && !LostNetworkUser.TryGetUser(item.master, out lostUser))
                {
                    continue;
                }
                PlayersData.Add(new PlayerData(item, lostUser));
            }

            var gatheredData = new Dictionary<string, object>();
            OnGatherSaveData?.Invoke(gatheredData);

            ModdedData = gatheredData.ToDictionary(
                el => el.Key, 
                el => new ModdedData 
                { 
                    ObjectType = el.Value.GetType().AssemblyQualifiedName, 
                    Value = el.Value 
                });
        }

        internal void LoadRun()
        {
            RunData.LoadData();
        }

        internal void LoadArtifacts()
        {
            RunArtifactsData.LoadData();
            ArtifactsData.LoadData();
        }

        internal void LoadTeam()
        {
            TeamData.LoadData();
        }

        internal void LoadPlayers() 
        {
            var players = PlayersData.ToList();
            foreach (var user in NetworkUser.readOnlyInstancesList) {
                var player = players.FirstOrDefault(el => el.userId.Load() == user.Network_id.steamId);

                if (player == null)
                {
                    continue;
                }

                players.Remove(player);
                player.LoadPlayer(user);
            }
        }

        public T GetModdedData<T>(string key)
        {
            return (T)ModdedData[key].Value;
        }
    }
}
