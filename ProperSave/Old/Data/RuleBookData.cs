using RoR2;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Old.Data
{
    public class RuleBookData
    {
        [DataMember(Name = "rv")]
        public byte[] ruleValues;

        internal ProperSave.Data.RuleBookData Migrate()
        {
            return new ProperSave.Data.RuleBookData
            {
                ruleValues = ruleValues
                    .Select((v, i) => new ProperSave.Data.RuleValueData
                    {
                        index = i,
                        value = v,
                    })
                    .ToList(),
            };
        }
    }
}
