using ProperSave.Utils;

namespace ProperSave.SaveData.Runs
{
    public interface ITypedRunData
    {
        void Load();
        void Write(WriterContext context);
    }
}
