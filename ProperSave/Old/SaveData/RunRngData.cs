using ProperSave.Old.Data;
using RoR2;
using System.Runtime.Serialization;

namespace ProperSave.Old.SaveData
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

        internal ProperSave.SaveData.RunRngData Migrate()
        {
            return new ProperSave.SaveData.RunRngData
            {
                runRng = runRng.Migrate(),
                nextStageRng = nextStageRng.Migrate(),
                stageRngGenerator = stageRngGenerator.Migrate(),
                loopRngGenerator = loopRngGenerator.Migrate(),
            };
        }
    }
}
