using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProperSave.Utils
{
    public class ReaderContext
    {
        public BinaryReader Reader { get; set; }
        public string[] SharedStrings { get; set; }
        public bool Resilient { get; set; }
        public int Version { get; set; }
    }
}
