using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ProperSave.Old.Data
{
    public class UserIDData
    {
        [DataMember(Name = "s")]
        public ulong steam;
        [DataMember(Name = "e")]
        public string egs;
        [DataMember(Name = "si")]
        public byte subId;

        internal ProperSave.Data.UserIDData Migrate()
        {
            return new ProperSave.Data.UserIDData
            {
                steam = steam,
                subId = subId,
                egs = egs,
            };
        }
    }
}
