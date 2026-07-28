using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProperSave.Data;

namespace ProperSave.Utils
{
    public class ReaderContext
    {
        public BinaryReader Reader { get; set; }
        public string[] SharedStrings { get; set; }
        public TypeData[] Types { get; set; }
        public (object obj, TypeData type)[] Objects { get; set; }
        public bool Resilient { get; set; }
        public int Version { get; set; }
    }
}
