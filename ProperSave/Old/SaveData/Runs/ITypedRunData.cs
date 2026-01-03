using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProperSave.Old.SaveData.Runs
{
    public interface ITypedRunData
    {
        ProperSave.SaveData.Runs.ITypedRunData Migrate();
    }
}
