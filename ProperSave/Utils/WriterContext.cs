using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProperSave.Utils
{
    public class WriterContext
    {
        public BinaryWriter Writer { get; set; }
        public List<string> SharedStrings { get; set; }
        public bool Resilient { get; set; }
    }
}
