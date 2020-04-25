using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ProperSave.Data {
    [Serializable]
    public class SaveData {
        [DataMember(Name = "r")]
        public RunData RunData { get; set; }
        [DataMember(Name = "t")]
        public TeamData TeamData { get; set; }
        [DataMember(Name = "a")]
        public ArtifactsData ArtifactsData { get; set; }
        [DataMember(Name = "p")]
        public List<PlayerData> PlayersData { get; set; }

        public SaveData() {
            RunData = new RunData();
            TeamData = new TeamData();
            ArtifactsData = new ArtifactsData();
            PlayersData = new List<PlayerData>();

            foreach (var item in NetworkUser.readOnlyInstancesList) {
                PlayersData.Add(new PlayerData(item));
            }
        }

        public void LoadRun()
        {
            RunData.LoadData();
        }

        public void LoadArtifacts()
        {
            ArtifactsData.LoadData();
        }

        public void LoadTeam()
        {
            TeamData.LoadData();
        }

        public void LoadPlayers() 
        {
            foreach (var item in PlayersData) {
                item.LoadPlayer();
            }
        }
    }
}
