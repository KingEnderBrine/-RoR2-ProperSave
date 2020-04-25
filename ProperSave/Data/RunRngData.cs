using R2API.Utils;
using RoR2;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class RunRngData
    {
        [DataMember(Name = "rr")]
        public RngData runRng;
        [DataMember(Name = "nsr")]
        public RngData nextStageRng;
        [DataMember(Name = "srg")]
        public RngData stageRngGenerator;

        public RunRngData(Run run)
        {
            runRng = new RngData(run.runRNG);
            nextStageRng = new RngData(run.nextStageRng);
            stageRngGenerator = new RngData(run.stageRngGenerator);
        }

        public void LoadData(Run run)
        {
            runRng.LoadData(out run.runRNG);
            nextStageRng.LoadData(out run.nextStageRng);
            stageRngGenerator.LoadData(out run.stageRngGenerator);
        }

        public class RngData
        {
            [DataMember(Name = "s0")]
            public ulong state0;
            [DataMember(Name = "s1")]
            public ulong state1;

            public RngData(Xoroshiro128Plus rng)
            {
                state0 = rng.GetFieldValue<ulong>("state0");
                state1 = rng.GetFieldValue<ulong>("state1");
            }

            public void LoadData(out Xoroshiro128Plus rng)
            {
                rng = FormatterServices.GetUninitializedObject(typeof(Xoroshiro128Plus)) as Xoroshiro128Plus;
                rng.SetFieldValue("state0", state0);
                rng.SetFieldValue("state1", state1);
            }
        }
    }
}
