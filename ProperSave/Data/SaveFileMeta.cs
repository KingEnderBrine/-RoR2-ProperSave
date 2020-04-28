using System;
using System.Collections.Generic;
using System.Text;

namespace ProperSave.Data
{
    public class SaveFileMeta
    {
        public bool IsSingleplayer { get; set; }
        public string[] UserProfileIds { get; set; }
        public string[] SteamIds { get; set; }
    }
}
