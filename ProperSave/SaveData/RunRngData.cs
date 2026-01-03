using ProperSave.Data;
using ProperSave.Utils;
using RoR2;
using System.Runtime.Serialization;

namespace ProperSave.SaveData
{
    public class RunRngData
    {
        public RngData runRng;
        public RngData nextStageRng;
        public RngData stageRngGenerator;
        public RngData loopRngGenerator;

        internal static RunRngData Create(Run run)
        {
            return new RunRngData
            {
                runRng = RngData.Create(run.runRNG),
                nextStageRng = RngData.Create(run.nextStageRng),
                stageRngGenerator = RngData.Create(run.stageRngGenerator),
                loopRngGenerator = RngData.Create(run.loopRngGenerator),
            };
        }

        internal void LoadData(Run run)
        {
            runRng.LoadDataOut(out run.runRNG);
            nextStageRng.LoadDataOut(out run.nextStageRng);
            stageRngGenerator.LoadDataOut(out run.stageRngGenerator);
            loopRngGenerator.LoadDataOut(out run.loopRngGenerator);
        }

        internal static RunRngData Read(ReaderContext context)
        {
            var data = new RunRngData();
            data.runRng = RngData.Read(context);
            data.nextStageRng = RngData.Read(context);
            data.stageRngGenerator = RngData.Read(context);
            data.loopRngGenerator = RngData.Read(context);

            return data;
        }

        internal void Write(WriterContext context)
        {
            runRng.Write(context);
            nextStageRng.Write(context);
            stageRngGenerator.Write(context);
            loopRngGenerator.Write(context);
        }
    }
}
