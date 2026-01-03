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

        public DateTime SaveDate { get; internal set; }
        public string UserProfileId { get; internal set; }
        public HeaderUserData[] Users { get; internal set; }
        public GameModeIndex GameMode { get; internal set; }
        public string SceneName { get; internal set; }
        public DifficultyIndex Difficulty { get; internal set; }
        public int Time { get; internal set; }
        public int StageClearCount { get; internal set; }
        public string ContentHash { get; internal set; }

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

            var sharedStringsOffset = reader.ReadInt64();
            var currentOffset = reader.BaseStream.Position;
            reader.BaseStream.Seek(sharedStringsOffset, SeekOrigin.Begin);
            var sharedStrings = new string[reader.ReadInt32()];
            for (var i = 0; i < sharedStrings.Length; i++)
            {
                sharedStrings[i] = reader.ReadString();
            }
            var endOffset = reader.BaseStream.Position;
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

            reader.BaseStream.Seek(endOffset, SeekOrigin.Begin);
            return header;
        }

        internal void Write(BinaryWriter writer, bool resilient)
        {
            var context = new WriterContext
            {
                Writer = writer,
                SharedStrings = new List<string>(),
                Resilient = resilient,
            };

            writer.Write(currentVersion);
            writer.Write(resilient);
            var currentOffset = writer.BaseStream.Position;
            writer.Write(0L);

            writer.Write(DateTime.UtcNow.Ticks);
            writer.Write(UserProfileId);
            writer.Write(Users.Length);
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
