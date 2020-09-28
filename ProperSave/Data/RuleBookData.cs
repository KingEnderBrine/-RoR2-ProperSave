using RoR2;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class RuleBookData
    {
        [DataMember(Name = "rv")]
        public byte[] ruleValues;

        public RuleBookData(RuleBook ruleBook)
        {
            ruleValues = ruleBook.ruleValues.ToArray();
        }

        public RuleBook Load()
        {
            return new RuleBook { ruleValues = ruleValues.ToArray() };
        }
    }
}
