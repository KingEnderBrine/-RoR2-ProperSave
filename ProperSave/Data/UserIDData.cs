using RoR2;
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
        [DataMember(Name = "si")]
        public byte subId;

        public UserIDData(NetworkUserId userID)
        {
            egs = userID.strValue;
            steam = userID.value;
            subId = userID.subId;
        }

        public NetworkUserId Load()
        {
            if (steam != 0L)
            {
                return new NetworkUserId(steam, subId);
            }
            if (!string.IsNullOrWhiteSpace(egs))
            {
                return new NetworkUserId(egs, subId);
            }

            return default;
        }
    }
}
