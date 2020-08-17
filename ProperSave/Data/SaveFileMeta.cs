using RoR2;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class SaveFileMeta
    {
        [DataMember(Name = "fn")]
        public string FileName { get; set; }
        [DataMember(Name = "upi")]
        public string UserProfileId { get; set; }
        [DataMember(Name = "si")]
        public ulong[] SteamIds { get; set; }
        [DataMember(Name = "gm")]
        public GameModeIndex GameMode { get; set; } = 0;

        [IgnoreDataMember]
        public string FilePath
        {
            get
            {
                return string.IsNullOrEmpty(FileName) ? null : $"{ProperSave.SavesDirectory}\\{FileName}.json";
            }
        }

        public static SaveFileMeta CreateCurrentMetadata()
        {
            return new SaveFileMeta
            {
                SteamIds = NetworkUser.readOnlyInstancesList.ToArray().Select(el => el.Network_id.steamId.value).ToArray(),
                UserProfileId = LocalUserManager.readOnlyLocalUsersList[0].userProfile.fileName,
                GameMode = Run.instance.gameModeIndex
            };
        }
    }
}
