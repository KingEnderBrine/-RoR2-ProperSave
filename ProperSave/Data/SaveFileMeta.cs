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

        [IgnoreDataMember]
        public string FilePath
        {
            get
            {
                return string.IsNullOrEmpty(FileName) ? null : $"{ProperSave.SavesDirectory}\\{FileName}.json";
            }
        }

        public static bool operator ==(SaveFileMeta first, SaveFileMeta second)
        {
            
            if (first is null)
            {
                return second is null;
            }
            return first.Equals(second);
        }

        public static bool operator !=(SaveFileMeta first, SaveFileMeta second)
        {
            return !(first == second);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return obj is SaveFileMeta second ?
                (UserProfileId == second.UserProfileId &&
                SteamIds?.Except(second.SteamIds ?? Array.Empty<ulong>())?.Count() == 0) : false;
        }

        public override int GetHashCode()
        {
            return (SteamIds, UserProfileId).GetHashCode();
        }
    }
}
