using ProperSave.Data;
using ProperSave.SaveData;
using ProperSave.Utils;
using PSTinyJson;
using RoR2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProperSave
{
    public class SaveFile
    {
        internal static readonly int currentVersion = 1;

        public RunData RunData { get; set; }
        public TeamData TeamData { get; set; }
        public RunArtifactsData RunArtifactsData { get; set; }
        public ArtifactsData ArtifactsData { get; set; }
        public List<PlayerData> PlayersData { get; set; } = new List<PlayerData>();
        public Dictionary<string, ModdedData> ModdedData { get; set; } = new Dictionary<string, ModdedData>();

        public static event Action<Dictionary<string, object>> OnGatherSaveData;

        internal SaveFile() { }

        internal void FillFromCurrentRun() 
        {
            RunData = RunData.Create();
            TeamData = TeamData.Create();
            RunArtifactsData = RunArtifactsData.Create();
            ArtifactsData = ArtifactsData.Create();

            foreach (var item in PlayerCharacterMasterController.instances) {
                LostNetworkUser lostUser = null;
                if (!item.networkUser && !LostNetworkUser.TryGetUser(item.master, out lostUser))
                {
                    continue;
                }
                PlayersData.Add(PlayerData.Create(item, lostUser));
            }

            var gatheredData = new Dictionary<string, object>();
            var invocationList = OnGatherSaveData?.GetInvocationList();
            if (invocationList != null)
            {
                foreach (var invocation in invocationList)
                {
                    try
                    {
                        ((Action<Dictionary<string, object>>)invocation)(gatheredData);
                    }
                    catch (Exception ex)
                    {
                        ProperSavePlugin.InstanceLogger.LogError(ex);
                    }
                }
            }

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
            try
            {
                //Hopefully temporary workaround for Conduit Canyon's preplaced teleporter throwing NRE in Awake if when loaded.
                LegacyResourcesAPI.Load<GameObject>("Prefabs/PositionIndicators/TeleporterChargingPositionIndicator", true);
            }
            catch { }

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
            if (NetworkUser.readOnlyInstancesList.Count == 1) 
            {
                var player = PlayersData.FirstOrDefault();
                if (player == null)
                {
                    return;
                }

                var user = NetworkUser.readOnlyInstancesList.FirstOrDefault();
                player.LoadPlayer(user);
                return;
            }

            var players = PlayersData.ToList();
            foreach (var user in NetworkUser.readOnlyInstancesList) {
                var player = players.FirstOrDefault(el => el.userId.Load().Equals(user.id));
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

        internal static SaveFile Read(BinaryReader reader)
        {
            var saveFile = new SaveFile();

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

            saveFile.ArtifactsData = ArtifactsData.Read(context);
            saveFile.RunData = RunData.Read(context);
            saveFile.RunArtifactsData = RunArtifactsData.Read(context);
            saveFile.TeamData = TeamData.Read(context);
            var playersCount = reader.ReadInt32();
            for (var i = 0; i < playersCount; i++)
            {
                saveFile.PlayersData.Add(PlayerData.Read(context));
            }

            saveFile.ModdedData = ReadModdedData(context);

            reader.BaseStream.Seek(endOffset, SeekOrigin.Begin);

            return saveFile;
        }

        private static Dictionary<string, ModdedData> ReadModdedData(ReaderContext context)
        {
            return JSONParser.FromJson<Dictionary<string, ModdedData>>(context.Reader.ReadString());
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

            ArtifactsData.Write(context);
            RunData.Write(context);
            RunArtifactsData.Write(context);
            TeamData.Write(context);
            writer.Write(PlayersData.Count);
            for (var i = 0; i < PlayersData.Count; i++)
            {
                PlayersData[i].Write(context);
            }

            WriteModdedData(context);

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

        private void WriteModdedData(WriterContext context)
        {
            context.Writer.Write(ModdedData.ToJson());
        }
    }
}
