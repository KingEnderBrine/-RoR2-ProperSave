using ProperSave.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProperSave.SaveData.Runs
{
    public interface ITypedRunData
    {
        void Load();
        void Write(WriterContext context);
    }
}
