using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ProperSave.Data
{
    public class UserIDData
    {
        [DataMember(Name = "s")]
        public ulong steam;
        [DataMember(Name = "e")]
        public string egs;

        public UserIDData(CSteamID userID)
        {
            egs = userID.stringValue;
            steam = userID.steamValue;
        }

        public CSteamID Load()
        {
            if (steam != 0L)
            {
                return new CSteamID(steam);
            }
            if (!string.IsNullOrWhiteSpace(egs))
            {
                return new CSteamID(egs);
            }

            return CSteamID.nil;
        }
    }
}
