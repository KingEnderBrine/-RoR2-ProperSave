using ProperSave.Data;
using ProperSave.Utils;
using RoR2;

namespace ProperSave.SaveData
{
    public class RunRngData
    {
        public RngData runRng;
        public RngData nextStageRng;
        public RngData stageRngGenerator;
        public RngData loopRngGenerator;
        public RngData rerollStageRng;

        internal static RunRngData Create(Run run)
        {
            return new RunRngData
            {
                runRng = RngData.Create(run.runRNG),
                nextStageRng = RngData.Create(run.nextStageRng),
                stageRngGenerator = RngData.Create(run.stageRngGenerator),
                loopRngGenerator = RngData.Create(run.loopRngGenerator),
                rerollStageRng = RngData.Create(run.rerollStageRng),
            };
        }

        internal void LoadData(Run run)
        {
            runRng.LoadDataOut(out run.runRNG);
            nextStageRng.LoadDataOut(out run.nextStageRng);
            stageRngGenerator.LoadDataOut(out run.stageRngGenerator);
            loopRngGenerator.LoadDataOut(out run.loopRngGenerator);
            rerollStageRng.LoadDataOut(out run.rerollStageRng);
        }

        internal static RunRngData Read(ReaderContext context)
        {
            var version = context.Version;
            var data = new RunRngData();
            data.runRng = RngData.Read(context);
            data.nextStageRng = RngData.Read(context);
            data.stageRngGenerator = RngData.Read(context);
            data.loopRngGenerator = RngData.Read(context);
            if (version > 2)
            {
                data.rerollStageRng = RngData.Read(context);
            }
            else
            {
                data.rerollStageRng = new RngData();
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            runRng.Write(context);
            nextStageRng.Write(context);
            stageRngGenerator.Write(context);
            loopRngGenerator.Write(context);
            rerollStageRng.Write(context);
        }
    }
}
