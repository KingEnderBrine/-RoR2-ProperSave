using ProperSave.Utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class RuleBookData
    {
        public List<RuleValueData> ruleValues = new List<RuleValueData>();

        public static RuleBookData Create(RuleBook ruleBook)
        {
            var data = new RuleBookData();

            for (var i = 0; i < ruleBook.ruleValues.Length; i++)
            {
                var ruleDef = RuleCatalog.GetRuleDef(i);
                if (ruleDef.defaultChoiceIndex == ruleBook.ruleValues[i])
                {
                    continue;
                }
                data.ruleValues.Add(new RuleValueData
                {
                    index = ruleDef.globalIndex,
                    value = ruleBook.ruleValues[i]
                });
            }

            return data;
        }

        public RuleBook Load()
        {
            var ruleBook = new RuleBook();
            foreach (var ruleValue in ruleValues)
            {
                var ruleIndex = ruleValue.index;
                if (ruleIndex == -1)
                {
                    continue;
                }
                ruleBook.ruleValues[ruleIndex] = ruleValue.value;
            }

            return ruleBook;
        }

        internal static RuleBookData Read(ReaderContext context)
        {
            var data = new RuleBookData();
            var reader = context.Reader;

            var ruleValuesCount = reader.ReadInt32();
            data.ruleValues = new List<RuleValueData>(ruleValuesCount);
            for (var i = 0; i < ruleValuesCount; i++)
            {
                data.ruleValues.Add(RuleValueData.Read(context));
            }

            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;

            writer.Write(ruleValues.Count);
            for (var i = 0; i < ruleValues.Count; i++)
            {
                ruleValues[i].Write(context);
            }
        }
    }
}
