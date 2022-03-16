using RoR2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using PSTinyJson;
using ProperSave.Data;

namespace ProperSave
{
    public class SaveFileMetadata
    {
        [DataMember(Name = "fn")]
        public string FileName { get; set; }
        [DataMember(Name = "upi")]
        public string UserProfileId { get; set; }
        [DataMember(Name = "si")]
        public UserIDData[] UserIds { get; set; }
        [DataMember(Name = "gm")]
        public GameModeIndex GameMode { get; set; } = 0;

        [IgnoreDataMember]
        public string FilePath
        {
            get
            {
                return string.IsNullOrEmpty(FileName) ? null : $"{ProperSavePlugin.SavesDirectory}\\{FileName}.json";
            }
        }

        private static List<SaveFileMetadata> SavesMetadata { get; } = new List<SaveFileMetadata>();
        
        internal static SaveFileMetadata CreateMetadataForCurrentLobby()
        {
            return new SaveFileMetadata
            {
                UserIds = NetworkUser.readOnlyInstancesList.ToArray().Select(el => new UserIDData(el.Network_id.steamId)).ToArray(),
                UserProfileId = LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName,
                GameMode = Run.instance.gameModeIndex
            };
        }

        internal static SaveFileMetadata GetCurrentLobbySaveMetadata(NetworkUser exceptUser = null)
        {
            var users = NetworkUser.readOnlyInstancesList.Select(el => el.Network_id.steamId.value).ToList();
            if (exceptUser != null)
            {
                users.Remove(exceptUser.Network_id.steamId.value);
            }
            var usersCount = users.Count();
            if (usersCount == 0)
            {
                return null;
            }
            var gameMode = PreGameController.instance ? PreGameController.instance.gameModeIndex : Run.instance ? Run.instance.gameModeIndex : GameModeIndex.Invalid;
            if (gameMode == GameModeIndex.Invalid)
            {
                return null;
            }
            if (usersCount == 1)
            {
                var profile = LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName.Replace(".xml", "");
                return SavesMetadata.FirstOrDefault(el => el.UserProfileId == profile && el.UserIds.Length == 1 && el.GameMode == gameMode);
            }
            return SavesMetadata.FirstOrDefault(el => el.UserIds.DifferenceCount(users) == 0 && el.GameMode == gameMode);
        }

        internal static void PopulateSavesMetadata()
        {
            if (!Directory.Exists(ProperSavePlugin.SavesDirectory))
            {
                Directory.CreateDirectory(ProperSavePlugin.SavesDirectory);
                return;
            }
            var path = $"{ProperSavePlugin.SavesDirectory}\\SavesMetadata.json";
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                var json = File.ReadAllText(path);
                var metadata = JSONParser.FromJson<SaveFileMetadata[]>(json);
                
                SavesMetadata.Clear();
                SavesMetadata.AddRange(metadata);
            }
            catch (Exception e)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("SavesMetadata file corrupted.");
                ProperSavePlugin.InstanceLogger.LogError(e);
            }
        }


        internal static void AddIfNotExists(SaveFileMetadata metadata)
        {
            if (SavesMetadata.Contains(metadata))
            {
                return;
            }
            SavesMetadata.Add(metadata);
            UpdateSaveMetadata();
        }

        internal static void Remove(SaveFileMetadata metadata)
        {
            if (SavesMetadata.Remove(metadata))
            {
                UpdateSaveMetadata();
            }
        }

        private static void UpdateSaveMetadata()
        {
            var path = $"{ProperSavePlugin.SavesDirectory}\\SavesMetadata.json";
            if (!Directory.Exists(ProperSavePlugin.SavesDirectory))
            {
                Directory.CreateDirectory(ProperSavePlugin.SavesDirectory);
            }

            try
            {
                File.WriteAllText(path, JSONWriter.ToJson(SavesMetadata));
            }
            catch (Exception e)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("Can't update SavesMetadata file");
                ProperSavePlugin.InstanceLogger.LogError(e);
            }
        }
    }
}
