using System;
using System.Collections.Generic;
using System.IO;
using ProperSave.Data;

namespace ProperSave.Utils
{
    public class WriterContext
    {
        public BinaryWriter Writer { get; set; }
        public List<string> SharedStrings { get; } = new List<string>();
        public Dictionary<Type, TypeData> Types { get; } = new Dictionary<Type, TypeData>();
        public Dictionary<object, (int index, TypeData typeData)> Objects { get; } = new Dictionary<object, (int index, TypeData typeData)>();
        public Queue<object> ObjectsQueue { get; } = new Queue<object>();
        public bool Resilient { get; set; }
    }
}
