using ProperSave.Data;
using ProperSave.SaveData;
using RoR2;
using System;
using System.Collections.Generic;
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

        public static event Action<Dictionary<string, object>> OnGatgherSaveData;

        internal SaveFile() 
        {
            RunData = new RunData();
            TeamData = new TeamData();
            RunArtifactsData = new RunArtifactsData();
            ArtifactsData = new ArtifactsData();
            PlayersData = new List<PlayerData>();

            foreach (var item in NetworkUser.readOnlyInstancesList) {
                PlayersData.Add(new PlayerData(item));
            }

            var gatheredData = new Dictionary<string, object>();
            OnGatgherSaveData?.Invoke(gatheredData);

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
                var player = players.FirstOrDefault(el => el.steamId == user.Network_id.steamId.value);

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
