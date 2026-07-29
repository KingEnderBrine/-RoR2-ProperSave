using ProperSave.Old.Data;
using ProperSave.Old.SaveData;
using RoR2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProperSave.Old
{
    public class SaveFile {
        [DataMember(Name = "r")]
        public RunData RunData { get; set; }
        [DataMember(Name = "t")]
        public TeamData TeamData { get; set; }
        [DataMember(Name = "ra")]
        public RunArtifactsData RunArtifactsData { get; set; }
        [DataMember(Name = "a")]
        public ArtifactsData ArtifactsData { get; set; }
        [DataMember(Name = "p")]
        public List<PlayerData> PlayersData { get; set; }
        [DataMember(Name = "md")]
        public Dictionary<string, ModdedData> ModdedData { get; set; }

        [DataMember(Name = "ch")]
        public string ContentHash { get; set; }

        [IgnoreDataMember]
        public SaveFileMetadata SaveFileMeta { get; set; }

        internal ProperSave.SaveFile Migrate()
        {
            return new ProperSave.SaveFile
            {
                ArtifactsData = ArtifactsData.Migrate(),
                TeamData = TeamData.Migrate(),
                RunArtifactsData = RunArtifactsData.Migrate(),
                ModdedData = ModdedData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Migrate()),
                ModdedObjectsData = ModdedData.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Migrate().Value),
                PlayersData = PlayersData.Select(d => d.Migrate()).ToList(),
                RunData = RunData.Migrate(),
            };
        }
    }
}
