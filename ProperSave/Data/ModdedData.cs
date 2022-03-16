using ProperSave.TinyJson;
using System.Runtime.Serialization;

namespace ProperSave.Data
{
    public class ModdedData
    {
        [DataMember(Name = "ot")]
        public string ObjectType { get; set; }

        [DataMember(Name = "v")]
        [DiscoverObjectType(nameof(ObjectType))]
        public object Value { get; set; }
    }
}
