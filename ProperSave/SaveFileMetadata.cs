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
                UserIds = PlayerCharacterMasterController.instances
                    .Select(el => 
                        el.networkUser ? 
                            new UserIDData(el.networkUser.Network_id.steamId) :
                            LostNetworkUser.TryGetUser(el.master, out var lostNetworkUser) ?
                                new UserIDData(lostNetworkUser.userID) :
                                null)
                    .Where(el => el != null)
                    .ToArray(),
                UserProfileId = LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName,
                GameMode = Run.instance.gameModeIndex
            };
        }

        internal static SaveFileMetadata GetCurrentLobbySaveMetadata(NetworkUser exceptUser = null)
        {
            try
            {
                var users = NetworkUser.readOnlyInstancesList.Select(el => el.Network_id.steamId).ToList();
                if (exceptUser != null)
                {
                    users.Remove(exceptUser.Network_id.steamId);
                }
                if (users.Count == 0)
                {
                    return null;
                }
                var gameMode = PreGameController.instance ? PreGameController.instance.gameModeIndex : Run.instance ? Run.instance.gameModeIndex : GameModeIndex.Invalid;
                if (gameMode == GameModeIndex.Invalid)
                {
                    return null;
                }
                if (users.Count == 1)
                {
                    var profile = LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName.Replace(".xml", "");
                    return SavesMetadata.FirstOrDefault(el => el.UserProfileId == profile && el.UserIds.Length == 1 && el.UserIds[0]?.Load() == users[0] && el.GameMode == gameMode);
                }
                return SavesMetadata.FirstOrDefault(el =>
                {
                    if (el.UserIds.Length != users.Count || el.GameMode != gameMode)
                    {
                        return false;
                    }
                    return users.DifferenceCount(el.UserIds.Select(e => e?.Load() ?? default)) == 0;
                });
            }
            catch (Exception ex)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("Couldn't get save metadata for current lobby");
                ProperSavePlugin.InstanceLogger.LogError(ex.ToString());
                return null;
            }
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
