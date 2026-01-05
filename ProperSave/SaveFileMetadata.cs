using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using Zio;
using System.IO;

namespace ProperSave
{
    public class SaveFileMetadata
    {
        public string FileName { get; internal set; }
        public bool ForceLoad { get; internal set; }
        public SaveFileHeader Header { get; internal set; }
        public long BodyOffset { get; internal set; }
        public SaveFile Body { get; internal set; }

        public UPath? FilePath
        {
            get
            {
                return string.IsNullOrEmpty(FileName) ? null : ProperSavePlugin.SavesPath / $"{FileName}.bin";
            }
        }

        internal static List<SaveFileMetadata> SavesMetadata { get; } = new List<SaveFileMetadata>();

        internal void FillMetadataForCurrentLobby()
        {
            ForceLoad = false;
            Header = new SaveFileHeader();
            Body = new SaveFile();
            BodyOffset = 0;

            Header.FillFromCurrentRun();
            Body.FillFromCurrentRun();

            if (FileName is null)
            {
                do
                {
                    FileName = Guid.NewGuid().ToString();
                }
                while (ProperSavePlugin.SavesFileSystem.FileExists(FilePath.Value));
            }
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
                    var profile = System.IO.Path.GetFileNameWithoutExtension(LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName);
                    return SavesMetadata.FirstOrDefault(el => el.Header.UserProfileId == profile && el.Header.Users.Length == 1 && el.Header.GameMode == gameMode);
                }

                return SavesMetadata.FirstOrDefault(el =>
                {
                    if (el.Header.Users.Length != users.Count || el.Header.GameMode != gameMode)
                    {
                        return false;
                    }
                    return users.DifferenceCount(el.Header.Users.Select(e => e?.UserId?.Load() ?? default)) == 0;
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

            SavesMetadata.Clear();
            var metadatas = new List<SaveFileMetadata>();
            foreach (var filePath in ProperSavePlugin.SavesFileSystem.EnumerateFiles(ProperSavePlugin.SavesPath, "*.bin"))
            {
                try
                {
                    var metadata = new SaveFileMetadata();
                    metadata.FileName = filePath.GetNameWithoutExtension();
                    metadata.ReadHeader();

                    metadatas.Add(metadata);
                }
                catch (Exception e)
                {
                    ProperSavePlugin.InstanceLogger.LogWarning($"Failed to load save file \"{filePath.GetName()}\"");
                    ProperSavePlugin.InstanceLogger.LogError(e);
                }
            }
            SavesMetadata.AddRange(metadatas
                .OrderByDescending(el => el.Header.ContentHash == ProperSavePlugin.ContentHash)
                .ThenByDescending(el => el.Header.SaveDate));
        }

        internal static void Replace(SaveFileMetadata metadata, SaveFileMetadata oldMetadata)
        {
            if (oldMetadata != null)
            {
                SavesMetadata.Remove(oldMetadata);
            }

            SavesMetadata.Insert(0, metadata);
        }

        internal void Write(bool resilient)
        {
            using var fileStream = ProperSavePlugin.SavesFileSystem.OpenFile(FilePath.Value, FileMode.Create, FileAccess.Write);
            using var writer = new BinaryWriter(fileStream);

            Header.Write(writer, resilient);
            Body.Write(writer, resilient);
        }

        internal void ReadBody()
        {
            if (Body != null)
            {
                return;
            }

            using var fileStream = ProperSavePlugin.SavesFileSystem.OpenFile(FilePath.Value, FileMode.Open, FileAccess.Read);
            fileStream.Seek(BodyOffset, SeekOrigin.Begin);
            using var reader = new BinaryReader(fileStream);

            Body = SaveFile.Read(reader);
        }

        internal void ReadHeader()
        {
            if (Header != null)
            {
                return;
            }

            using var fileStream = ProperSavePlugin.SavesFileSystem.OpenFile(FilePath.Value, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            Header = SaveFileHeader.Read(reader);
            BodyOffset = fileStream.Position;
            ForceLoad = false;
        }

        internal void ReadForce(string path)
        {
            using var fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(fileStream);

            Header = SaveFileHeader.Read(reader);
            BodyOffset = fileStream.Position;
            Body = SaveFile.Read(reader);
            ForceLoad = true;
        }
    }
}
