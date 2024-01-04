﻿using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PSTinyJson;
using ProperSave.Data;
using Zio;

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
        public GameModeIndex GameMode { get; set; }

        [IgnoreDataMember]
        public UPath? FilePath
        {
            get
            {
                return string.IsNullOrEmpty(FileName) ? null : ProperSavePlugin.SavesPath / $"{FileName}.json";
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
                            new UserIDData(el.networkUser.id) :
                            LostNetworkUser.TryGetUser(el.master, out var lostNetworkUser) ?
                                new UserIDData(lostNetworkUser.userID) :
                                null)
                    .Where(el => el != null)
                    .ToArray(),
                UserProfileId = LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName,
                GameMode = Run.instance.gameModeIndex,
            };
        }

        internal static SaveFileMetadata GetCurrentLobbySaveMetadata(NetworkUser exceptUser = null)
        {
            try
            {
                var users = NetworkUser.readOnlyInstancesList.Select(el => el.id).ToList();
                if (exceptUser != null)
                {
                    users.Remove(exceptUser.id);
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
                    return SavesMetadata.FirstOrDefault(el => el.UserProfileId == profile && el.UserIds.Length == 1 && (el.UserIds[0]?.Load().Equals(users[0]) ?? false) && el.GameMode == gameMode);
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
            if (!ProperSavePlugin.SavesFileSystem.DirectoryExists(ProperSavePlugin.SavesPath))
            {
                ProperSavePlugin.SavesFileSystem.CreateDirectory(ProperSavePlugin.SavesPath);
                return;
            }

            var path = ProperSavePlugin.SavesPath / "SavesMetadata.json";
            if (!ProperSavePlugin.SavesFileSystem.FileExists(path))
            {
                return;
            }

            try
            {
                var json = ProperSavePlugin.SavesFileSystem.ReadAllText(path);
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
            if (!ProperSavePlugin.SavesFileSystem.DirectoryExists(ProperSavePlugin.SavesPath))
            {
                ProperSavePlugin.SavesFileSystem.CreateDirectory(ProperSavePlugin.SavesPath);
                return;
            }

            var path = ProperSavePlugin.SavesPath / "SavesMetadata.json";
            try
            {
                ProperSavePlugin.SavesFileSystem.WriteAllText(path, JSONWriter.ToJson(SavesMetadata));
            }
            catch (Exception e)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("Can't update SavesMetadata file");
                ProperSavePlugin.InstanceLogger.LogError(e);
            }
        }
    }
}
