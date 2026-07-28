using ProperSave.TinyJson;

namespace ProperSave.Data
{
    public class ModdedData
    {
        public string ObjectType { get; set; }

        [DiscoverObjectType(nameof(ObjectType))]
        public object Value { get; set; }
    }
}
