using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PSTinyJson;
using ProperSave.Old.Data;
using Zio;
using System.IO;

namespace ProperSave.Old
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

        internal static void MigrateAll()
        {
            if (!ProperSavePlugin.SavesFileSystem.DirectoryExists(ProperSavePlugin.SavesPath))
            {
                return;
            }

            var path = ProperSavePlugin.SavesPath / "SavesMetadata.json";
            if (!ProperSavePlugin.SavesFileSystem.FileExists(path))
            {
                return;
            }

            ProperSavePlugin.InstanceLogger.LogInfo("Found Saves.Metadata.json, migrating to new format");

            try
            {
                var json = ProperSavePlugin.SavesFileSystem.ReadAllText(path);
                var metadatas = JSONParser.FromJson<SaveFileMetadata[]>(json);

                foreach (var metadata in metadatas)
                {
                    if (!ProperSavePlugin.SavesFileSystem.FileExists(metadata.FilePath.Value))
                    {
                        ProperSavePlugin.InstanceLogger.LogWarning($"Save file {metadata.FileName} doesn't exist and will not be migrated");
                        continue;
                    }


                    try
                    {
                        var filejson = ProperSavePlugin.SavesFileSystem.ReadAllText(metadata.FilePath.Value);
                        filejson = filejson.Replace("ProperSave.SaveData", "ProperSave.Old.SaveData");
                        filejson = filejson.Replace("ProperSave.Data", "ProperSave.Old.Data");

                        var oldSaveFile = JSONParser.FromJson<SaveFile>(filejson);
                        var newSaveFile = oldSaveFile.Migrate();
                        var newMetadata = new ProperSave.SaveFileMetadata
                        {
                            FileName = metadata.FileName,
                            Header = new ProperSave.SaveFileHeader
                            {
                                ContentHash = oldSaveFile.ContentHash,
                                Difficulty = (DifficultyIndex)oldSaveFile.RunData.difficulty,
                                GameMode = metadata.GameMode,
                                SaveDate = ProperSavePlugin.SavesFileSystem.GetLastWriteTime(metadata.FilePath.Value),
                                SceneName = oldSaveFile.RunData.sceneName,
                                StageClearCount = oldSaveFile.RunData.stageClearCount,
                                Time = oldSaveFile.RunData.isPaused
                                    ? (int)oldSaveFile.RunData.offsetFromFixedTime
                                    : (int)(oldSaveFile.RunData.fixedTime + oldSaveFile.RunData.offsetFromFixedTime),
                                UserProfileId = metadata.UserProfileId,
                                Users = oldSaveFile.PlayersData.Select(d => new ProperSave.Data.HeaderUserData
                                {
                                    Body = BodyCatalog.FindBodyIndex(d.master.bodyName),
                                    UserId = d.userId.Migrate(),
                                }).ToArray(),
                            },
                            Body = newSaveFile
                        };

                        newMetadata.Write(false);
                    }
                    catch (Exception e)
                    {
                        ProperSavePlugin.InstanceLogger.LogWarning($"Failed to migrate {metadata.FileName}");
                        ProperSavePlugin.InstanceLogger.LogError(e);
                    }
                }

                foreach (var file in ProperSavePlugin.SavesFileSystem.EnumerateFiles(ProperSavePlugin.SavesPath, "*.json"))
                {
                    ProperSavePlugin.SavesFileSystem.DeleteFile(file);
                }
            }
            catch (Exception e)
            {
                ProperSavePlugin.InstanceLogger.LogWarning("An error occurred during migration");
                ProperSavePlugin.InstanceLogger.LogError(e);
            }
        }
    }
}
