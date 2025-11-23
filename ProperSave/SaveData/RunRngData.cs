using ProperSave.Data;
using RoR2;
using System.Runtime.Serialization;

namespace ProperSave.SaveData
{
    public class RunRngData
    {
        [DataMember(Name = "rr")]
        public RngData runRng;
        [DataMember(Name = "nsr")]
        public RngData nextStageRng;
        [DataMember(Name = "srg")]
        public RngData stageRngGenerator;
        [DataMember(Name = "lrg")]
        public RngData loopRngGenerator;

        internal RunRngData(Run run)
        {
            runRng = new RngData(run.runRNG);
            nextStageRng = new RngData(run.nextStageRng);
            stageRngGenerator = new RngData(run.stageRngGenerator);
            loopRngGenerator = new RngData(run.loopRngGenerator);
        }

        internal void LoadData(Run run)
        {
            runRng.LoadDataOut(out run.runRNG);
            nextStageRng.LoadDataOut(out run.nextStageRng);
            stageRngGenerator.LoadDataOut(out run.stageRngGenerator);
            loopRngGenerator.LoadDataOut(out run.loopRngGenerator);
        }
    }
}
