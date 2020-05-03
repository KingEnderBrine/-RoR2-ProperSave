using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class SaveData {
        [DataMember(Name = "r")]
        public RunData RunData { get; set; }
        [DataMember(Name = "t")]
        public TeamData TeamData { get; set; }
        [DataMember(Name = "a")]
        public ArtifactsData ArtifactsData { get; set; }
        [DataMember(Name = "p")]
        public List<PlayerData> PlayersData { get; set; }

        [IgnoreDataMember()]
        public SaveFileMeta SaveFileMeta { get; set; }

        public SaveData() {
            RunData = new RunData();
            TeamData = new TeamData();
            ArtifactsData = ProperSave.RunArtifactData;
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

        public SaveFileMeta CreateMetadata()
        {
            return new SaveFileMeta
            {
                SteamIds = NetworkUser.readOnlyInstancesList.Select(el => el.Network_id.steamId.value).ToArray(),
                UserProfileId = LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName
            };
        }
    }
}
