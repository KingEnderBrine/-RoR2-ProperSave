using RoR2;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ProperSave
{
    internal class LostNetworkUser
    {
        private static readonly ConditionalWeakTable<CharacterMaster, LostNetworkUser> lostUsers = new ConditionalWeakTable<CharacterMaster, LostNetworkUser>();

        public uint lunarCoins;
        public CSteamID userID;

        public static bool TryGetUser(CharacterMaster master, out LostNetworkUser lostUser)
        {
            if (!master || !lostUsers.TryGetValue(master, out lostUser))
            {
                lostUser = null;
                return false;
            }

            return true;
        }

        public static void Subscribe()
        {
            NetworkUser.onNetworkUserLost += OnNetworkUserLost;
        }

        public static void Unsubscribe()
        {
            NetworkUser.onNetworkUserLost -= OnNetworkUserLost;
        }

        private static void OnNetworkUserLost(NetworkUser networkUser)
        {
            if (networkUser.master)
            {
                lostUsers.Add(networkUser.master, new LostNetworkUser
                {
                    lunarCoins = networkUser.lunarCoins,
                    userID = networkUser.Network_id.steamId
                });
            }
        }
    }
}
