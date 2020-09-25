using ProperSave.TinyJson;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class ModdedData
    {
        [DataMember(Name = "ot", Order = 1)]
        public string ObjectType { get; set; }

        [DataMember(Name = "v", Order = 2)]
        [ObjectTypeFromProperty(nameof(ObjectType))]
        public object Value { get; set; }
    }
}
