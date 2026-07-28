using ProperSave.Utils;
using RoR2;

namespace ProperSave.Data
{
    public partial class UserIDData
    {
        public ulong steam;
        public string egs;
        public byte subId;

        public static UserIDData Create(NetworkUserId userID)
        {
            var data = new UserIDData
            {
                egs = userID.strValue,
                steam = userID.value,
                subId = userID.subId,
            };

            return data;
        }

        public NetworkUserId Load()
        {
            if (steam != 0L)
            {
                return new NetworkUserId(steam, subId);
            }
            if (egs != null)
            {
                return new NetworkUserId(egs, subId);
            }

            return default;
        }

        internal static UserIDData Read(ReaderContext context)
        {
            var data = new UserIDData();
            var reader = context.Reader;

            var isSteam = reader.ReadBoolean();
            if (isSteam)
            {
                data.steam = reader.ReadUInt64();
            }
            else
            {
                data.egs = reader.ReadString();
            }

            data.subId = reader.ReadByte();
            
            return data;
        }

        internal void Write(WriterContext context)
        {
            var writer = context.Writer;
            if (egs == null)
            {
                writer.Write(true);
                writer.Write(steam);
            }
            else
            {
                writer.Write(false);
                writer.Write(egs);
            }

            writer.Write(subId);
        }
    }
}
