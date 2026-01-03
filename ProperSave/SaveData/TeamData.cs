using ProperSave.Utils;
using RoR2;
using System;
using System.Runtime.Serialization;

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
            var data = new TeamData();
            data.experience = context.Reader.ReadInt64();

            return data;
        }

        internal void Write(WriterContext context)
        {
            context.Writer.Write(experience);
        }
    }
}
