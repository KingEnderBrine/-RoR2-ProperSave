using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProperSave.Data;
using ProperSave.Utils;
using RoR2;
using UnityEngine.SceneManagement;

namespace ProperSave
{
    public class SaveFileHeader
    {
        internal static readonly int currentVersion = 1;

        public DateTime SaveDate { get; private set; }
        public string UserProfileId { get; private set; }
        public HeaderUserData[] Users { get; private set; }
        public GameModeIndex GameMode { get; private set; }
        public string SceneName { get; private set; }
        public DifficultyIndex Difficulty { get; private set; }
        public int Time { get; private set; }
        public int StageClearCount { get; private set; }
        public string ContentHash { get; private set; }

        public void FillFromCurrentRun()
        {
            Users = PlayerCharacterMasterController.instances
                .Select(HeaderUserData.Create)
                .Where(el => el != null)
                .ToArray();
            UserProfileId = LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName;
            GameMode = Run.instance.gameModeIndex;
            ContentHash = ProperSavePlugin.ContentHash;
            Difficulty = Run.instance.selectedDifficulty;
            SceneName = SceneManager.GetActiveScene().name;
            StageClearCount = Run.instance.stageClearCount;
            var stopWatch = Run.instance.runStopwatch;
            Time = stopWatch.isPaused ? (int)stopWatch.offsetFromFixedTime : (int)(Run.instance.fixedTime + stopWatch.offsetFromFixedTime);
        }

        internal static SaveFileHeader Read(BinaryReader reader)
        {
            var header = new SaveFileHeader();

            var version = reader.ReadInt32();
            var resilient = reader.ReadBoolean();

            var sharedStringsOffset = reader.ReadInt32();
            var currentOffset = reader.BaseStream.Position;
            reader.BaseStream.Seek(sharedStringsOffset, SeekOrigin.Begin);
            var sharedStrings = new string[reader.ReadInt32()];
            for (var i = 0; i < sharedStrings.Length; i++)
            {
                sharedStrings[i] = reader.ReadString();
            }
            reader.BaseStream.Seek(currentOffset, SeekOrigin.Begin);

            var context = new ReaderContext
            {
                Reader = reader,
                Resilient = resilient,
                SharedStrings = sharedStrings,
                Version = version,
            };

            header.SaveDate = new DateTime(reader.ReadInt64(), DateTimeKind.Utc);
            header.UserProfileId = reader.ReadString();
            header.Users = new HeaderUserData[reader.ReadInt32()];
            for (var i = 0; i < header.Users.Length; i++)
            {
                header.Users[i] = HeaderUserData.Read(context);
            }
            header.GameMode = SharedIndexHelpers.ResolveGameMode(reader.ReadInt32(), context);
            header.SceneName = reader.ReadString();
            header.Difficulty = SharedIndexHelpers.ResolveDifficulty(reader.ReadInt32(), context);
            header.Time = reader.ReadInt32();
            header.StageClearCount = reader.ReadInt32();
            header.ContentHash = reader.ReadString();

            return header;
        }

        internal void Write(BinaryWriter writer)
        {
            var resilient = ProperSavePlugin.Resilient.Value;
            var context = new WriterContext
            {
                Writer = writer,
                SharedStrings = new List<string>(),
                Resilient = resilient,
            };

            writer.Write(currentVersion);
            var currentOffset = writer.BaseStream.Position;
            writer.Write(0);

            writer.Write(DateTime.UtcNow.Ticks);
            writer.Write(UserProfileId);
            for (var i = 0; i < Users.Length; i++)
            {
                Users[i].Write(context);
            }
            writer.Write(SharedIndexHelpers.FromGameMode(GameMode, context));
            writer.Write(SceneName);
            writer.Write(SharedIndexHelpers.FromDifficulty(Difficulty, context));
            writer.Write(Time);
            writer.Write(StageClearCount);
            writer.Write(ContentHash);

            var sharedStringsOffset = writer.BaseStream.Position;
            writer.Write(context.SharedStrings.Count);
            for (var i = 0; i < context.SharedStrings.Count; i++)
            {
                writer.Write(context.SharedStrings[i]);
            }

            writer.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
            writer.Write(sharedStringsOffset);
            writer.BaseStream.Seek(writer.BaseStream.Length, SeekOrigin.Begin);
        }
    }
}
