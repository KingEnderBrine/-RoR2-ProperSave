using ProperSave.Utils;
using RoR2;

namespace ProperSave.SaveData
{
    public class TeamData
    {
        public long experience;

        internal static TeamData Create()
        {
            return new TeamData
            {
                experience = (long)TeamManager.instance.GetTeamExperience(TeamIndex.Player),
            };
        }

        internal void LoadData()
        {
            TeamManager.instance.GiveTeamExperience(TeamIndex.Player, (ulong)experience);
        }

        internal static TeamData Read(ReaderContext context)
        {
            var reader = context.Reader;
            var version = context.Version;

            var data = new TeamData();
            data.experience = version > 1 ? reader.ReadPackedInt64() : reader.ReadInt64();

            return data;
        }

        internal void Write(WriterContext context)
        {
            context.Writer.WritePacked(experience);
        }
    }
}
