using ProperSave.Old.Data;
using RoR2;
using RoR2.Stats;
using System.Linq;
using System.Runtime.Serialization;

namespace ProperSave.Old.SaveData
{
    public class PlayerData
    {
        [DataMember(Name = "si")]
        public UserIDData userId;

        [DataMember(Name = "sf")]
        public string[] statsFields;
        [DataMember(Name = "su")]
        public int[] statsUnlockables;

        [DataMember(Name = "lc")]
        public uint lunarCoins;

        [DataMember(Name = "lccm")]
        public float lunarCoinChanceMultiplier;

        [DataMember(Name = "pbi")]
        public int preferredBodyIndex;

        [DataMember(Name = "m")]
        public CharacterMasterData master;

        internal ProperSave.SaveData.PlayerData Migrate()
        {
            return new ProperSave.SaveData.PlayerData
            {
                userId = userId.Migrate(),
                master = master.Migrate(),
                lunarCoinChanceMultiplier = lunarCoinChanceMultiplier,
                lunarCoins = lunarCoins,
                preferredBodyIndex = (BodyIndex)preferredBodyIndex,
                statsFields = statsFields
                    .Select((f, i) =>
                    {
                        var field = new StatField
                        {
                            statDef = StatDef.allStatDefs[i],
                        };
                        field.SetFromString(f);
                        if (field.IsDefault())
                        {
                            return null;
                        }

                        return new ProperSave.Data.StatFieldData
                        {
                            index = i,
                            value = field.ulongValue,
                        };
                    })
                    .Where(d => d != null)
                    .ToList(),
                statsUnlockables = statsUnlockables.Select(u => (UnlockableIndex)u).ToList(),
            };
        }
    }
}
